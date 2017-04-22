using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Leayal;

namespace LeaDiscordBot.BotWrapper
{
    public class Bot : System.IDisposable
    {
        private DiscordSocketClient client;
        private char commandPrefix;
        private ulong ownerID;

        public Bot(char cmdPrefix)
        {
            System.Console.WriteLine("Creating Bot instance");
            //this.ownerID = 1;
            this.commandPrefix = cmdPrefix;
            this.client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                MessageCacheSize = 1,
                LogLevel = Discord.LogSeverity.Error,
                HandlerTimeout = 5000,
                ConnectionTimeout = 3000,
                DefaultRetryMode = Discord.RetryMode.Retry502 | Discord.RetryMode.RetryTimeouts
            });
            this.client.LoggedIn += Client_LoggedIn;
            this.client.LoggedOut += Client_LoggedOut;
            this.client.MessageReceived += Client_MessageReceived;
            this.client.Log += Client_Log;
            //this.commander = new CommandService(new CommandServiceConfig() { CaseSensitiveCommands = false });
        }

        private async Task Client_LoggedIn()
        {
            System.Console.WriteLine("Bot logged in. Ready~!");
            var myInfo = await client.GetApplicationInfoAsync();
            System.Console.WriteLine(string.Format("Owner: {0}{1}", myInfo.Owner.Username, string.IsNullOrEmpty(myInfo.Owner.Discriminator) ? string.Empty : "#" + myInfo.Owner.Discriminator));
            this.ownerID = myInfo.Owner.Id;
            await Task.Yield();
        }

        public async Task Login(string botToken)
        {
            System.Console.WriteLine("Logging in");
            await this.client.LoginAsync(TokenType.Bot, botToken);
            await this.client.StartAsync();
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
                if (message.Author != this.client.CurrentUser)
                {
                    foreach (var mentioned in message.MentionedUsers)
                        if (mentioned.Id == this.client.CurrentUser.Id)
                        {
                            await message.Channel.SendMessageAsync(string.Format("How can i help you, <@{0}>???", message.Author.Id));
                            break;
                        }
                    // Command type with command prefix ????
                    if (message.Content.Length > 1 && message.Content.IndexOf(this.commandPrefix) == 0)
                    {
                        // Remove the command prefix and start to parse the command
                        string[] splittedMsg = message.Content.Remove(0, 1).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        // Someone ordered me to do something ???
                        switch(splittedMsg[0].ToLower())
                        {
                            case "info":
                                await Cmds.Info.ProcessMessage(this.client, message);
                                break;
                            case "shutdown":
                                if (message.Author.Id == this.ownerID)
                                {
#pragma warning disable 4014
                                    this.client.LogoutAsync();
#pragma warning restore 4014
                                    await this.client.StopAsync();                                    
                                    System.Environment.Exit(0);
                                }
                                return;
                            case "error":
                                await Cmds.ErrorExplainer.ProcessMessage(this.client, message, splittedMsg);
                                break;
                            //case "wiki": let the wiki come later
                                
                            default:
                                System.Console.WriteLine(string.Format("'{0}' (ID: {1}) ordered '{2}' but this command is not existed.", message.Author.Username, message.Author.Id, splittedMsg[0]));
                                break;
                        }
                    }
                }
        }

        private async Task Client_LoggedOut()
        {
            await Task.Yield();
            System.Environment.Exit(0);
        }

        private async Task Client_Log(LogMessage arg)
        {
            await Task.Yield();
        }

        public async Task Logout()
        {
            await this.client.LogoutAsync();
        }

        public void Dispose()
        {
            this.client.Dispose();
        }
    }
}
