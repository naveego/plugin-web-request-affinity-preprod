using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginWebRequest.API.Factory;
using PluginWebRequest.API.Write;
using PluginWebRequest.DataContracts;
using PluginWebRequest.Helper;

namespace PluginWebRequest.Plugin
{
    public class Plugin : Publisher.PublisherBase
    {
        private readonly ServerStatus _server;
        private TaskCompletionSource<bool> _tcs;
        private readonly IApiClientFactory _apiClientFactory;
        private IApiClient _apiClient;

        public Plugin(HttpClient client = null)
        {
            _apiClientFactory = new ApiClientFactory(client ?? new HttpClient());
            _server = new ServerStatus
            {
                Connected = false,
                WriteConfigured = false
            };
        }
        
                /// <summary>
        /// Creates an authorization url for oauth requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BeginOAuthFlowResponse> BeginOAuthFlow(BeginOAuthFlowRequest request,
            ServerCallContext context)
        {
            Logger.Info("Getting Auth URL...");

            // params for auth url
            var clientId = request.Configuration.ClientId;
            var responseType = "code";
            var redirectUrl = request.RedirectUrl;
            var scope = "offline_access";
            var state = new Guid().ToString();

            // build auth url
            var authUrl =
                $"https://aon-asc.oktapreview.com/oauth2/aus11pjxv9gRKnNDV0h8/v1/authorize?client_id={clientId}&response_type={responseType}&redirect_uri={redirectUrl}&scope={scope}&state={state}";

            // return auth url
            var oAuthResponse = new BeginOAuthFlowResponse
            {
                AuthorizationUrl = authUrl
            };

            Logger.Info($"Created Auth URL: {authUrl}");

            return Task.FromResult(oAuthResponse);
        }

        /// <summary>
        /// Gets auth token and refresh tokens from auth code
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<CompleteOAuthFlowResponse> CompleteOAuthFlow(CompleteOAuthFlowRequest request,
            ServerCallContext context)
        {
            Logger.Info("Getting Auth and Refresh Token...");
            Logger.Info($"Redirect url: {request.RedirectUrl}");

            // get code from redirect url
            string code;
            var uri = new Uri(request.RedirectUrl);

            try
            {
                code = HttpUtility.UrlDecode(HttpUtility.ParseQueryString(uri.Query).Get("code"));
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                throw;
            }

            // token url parameters
            var redirectUrl = String.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Authority,
                uri.AbsolutePath);
            var clientId = request.Configuration.ClientId;
            var clientSecret = request.Configuration.ClientSecret;
            var grantType = "authorization_code";

            // build token url
            var tokenUrl = "https://aon-asc.oktapreview.com/oauth2/aus11pjxv9gRKnNDV0h8/v1/token";

