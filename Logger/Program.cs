using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Logger.Interaction;
using Logger.Interaction.Report;

namespace Logger
{
    public class Program
    {
        private static DiscordSocketClient _client;
        public static bool isBotOn = false;
        static void Main(string[] args)
        {
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
            _client.Ready += async () =>
            {
                try
                {
                    InteractionService interactionService = iService.GetService<InteractionService>();
#if DEBUG
                    ulong guildId = botConfig.OwnerGuild;
                    await interactionService.RegisterCommandsToGuildAsync(guildId);
                    Log.Info("Registered guild command!");
#else
                    await interactionService.RegisterCommandsGloballyAsync();
                    Log.Info("Registered global command!");
#endif                
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to register command!");
                    Log.Error(ex.ToString());
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
