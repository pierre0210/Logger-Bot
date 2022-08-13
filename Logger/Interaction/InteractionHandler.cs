using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// https://discordnet.dev/guides/text_commands/dependency-injection.html

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
			_interaction.SlashCommandExecuted += HandleExecutedCommand;
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
		
		private async Task HandleExecutedCommand(SlashCommandInfo cmd, Discord.IInteractionContext context, IResult result)
		{
			await Logger.Log.Info($"{context.User.Username} executed {cmd.Name}");
			if(!result.IsSuccess)
			{
				switch(result.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						await context.Interaction.RespondAsync($"權限不符: {result.ErrorReason}", ephemeral: true);
						break;
					case InteractionCommandError.UnknownCommand:
						await context.Interaction.RespondAsync("未知指令", ephemeral: true);
						break;
					case InteractionCommandError.BadArgs:
						await context.Interaction.RespondAsync("參數錯誤", ephemeral: true);
						break;
					case InteractionCommandError.Exception:
						await context.Interaction.RespondAsync($"指令執行錯誤(請回報給機器人開發者): {result.ErrorReason}", ephemeral: true);
						break;
					case InteractionCommandError.Unsuccessful:
						await context.Interaction.RespondAsync("指令無法執行", ephemeral: true);
						break;
					default:
						break;
				}
			}
		}
	}
}
