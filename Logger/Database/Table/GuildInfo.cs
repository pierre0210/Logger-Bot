using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Database.Table
{
	public class GuildInfo : DbEntity
	{
		public ulong GuildId { get; set; } = ulong.MinValue;
		public ulong LogChannelId { get; set; } = ulong.MinValue;
		public ulong ReportChannelId { get; set; } = ulong.MinValue;
		public bool MessageLog { get; set; } = false;
		public bool ReportEnable { get; set; } = false;
	}
}
