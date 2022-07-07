using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Interaction
{
    public class InteractionHandler
    {
        private readonly IServiceProvider _service;
        private readonly InteractionService _interaction;
        private readonly DiscordSocketClient _client;

        public InteractionHandler(IServiceProvider service, InteractionService interaction, DiscordSocketClient client)
        {
            _service = service;
            _interaction = interaction;
            _client = client;
        }

        public async Task InitializeAsync()
        {
            await _interaction.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _service);
            _client.InteractionCreated += (cmd) =>
            {
                Task.Run(() => HandleInteraction(cmd));
                return Task.CompletedTask;
            };
            _client.SlashCommandExecuted += (cmd) =>
            {
                Task.Run(() => HandleExecutedCommand(cmd));
                return Task.CompletedTask;
            };
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, arg);
                await _interaction.ExecuteCommandAsync(ctx, _service);
            }
            catch(Exception ex)
            {
                await Logger.Log.Error(ex.ToString());

                if(arg.Type == InteractionType.ApplicationCommand)
                {
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }

        private async Task HandleExecutedCommand(SocketSlashCommand cmd)
        {
            await Logger.Log.Info($"{cmd.User.Username} executed {cmd.Data.Name}");
        }
    }
}
