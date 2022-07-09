using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Logger.Database;
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
        public async Task SetChannelAsync([Summary(description: "logging channel")] IChannel logChannel, [Summary(description: "report channel")] IChannel reportChannel)
        {
            using(var db = new SQLiteContext())
            {
                GuildInfo info = new GuildInfo();
                info.GuildId = Context.Guild.Id;
                info.LogChannelId = logChannel.Id;
                info.ReportChannelId = reportChannel.Id;

                var row = db.GuildInfos.Where(x => x.GuildId == info.GuildId).FirstOrDefault();
                if(row == null)
                {
                    db.GuildInfos.Add(info);
                    db.SaveChanges();
                }
                else
                {
                    row.LogChannelId = logChannel.Id;
                    row.ReportChannelId = reportChannel.Id;
                    db.SaveChanges();
                }
                
                //await new RedisUtility(Program.RedisDb).DbSetAsync<GuildInfo>(key.ToString(), info);
                await RespondAsync(text: "Done", ephemeral: true);
            }
        }

        [SlashCommand("pingbonk", "BONK!")]
        public async Task PingBonkAsync(IUser victim, int times, string msg = "")
        {
            await RespondAsync(text: "Start bonking!", ephemeral: true);
            string bonk = $"<@{victim.Id}> {msg}";

            for (int i = 0; i < times; i++)
            {
                await Context.Channel.SendMessageAsync(text: bonk);
            }
        }
    }
}
