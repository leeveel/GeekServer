using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geek.Server.Core.Utils;

namespace Geek.Server.RemoteBackup.Logic
{
    public class BackupSetting : BaseSetting
    {
        
        public bool AllowOpen { get; set; } = true;

        public string DBRootPath { get; set; }

        /// <summary>
        /// 备份进程本地数据库
        /// </summary>
        public string BackupDBPath { get; set; }
    }
}
