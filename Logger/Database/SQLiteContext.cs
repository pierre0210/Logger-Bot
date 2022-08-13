using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Logger.Database.Table;

namespace Logger.Database
{
	public class SQLiteContext : DbContext
	{
		public DbSet<GuildInfo> GuildInfos { get; set; }

		public string DbPath { get; }

		public SQLiteContext()
		{
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			DbPath = System.IO.Path.Join(path, "Database.db");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options.UseSqlite($"Data Source={DbPath}");
		}
	}
}
