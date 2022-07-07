using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Logger.Database;
using Logger.Interaction;
using Logger.Interaction.Report;

namespace Logger
{
    public class Program
    {
        private static DiscordSocketClient _client;
        public static bool isBotOn = false;
        public static ConnectionMultiplexer Redis { get; set; }
        public static IDatabase RedisDb { get; set; }
        public static List<ulong> BlackList { get; set; } = new List<ulong>();
        static void Main(string[] args)
        {
            using(var db = new SQLiteContext())
            {
                db.Database.EnsureCreated();
                Log.Info("SQLite database created!");
            }
            /*
            try
            {
                RedisConnection.Init("localhost");
                Redis = RedisConnection.Instance.multiplexer;
                RedisDb = Redis.GetDatabase();
                Log.Info("Redis connected!");
            }
            catch(Exception ex)
            {
                Log.Error("Failed to connect to Redis!");
                Log.Error(ex.Message);
                Environment.Exit(1);
            }
            */
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
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
            _client.MessageReceived += async (msg) =>
            {
                if (BlackList.Contains(msg.Author.Id))
                {
                    await msg.DeleteAsync();
                }
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
                    Log.Info("Registered global command!");
#endif                
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
    }
}
