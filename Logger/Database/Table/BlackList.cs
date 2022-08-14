using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Database.Table
{
	public class BlackList: DbEntity
	{
		public ulong UserId { get; set; } = ulong.MinValue;
		public DateTime ReleaseTime { get; set; } = DateTime.MinValue;
	}
}
