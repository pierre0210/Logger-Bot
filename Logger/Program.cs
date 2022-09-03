using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Logger.Database;
using Logger.Database.Table;
using Logger.Interaction;
using Logger.Interaction.Report;
using Logger.Interaction.Logging;

namespace Logger
{
	public class Program
	{
		private static DiscordSocketClient _client;
		public static bool isBotOn = false;
		public static ConnectionMultiplexer Redis { get; set; }
		public static IDatabase RedisDb { get; set; }
		public static IServer RedisServer { get; set; }
		public static string TempFolderPath { get; set; }
		static void Main(string[] args)
		{
			TempFolderPath = Path.Join(Directory.GetCurrentDirectory(), "Temp");
			Directory.CreateDirectory(TempFolderPath);
			Log.Info($"{TempFolderPath} folder created!");

			using(var db = new SQLiteContext())
			{
				db.Database.EnsureCreated();
				Log.Info($"{db.DbPath} SQLite database created!");
			}
			
			try
			{
				RedisConnection.Init("localhost");
				Redis = RedisConnection.Instance.multiplexer;
				RedisDb = Redis.GetDatabase();
				RedisServer = Redis.GetServer(Redis.GetEndPoints().First());
				Log.Info("Redis connected!");
			}
			catch(Exception ex)
			{
				Log.Error("Failed to connect to Redis!");
				Log.Error(ex.Message);
				Environment.Exit(1);
			}
			
			new Program().MainAsync().GetAwaiter().GetResult();
		}

		public async Task MainAsync()
		{
			_client = new DiscordSocketClient(
				new DiscordSocketConfig
				{
#if DEBUG
					LogLevel = LogSeverity.Verbose,
#else
					LogLevel = LogSeverity.Verbose,
#endif
					MessageCacheSize = 500,
					GatewayIntents = GatewayIntents.AllUnprivileged
				}
			);
			BotConfig botConfig = new();
			botConfig.InitConfig();
			var interactionServices = new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton(botConfig)
				.AddSingleton(new InteractionService(_client, new InteractionServiceConfig()
				{
					AutoServiceScopes = true,
					UseCompiledLambda = true,
					EnableAutocompleteHandlers = true,
					DefaultRunMode = Discord.Interactions.RunMode.Async,
					ExitOnMissingModalField = true,
				}))
				.AddSingleton<InteractionHandler>();
			
			IServiceProvider iService = interactionServices.BuildServiceProvider();
			await iService.GetService<InteractionHandler>().InitializeAsync();

#region EventHandler
			_client.Log += Log.Msg;
			_client.ModalSubmitted += new ReportHandler(_client).SendReport;
			_client.MessageDeleted += new MessageLogHandler(_client).LogDeleteMessage;
			_client.MessageUpdated += new MessageLogHandler(_client).LogUpdateMessage;
			_client.UserVoiceStateUpdated += (user, oldState, newState) =>
			{
				Task.Run(async () => 
				{
					RedisUtility utility = new RedisUtility(RedisDb);
					if (newState.VoiceChannel != null && (newState.VoiceChannel != oldState.VoiceChannel || newState.IsMuted != oldState.IsMuted))
					{
						var guildUser = user as SocketGuildUser;
						if (await utility.DbExistsAsync<BlackList>($"{guildUser.Guild.Id}:{user.Id}"))
						{
							await guildUser.ModifyAsync(u =>
							{
								u.Mute = true;
							});
						}
						else if (guildUser.IsMuted)
						{
							await guildUser.ModifyAsync(u =>
							{
								u.Mute = false;
							});
						}
					}
				});

				return Task.CompletedTask;
			};
			_client.MessageReceived += (msg) =>
			{
				Task.Run(async () =>
				{
					RedisUtility utility = new RedisUtility(RedisDb);
					var channel = msg.Channel as SocketGuildChannel;
					if (await utility.DbExistsAsync<BlackList>($"{channel.Guild.Id}:{msg.Author.Id}"))
					{
						await msg.DeleteAsync();
					}
				});

				return Task.CompletedTask;
			};
			_client.Ready += async () =>
			{
				try
				{
					InteractionService interactionService = iService.GetService<InteractionService>();
#if DEBUG
					ulong guildId = botConfig.OwnerGuild;
					await interactionService.RegisterCommandsToGuildAsync(guildId);
					await Log.Info("Registered guild command!");
#else
					await interactionService.RegisterCommandsGloballyAsync();
					await Log.Info("Registered global command!");
#endif
					await SyncGuildInfoWithRedis();
				}
				catch (Exception ex)
				{
					await Log.Error("Failed to register command!");
					await Log.Error(ex.Message);
				}
				isBotOn = true;
			};
#endregion

			await _client.LoginAsync(TokenType.Bot, botConfig.BotToken);
			await _client.StartAsync();

			do
			{
				await Task.Delay(5000);
			}
			while (isBotOn);

			await _client.StopAsync();
		}

		private async Task SyncGuildInfoWithRedis()
		{
			using(var db = new SQLiteContext())
			{
				foreach(var item in db.GuildInfos.ToList())
				{
					RedisUtility utility = new RedisUtility(RedisDb);
					await Log.Info("Sync GuildInfo with RedisDB");
					await utility.DbSetAsync<GuildInfo>($"{item.GuildId}", item);
				}
			}
		}
	}
}
