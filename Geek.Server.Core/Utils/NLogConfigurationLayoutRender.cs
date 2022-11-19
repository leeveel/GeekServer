using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Geek.Server.Core.Utils
{
    [ThreadAgnostic]
    [LayoutRenderer("NLogConfiguration")]
    public class NLogConfigurationLayoutRender : LayoutRenderer
    {
        private string logConfig;
        private string GetBuildConfig()
        {
            if (!string.IsNullOrEmpty(logConfig))
                return logConfig;

            logConfig = Settings.IsDebug ? "debug" : "release";
            return logConfig;
        }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(GetBuildConfig());
        }
    }

}