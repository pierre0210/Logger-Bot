using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Database.Table
{
    public class GuildInfo : DbEntity
    {
        public ulong GuildId { get; set; }
        public ulong LogChannelId { get; set; }
        public ulong ReportChannelId { get; set; }
        public bool MessageLog { get; set; } = false;
    }
}
