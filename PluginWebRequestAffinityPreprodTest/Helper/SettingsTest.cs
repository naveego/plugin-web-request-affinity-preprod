using System;
using PluginWebRequestAffinityPreprod.Helper;
using Xunit;

namespace PluginWebRequestTest.Helper
{
    public class SettingsTest
    {
        [Fact]
        public void ValidateValidTest()
        {
            // setup
            var settings = new Settings
            {
            };

            // act
            settings.Validate();

            // assert
        }
    }
}