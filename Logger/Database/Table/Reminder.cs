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
        public string TimeStamp { get; set; } = String.Empty;
        public int Duration { get; set; }
        public string Content { get; set; } = String.Empty;
    }
}
