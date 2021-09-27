using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginWebRequestAffinityPreprod.API.Factory;
using PluginWebRequestAffinityPreprod.API.Utility;
using PluginWebRequestAffinityPreprod.DataContracts;

namespace PluginWebRequestAffinityPreprod.API.Write
{
    public static partial class Write
    {
        private static readonly SemaphoreSlim WriteSemaphoreSlim = new SemaphoreSlim(1, 1);

        public static async Task<string> WriteRecordAsync(IApiClient apiClient, Schema schema, Record record,
            IServerStreamWriter<RecordAck> responseStream)
        {
            // debug
            Logger.Debug($"Starting timer for {record.RecordId}");
            var timer = Stopwatch.StartNew();

            try
            {
                // debug
                Logger.Debug(JsonConvert.SerializeObject(record, Formatting.Indented));

                // semaphore
                await WriteSemaphoreSlim.WaitAsync();

                // get settings
                var settings = JsonConvert.DeserializeObject<ConfigureWriteFormData>(schema.PublisherMetaJson);
                
                // get record map
                var recordMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(record.DataJson);
                
                // get okta token
                var token = await apiClient.GetToken();
                
                // write records
                // prepare url
                var urlValues = new List<object>();
                var urlProperties = schema.Properties.Where(p => p.Id.StartsWith(Constants.UrlPropertyPrefix));
                foreach (var property in urlProperties)
                {
                    try
                    {
                        if (recordMap.ContainsKey(property.Id))
                        {
                            urlValues.Add(recordMap[property.Id].ToString());
                        }
                        else
                        {
                            urlValues.Add("");
                        }
                    }
                    catch
                    {
                        urlValues.Add("");
                    }
                }

                var url = string.Format(settings.Url, urlValues.ToArray());
                
                // apply okta token
                url = url.Replace(Constants.OktaTokenFind, token);
                
                // prepare body
                var bodyValues = new List<object>();
                var bodyProperties = schema.Properties.Where(p => p.Id.StartsWith(Constants.BodyPropertyPrefix));
                foreach (var property in bodyProperties)
                {
                    try
                    {
                        if (recordMap.ContainsKey(property.Id))
                        {
                            bodyValues.Add(recordMap[property.Id].ToString());
                        }
                        else
                        {
                            bodyValues.Add("");
                        }
                    }
                    catch
                    {
                        bodyValues.Add("");
                    }
                }

                var safeBody = Regex.Replace(settings.Body, @"\{(?!\d)", "{{");
                safeBody = Regex.Replace(safeBody, @"(?<!\d)\}", "}}");
                
                // apply okta token
                safeBody = safeBody.Replace(Constants.OktaTokenFind, token);
                
                var body = string.Format(safeBody, bodyValues.ToArray());
                var json = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8,
                    "application/json");
                
                HttpResponseMessage response = new HttpResponseMessage();
                switch (settings.Method)
                {
                    case Constants.MethodGet:
                        response = await apiClient.GetAsync(url);
                        break;
                    case Constants.MethodPost:
                        response = await apiClient.PostAsync(url, json);
                        break;
                    case Constants.MethodPut:
                        response = await apiClient.PutAsync(url, json);
                        break;
                    case Constants.MethodPatch:
                        response = await apiClient.PatchAsync(url, json);
                        break;
                    case Constants.MethodDelete:
                        response = await apiClient.DeleteAsync(url);
                        break;
                }

                // send ack
                var ack = new RecordAck
                {
                    CorrelationId = record.CorrelationId,
                    Error = ""
                };
                
                if (!response.IsSuccessStatusCode)
                {
                    ack.Error = await response.Content.ReadAsStringAsync();
                }
                
                await responseStream.WriteAsync(ack);

                timer.Stop();
                Logger.Debug($"Acknowledged Record {record.RecordId} time: {timer.ElapsedMilliseconds}");

                return ack.Error;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error writing record {e.Message}");
                // send ack
                var ack = new RecordAck
                {
                    CorrelationId = record.CorrelationId,
                    Error = e.Message
                };
                await responseStream.WriteAsync(ack);

                timer.Stop();
                Logger.Debug($"Failed Record {record.RecordId} time: {timer.ElapsedMilliseconds}");

                return e.Message;
            }
            finally
            {
                WriteSemaphoreSlim.Release();
            }
        }
    }
}