using System;

namespace PluginWebRequest.Helper
{
    public class Settings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
        public string RedirectUrl { get; set; }
        
        
        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new Exception("the ClientId property must be set");
            }
            
            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new Exception("the ClientSecret property must be set");
            }
            
            if (string.IsNullOrWhiteSpace(RefreshToken))
            {
                throw new Exception("the RefreshToken property must be set");
            }
            
            if (string.IsNullOrWhiteSpace(RedirectUrl))
            {
                throw new Exception("the RedirectUrl property must be set");
            }
        }
    }
}