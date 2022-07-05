using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using Logger.Database.Table;

namespace Logger.Interaction.Admin
{
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("admin", "admin only command")]
    public class Admin : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _service;
        private readonly InteractionService _interaction;

        public Admin(DiscordSocketClient client, IServiceProvider service, InteractionService interaction)
        {
            _client = client;
            _service = service;
            _interaction = interaction;
        }

        [SlashCommand("setchannel", "set logging and report channel")]
        public async Task SetLogAsync([Summary(description: "logging channel")] IChannel logChannel, [Summary(description: "report channel")] IChannel reportChannel)
        {
            GuildInfo info = new GuildInfo();
            info.LogChannelId = logChannel.Id;
            info.ReportChannelId = reportChannel.Id;
            ulong key = Context.Guild.Id;
            await new RedisUtility(Program.RedisDb).DbSetAsync<GuildInfo>(key.ToString(), info);
            await RespondAsync("Done", ephemeral: true);
        }
    }
}
