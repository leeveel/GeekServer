using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System.Text;

namespace Geek.Server
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

			logConfig = Settings.Ins.IsDebug ? "debug" : "release";
			return logConfig;
		}

		protected override void Append(StringBuilder builder, LogEventInfo logEvent)
		{
			builder.Append(GetBuildConfig());
		}
	}
}