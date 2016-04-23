using System;
using Microsoft.Owin;
using Owin;
using WedApiSeed;
using System.IO;
using Newtonsoft.Json;

[assembly: OwinStartup(typeof(WebApiSeed.Startup))]

namespace WebApiSeed
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LoadSettings();
            ConfigureAuth(app);
        }

        private static void LoadSettings()
        {
            SetupConfig.Setting = JsonConvert.DeserializeObject<Setting>
                (File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SetupConfig.json")));
        }
    }
}