            // build form data request
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", grantType),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUrl),
                new KeyValuePair<string, string>("code", code)
            };

            var body = new FormUrlEncodedContent(formData);

            // get tokens
            var oAuthState = new OAuthState();
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(tokenUrl, body);

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Info(await response.Content.ReadAsStringAsync());
                    response.EnsureSuccessStatusCode();
                }

                var content = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());

                oAuthState.AuthToken = content.AccessToken;
                oAuthState.RefreshToken = content.RefreshToken;
                oAuthState.Config = JsonConvert.SerializeObject(new OAuthConfig
                {
                    RedirectUri = redirectUrl
                });

                if (string.IsNullOrWhiteSpace(oAuthState.RefreshToken))
                {
                    throw new Exception("Response did not contain a refresh token");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                throw;
            }

            // return oauth state json
            var oAuthResponse = new CompleteOAuthFlowResponse
            {
                OauthStateJson = JsonConvert.SerializeObject(oAuthState)
            };

            Logger.Info("Got Auth Token and Refresh Token");

            return oAuthResponse;
        }

        /// <summary>
        /// Configures the plugin
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ConfigureResponse> Configure(ConfigureRequest request, ServerCallContext context)
        {
            Logger.Debug("Got configure request");
            Logger.Debug(JsonConvert.SerializeObject(request, Formatting.Indented));

            // ensure all directories are created
            Directory.CreateDirectory(request.TemporaryDirectory);
            Directory.CreateDirectory(request.PermanentDirectory);
            Directory.CreateDirectory(request.LogDirectory);

            // configure logger
            Logger.SetLogLevel(request.LogLevel);
            Logger.Init(request.LogDirectory);

            _server.Config = request;

            return Task.FromResult(new ConfigureResponse());
        }

        /// <summary>
        /// Establishes a connection
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>A message indicating connection success</returns>
        public override async Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            // for setting the log level
            // Logger.SetLogLevel(Logger.LogLevel.Debug);

            Logger.SetLogPrefix("connect");

            OAuthState oAuthState;
            OAuthConfig oAuthConfig;
            try
            {
                oAuthState = JsonConvert.DeserializeObject<OAuthState>(request.OauthStateJson);
                oAuthConfig = JsonConvert.DeserializeObject<OAuthConfig>(oAuthState.Config);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = e.Message,
                    SettingsError = ""
                };
            }

            var settings = new Settings
            {
                ClientId = request.OauthConfiguration.ClientId,
                ClientSecret = request.OauthConfiguration.ClientSecret,
                RefreshToken = oAuthState.RefreshToken,
                RedirectUrl = oAuthConfig.RedirectUri
            };

            // validate settings passed in
            try
            {
                _server.Settings = settings;
                _server.Settings.Validate();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                };
            }
            
            // get api client
            try
            {
                _apiClient = _apiClientFactory.CreateApiClient(_server.Settings);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                };
            }

            // test cluster factory
            try
            {
                await _apiClient.TestConnection();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);

                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = e.Message,
                    OauthError = "",
                    SettingsError = ""
                };
            }
            
            _server.Connected = true;

            return new ConnectResponse
            {
                OauthStateJson = request.OauthStateJson,
                ConnectionError = "",
                OauthError = "",
                SettingsError = ""
            };
        }

        public override async Task ConnectSession(ConnectRequest request,
            IServerStreamWriter<ConnectResponse> responseStream, ServerCallContext context)
        {
            Logger.SetLogPrefix("connect_session");
            Logger.Info("Connecting session...");

            // create task to wait for disconnect to be called
            _tcs?.SetResult(true);
            _tcs = new TaskCompletionSource<bool>();

            // call connect method
            var response = await Connect(request, context);

            await responseStream.WriteAsync(response);

            Logger.Info("Session connected.");

            // wait for disconnect to be called
            await _tcs.Task;
        }


        /// <summary>
        /// Discovers schemas
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Discovered schemas</returns>
        public override async Task<DiscoverSchemasResponse> DiscoverSchemas(DiscoverSchemasRequest request,
            ServerCallContext context)
        {
            Logger.SetLogPrefix("discover");
            Logger.Info("Skipping discovering schemas - NOT SUPPORTED");

            return new DiscoverSchemasResponse();
        }

        /// <summary>
        /// Publishes a stream of data for a given schema
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ReadStream(ReadRequest request, IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            Logger.SetLogPrefix(request.JobId);
            Logger.Info("Skipping reading records - NOT SUPPORTED");
        }

        /// <summary>
        /// Creates a form and handles form updates for write backs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ConfigureWriteResponse> ConfigureWrite(ConfigureWriteRequest request, ServerCallContext context)
        {
            Logger.SetLogPrefix("configure write");
            Logger.Info("Configuring write...");
            
            var schemaJson = Write.GetSchemaJson();
            var uiJson = Write.GetUIJson();

            // if first call 
            if (string.IsNullOrWhiteSpace(request.Form.DataJson) || request.Form.DataJson == "{}")
            {
                return new ConfigureWriteResponse
                {
                    Form = new ConfigurationFormResponse
                    {
                        DataJson = "",
                        DataErrorsJson = "",
                        Errors = { },
                        SchemaJson = schemaJson,
                        UiJson = uiJson,
                        StateJson = ""
                    },
                    Schema = null
                };
            }

            try
            {
                // get form data
                var formData = JsonConvert.DeserializeObject<ConfigureWriteFormData>(request.Form.DataJson);
                // base schema to return
                var schema = await Write.GetSchemaForSettingsAsync(formData);

                return new ConfigureWriteResponse
                {
                    Form = new ConfigurationFormResponse
                    {
                        DataJson = request.Form.DataJson,
                        Errors = { },
                        SchemaJson = schemaJson,
                        UiJson = uiJson,
                        StateJson = request.Form.StateJson
                    },
                    Schema = schema
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                return new ConfigureWriteResponse
                {
                    Form = new ConfigurationFormResponse
                    {
                        DataJson = request.Form.DataJson,
                        Errors = {e.Message},
                        SchemaJson = schemaJson,
                        UiJson = uiJson,
                        StateJson = request.Form.StateJson
                    },
                    Schema = null
                };
            }
        }

        /// <summary>
        /// Prepares writeback settings to send request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<PrepareWriteResponse> PrepareWrite(PrepareWriteRequest request,
            ServerCallContext context)
        {
            Logger.SetLogPrefix(request.DataVersions.JobId);
            Logger.Info("Preparing write...");
            _server.WriteConfigured = false;

            _server.WriteSettings = new WriteSettings
            {
                CommitSLA = request.CommitSlaSeconds,
                Schema = request.Schema,
                Replication = request.Replication,
                DataVersions = request.DataVersions,
            };

            var settings = JsonConvert.DeserializeObject<ConfigureWriteFormData>(request.Schema.PublisherMetaJson);

            _apiClient = _apiClientFactory.CreateApiClient(settings, _server.Settings);

            _server.WriteConfigured = true;

            Logger.Debug(JsonConvert.SerializeObject(_server.WriteSettings, Formatting.Indented));
            Logger.Info("Write prepared.");
            return new PrepareWriteResponse();
        }

        /// <summary>
        /// Writes records
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task WriteStream(IAsyncStreamReader<Record> requestStream,
            IServerStreamWriter<RecordAck> responseStream, ServerCallContext context)
        {
            try
            {
                Logger.Info($"Sending web requests...");

                var schema = _server.WriteSettings.Schema;
                var inCount = 0;

                // get next record to publish while connected and configured
                while (await requestStream.MoveNext(context.CancellationToken) && _server.Connected &&
                       _server.WriteConfigured)
                {
                    var record = requestStream.Current;
                    inCount++;

                    Logger.Debug($"Got record: {record.DataJson}");

                    if (_server.WriteSettings.IsReplication())
                    {
                        throw new System.NotSupportedException();
                    }
                    else
                    {
                        // send record to source system
                        // add await for unit testing 
                        // removed to allow multiple to run at the same time
                        await Task.Run(async () =>
                                await Write.WriteRecordAsync(_apiClient, schema, record, responseStream),
                            context.CancellationToken);
                    }
                }

                Logger.Info($"Wrote {inCount} records.");
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
            }
        }

        /// <summary>
        /// Handles disconnect requests from the agent
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context)
        {
            // clear connection
            _server.Connected = false;
            _server.Settings = null;

            // alert connection session to close
            if (_tcs != null)
            {
                _tcs.SetResult(true);
                _tcs = null;
            }

            Logger.Info("Disconnected");
            return Task.FromResult(new DisconnectResponse());
        }
    }
}