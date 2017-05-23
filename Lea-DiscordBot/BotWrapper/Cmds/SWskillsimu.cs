using Discord.WebSocket;
using System.Threading.Tasks;
using System.Text;
using Discord;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    public static class SWskillsimu
    {
        public static async Task ProcessMessage(DiscordSocketClient client, SocketMessage message)
        {
            await message.Channel.SendMessageAsync("https://swskillsim.leayal.tk/");

            /*var dmChannel = await message.Author.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync(sb.ToString(), false, ebuilder.Build());
            await dmChannel.CloseAsync();//*/
        }
    }
}
