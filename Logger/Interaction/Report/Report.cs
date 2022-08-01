using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using Logger.Database;
using Logger.Database.Table;

namespace Logger.Interaction.Report
{
    public class Report : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;

        public Report(DiscordSocketClient client)
        {
            _client = client;
        }

        [EnabledInDm(false)]
        [SlashCommand("report", "檢舉指令")]
        public async Task ReportAsync()
        {
            RedisUtility utility = new RedisUtility(Program.RedisDb);
            bool guildExist = await utility.DbExistsAsync<GuildInfo>($"{Context.Guild.Id}");
            var info = await utility.DbGetAsync<GuildInfo>($"{Context.Guild.Id}");
                
            if(!guildExist)
            {
                await RespondAsync(text: "此伺服器未設定檢舉功能!", ephemeral: true);
            }
            else if(!info.ReportEnable)
            {
                await RespondAsync(text: "此伺服器未開啟檢舉功能!", ephemeral: true);
            }
            else
            {
                var mb = new ModalBuilder()
                    .WithTitle("檢舉表單")
                    .WithCustomId("report_form")
                    .AddTextInput("被檢舉人名稱：", "report_user", TextInputStyle.Short, placeholder: "Troll#9487", required: true, maxLength: 256)
                    .AddTextInput("被檢舉人ID：", "report_id", TextInputStyle.Short, placeholder: "123456789012345678", required: true, maxLength: 1024)
                    .AddTextInput("訊息連結或截圖網址：", "report_url", TextInputStyle.Short, placeholder: "https://example.com/image.jpg", required: true, maxLength: 1024)
                    .AddTextInput("檢舉事由：", "report_detail", TextInputStyle.Paragraph, required: true, maxLength: 1024);

                await RespondWithModalAsync(mb.Build());
            }
        }

        [EnabledInDm(false)]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("enable-report", "開啟檢舉功能")]
        public async Task EnableReportAsync()
        {
            RedisUtility utility = new RedisUtility(Program.RedisDb);
            bool guildExist = await utility.DbExistsAsync<GuildInfo>($"{Context.Guild.Id}");
            var info = await utility.DbGetAsync<GuildInfo>($"{Context.Guild.Id}");

            if (!guildExist)
            {
                await RespondAsync(text: "此伺服器未設定檢舉功能!", ephemeral: true);
            }
            else if (info.ReportChannelId == ulong.MinValue)
            {
                await RespondAsync(text: "此伺服器未設定檢舉頻道!", ephemeral: true);
            }
            else
            {
                using (var db = new SQLiteContext())
                {
                    var row = db.GuildInfos.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();
                    row.ReportEnable = true;
                    info.ReportEnable = true;
                    await utility.DbDelAsync<GuildInfo>($"{Context.Guild.Id}");
                    await utility.DbSetAsync($"{Context.Guild.Id}", info);
                    await RespondAsync(text: "開啟檢舉功能");
                    db.SaveChanges();
                }
            }
        }

        [EnabledInDm(false)]
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [SlashCommand("disable-report", "關閉檢舉功能")]
        public async Task DisableReportAsync()
        {
            RedisUtility utility = new RedisUtility(Program.RedisDb);
            bool guildExist = await utility.DbExistsAsync<GuildInfo>($"{Context.Guild.Id}");
            var info = await utility.DbGetAsync<GuildInfo>($"{Context.Guild.Id}");

            if (!guildExist)
            {
                await RespondAsync(text: "此伺服器未設定檢舉功能!", ephemeral: true);
            }
            else if (info.ReportChannelId == ulong.MinValue)
            {
                await RespondAsync(text: "此伺服器未設定檢舉頻道!", ephemeral: true);
            }
            else
            {
                using(var db = new SQLiteContext())
                {
                    var row = db.GuildInfos.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();
                    row.ReportEnable = false;
                    info.ReportEnable = true;
                    await utility.DbDelAsync<GuildInfo>($"{Context.Guild.Id}");
                    await utility.DbSetAsync($"{Context.Guild.Id}", info);
                    await RespondAsync(text: "關閉檢舉功能");
                    db.SaveChanges();
                }
            }
        }
    }
}
