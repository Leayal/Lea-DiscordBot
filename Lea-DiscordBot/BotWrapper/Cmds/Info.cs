using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    public static class Info
    {
        private static Embed infoCache;
        public static void CreateInfoEmbed(DiscordSocketClient client, IUser owner)
        {
            if (infoCache == null)
            {
                var authorCache = client.GetUser(164090291421184002);
                // Introduce myself
                var myauthor = new EmbedAuthorBuilder()
                {
                    Name = authorCache.Username,
                    IconUrl = authorCache.GetAvatarUrl()
                };
                EmbedBuilder ebuilder = new EmbedBuilder()
                {
                    Description = "Why would you want to know of me??? Just take it simple: I'm a bot.",
                    ThumbnailUrl = client.CurrentUser.GetAvatarUrl(),
                    Title = "Lea-DiscordBot",
                    Url = "https://github.com/Leayal/Lea-DiscordBot/",
                    Author = myauthor
                };
                ebuilder.AddInlineField("Version", "0.0.1");
                ebuilder.AddInlineField("Language", ".NET Core/C#");
                ebuilder.AddInlineField("Owner", $"<@{owner.Id.ToString()}>");
                infoCache = ebuilder.Build();
            }
        }

        public static async Task ProcessMessage(DiscordSocketClient client, SocketMessage message, IUser owner)
        {
            if (infoCache != null)
                await message.Channel.SendMessageAsync(string.Empty, false, infoCache);
        }
    }
}
