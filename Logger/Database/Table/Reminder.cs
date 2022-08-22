using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Database.Table
{
	public class Reminder : DbEntity
	{
		public ulong UserId { get; set; }
		public ulong GuildId { get; set; }
		public ulong ChannelId { get; set; }
		public DateTime EndTime { get; set; }
		public int Duration { get; set; }
		public string Content { get; set; } = String.Empty;
	}
}
