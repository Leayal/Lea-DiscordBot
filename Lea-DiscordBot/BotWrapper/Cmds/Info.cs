using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    public static class Info
    {
        public const string BotVersion = "0.0.1";
        public static async Task ProcessMessage(DiscordSocketClient client, SocketMessage message)
        {
            // Introduce myself
            EmbedBuilder ebuilder = new EmbedBuilder();
            //ebuilder.Url = "";
            ebuilder.Description = "Why would you want to know of me??? Just take it simple: I'm a bot.";
            ebuilder.ThumbnailUrl = client.CurrentUser.GetAvatarUrl();
            ebuilder.AddInlineField("Version", BotVersion);
            ebuilder.Title = "Lea-DiscordBot";
            ebuilder.AddInlineField("Language", ".NET Core/C#");
            ebuilder.Url = "https://github.com/Leayal/Lea-DiscordBot/";
            var myauthor = new EmbedAuthorBuilder();
            var authorObject = client.GetUser(164090291421184002);
            myauthor.Name = authorObject.Username;
            myauthor.IconUrl = authorObject.GetAvatarUrl();
            ebuilder.Author = myauthor;
            //ebuilder.
            await message.Channel.SendMessageAsync("", false, ebuilder.Build());
        }
    }
}
