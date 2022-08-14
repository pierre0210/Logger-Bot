using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

//reference: https://jed1978.github.io/2018/05/11/Redis-Programming-CSharp-Basic-1.html

namespace Logger
{
	public sealed class RedisConnection
	{
		private static string _settingOption;
		public readonly ConnectionMultiplexer multiplexer;

		private static Lazy<RedisConnection> lazy = new Lazy<RedisConnection>(() => {
			if (String.IsNullOrEmpty(_settingOption)) throw new InvalidOperationException("Please call Init() first.");
			return new RedisConnection();
		});

		public static RedisConnection Instance
		{
			get { return lazy.Value; }
		}

		private RedisConnection()
		{
			multiplexer = ConnectionMultiplexer.Connect(_settingOption);
		}

		public static void Init(string settingOption)
		{
			_settingOption = settingOption;
		}
	}
}
