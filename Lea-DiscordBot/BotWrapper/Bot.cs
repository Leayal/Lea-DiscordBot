using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Leayal;
using System.Linq;
using System.Text.RegularExpressions;

namespace LeaDiscordBot.BotWrapper
{
    public class Bot : System.IDisposable
    {
        private DiscordSocketClient _client;
        public DiscordSocketClient Client => this._client;
        private string _commandPrefix;
        public string CommandPrefix => this._commandPrefix;
        private Tasks.EQPoking poooooookkeeeee;
        private bool launchEQPoking;
        public CommandCooldown CommandCooldown { get; }

        private System.Collections.Concurrent.ConcurrentDictionary<ulong, ISocketMessageChannel> eqchannelsCache;

        public Bot(string cmdPrefix, bool launchEQPokingOnLogin = true)
        {
            System.Console.WriteLine("Creating Bot instance");
            this.eqchannelsCache = new System.Collections.Concurrent.ConcurrentDictionary<ulong, ISocketMessageChannel>();
            this._commandPrefix = cmdPrefix;
            this.launchEQPoking = launchEQPokingOnLogin;
            this.CommandCooldown = new CommandCooldown();
            this._client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                MessageCacheSize = 1,
                LogLevel = Discord.LogSeverity.Error,
                HandlerTimeout = 5000,
                ConnectionTimeout = 3000,
                DefaultRetryMode = Discord.RetryMode.Retry502 | Discord.RetryMode.RetryTimeouts
            });
            this._client.LoggedIn += Client_LoggedIn;
            this._client.LoggedOut += Client_LoggedOut;
            this._client.MessageReceived += Client_MessageReceived;
            this._client.Ready += Client_Ready;
            this._client.JoinedGuild += Client_JoinedGuild;
            this._client.LeftGuild += Client_LeftGuild;
            this._client.Log += Client_Log;
            //this.commander = new CommandService(new CommandServiceConfig() { CaseSensitiveCommands = false });
            this.poooooookkeeeee = new Tasks.EQPoking();
            this.poooooookkeeeee.EQDataChanged += Poooooookkeeeee_EQDataChanged;
            Cmds.Help.BuildHelp(cmdPrefix);
        }

        private Task Client_LeftGuild(SocketGuild arg)
        {
            System.Console.WriteLine($"Left server '{arg.Name}'");
            this.eqchannelsCache.TryRemove(arg.Id, out var outfaceroll_laiwhgliahwg);
            return Task.CompletedTask;
        }

        private Task Client_JoinedGuild(SocketGuild arg)
        {
            System.Console.WriteLine($"Joined server '{arg.Name}'");
            foreach (var channelSocket in arg.Channels)
            {
                if (channelSocket.Name.IsEqual("eq-alert", true))
                {
                    ISocketMessageChannel faceroll_laiwhgliahwg = channelSocket as ISocketMessageChannel;
                    if (faceroll_laiwhgliahwg != null)
                    {
                        if (!this.eqchannelsCache.TryAdd(arg.Id, faceroll_laiwhgliahwg))
                            this.eqchannelsCache[arg.Id] = faceroll_laiwhgliahwg;
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            Cmds.Info.CreateInfoEmbed(this._client, this.Owner);
            await this._client.SetGameAsync(string.Format("Use {0}help for surprise.", this._commandPrefix));

            foreach (var guildSocket in this._client.Guilds)
            {
                System.Console.WriteLine(guildSocket.Name);
                foreach (var channelSocket in guildSocket.Channels)
                {
                    if (channelSocket.Name.IsEqual("eq-alert", true))
                    {
                        ISocketMessageChannel faceroll_laiwhgliahwg = channelSocket as ISocketMessageChannel;
                        if (faceroll_laiwhgliahwg != null)
                        {
                            if (!this.eqchannelsCache.TryAdd(guildSocket.Id, faceroll_laiwhgliahwg))
                                this.eqchannelsCache[guildSocket.Id] = faceroll_laiwhgliahwg;
                            /*
                            if (arg.EmbedData == null)
                                await faceroll_laiwhgliahwg.SendMessageAsync(arg.Message);
                            else
                                await faceroll_laiwhgliahwg.SendMessageAsync(arg.Message, false, arg.EmbedData);
                            //*/
                            break;
                        }
                    }
                }
            }

            if (this.launchEQPoking)
                await this.poooooookkeeeee.StartPoking();
        }

        private async Task Poooooookkeeeee_EQDataChanged(Tasks.EQPoking.EQPostBlock arg)
        {
            if (string.IsNullOrWhiteSpace(arg.Message) && arg.EmbedData == null) return;
            foreach (var channel in this.eqchannelsCache.Values)
            {
                await channel.SendMessageAsync(arg.Message);
            }
        }

        private async Task ThrowEQMsg(SocketGuild server, Tasks.EQPoking.EQPostBlock arg)
        {
            foreach (var channelSocket in server.Channels)
                if (channelSocket.Name.IsEqual("eq-alert", true))
                {
                    ISocketMessageChannel faceroll_laiwhgliahwg = channelSocket as ISocketMessageChannel;
                    if (faceroll_laiwhgliahwg != null)
                    {
                        await faceroll_laiwhgliahwg.SendMessageAsync(arg.Message);
                        /*
                        if (arg.EmbedData == null)
                            await faceroll_laiwhgliahwg.SendMessageAsync(arg.Message);
                        else
                            await faceroll_laiwhgliahwg.SendMessageAsync(arg.Message, false, arg.EmbedData);
                        //*/
                    }
                }
        }

        public IUser Owner { get; protected set; }

        private async Task Client_LoggedIn()
        {
            System.Console.WriteLine("Bot logged in. Ready~!");
            var myInfo = await _client.GetApplicationInfoAsync();
            System.Console.WriteLine(string.Format("Owner: {0}{1}", myInfo.Owner.Username, string.IsNullOrEmpty(myInfo.Owner.Discriminator) ? string.Empty : "#" + myInfo.Owner.Discriminator));
            this.Owner = myInfo.Owner;
            await Task.Yield();
        }

        public async Task Login(string botToken)
        {
            System.Console.WriteLine("Logging in");
            await this._client.LoginAsync(TokenType.Bot, botToken);
            await this._client.StartAsync();
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
                if (message.Author != this._client.CurrentUser)
                {
                    bool isMentioned = false;
                    foreach (var mentioned in message.MentionedUsers)
                        if (mentioned.Id == this._client.CurrentUser.Id)
                        {
                            isMentioned = true;
                            break;
                        }
                    if (isMentioned)
                    {
                        Regex finding = new Regex(@"<@\d+>");
                        string trimmedMentions = finding.Replace(message.Content, "");
                        if (string.IsNullOrWhiteSpace(trimmedMentions))
                            await message.Channel.SendMessageAsync(string.Format("How can i help you, <@{0}>???", message.Author.Id));
                        else
                        {
                            //string lowerMsg = trimmedMentions.ToLower();
                            //string[] splittedMsg = trimmedMentions.Remove(0, 1).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                            
                        }
                    }
                    // Command type with command prefix ????
                    if (message.Content.Length > 1 && message.Content.IndexOf(this._commandPrefix) == 0)
                    {
                        // Remove the command prefix and start to parse the command
                        string[] splittedMsg = message.Content.Remove(0, 1).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        // Someone ordered me to do something ???
                        switch(splittedMsg[0].ToLower())
                        {
                            case "info":
                                if (this.CommandCooldown.HasCooledDown("info", message.Channel.Id))
                                {
                                    this.CommandCooldown.RequestCooldown("info", message.Channel.Id, 300000);
                                    await Cmds.Info.ProcessMessage(this._client, message, this.Owner);
                                }
                                else
                                    await message.Channel.SendMessageAsync("Uh....Slow down~! Do you want to know about me that badly~?");
                                break;
                            case "help":
                                if (this.CommandCooldown.HasCooledDown("help", message.Channel.Id))
                                {
                                    this.CommandCooldown.RequestCooldown("help", message.Channel.Id, 300000);
                                    await Cmds.Help.ProcessMessage(this._client, message);
                                }
                                else
                                    await message.Channel.SendMessageAsync("Uh....Slow down~! Do you want to know what I can do that badly~?");
                                break;
                            case "swskillsimu":
                                if (this.CommandCooldown.HasCooledDown("swskillsimu", message.Channel.Id))
                                {
                                    this.CommandCooldown.RequestCooldown("swskillsimu", message.Channel.Id, 60000);
                                    await Cmds.SWskillsimu.ProcessMessage(this._client, message);
                                }
                                else
                                    await message.Channel.SendMessageAsync("Uh....Slow down~! Try to restrain yourself from spamming~!");
                                break;
                            /*case "warn":
                                await Orders.Warn.ProcessMessage(this, message);
                                break;//*/
                            case "shutdown":
                                if (message.Author.Id == this.Owner.Id)
                                {
#pragma warning disable 4014
                                    this._client.LogoutAsync();
#pragma warning restore 4014
                                    await this._client.StopAsync();                                    
                                    System.Environment.Exit(0);
                                }
                                return;
                            case "error":
                                await Cmds.ErrorExplainer.ProcessMessage(this._client, message, this._commandPrefix, splittedMsg);
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
            this.poooooookkeeeee.CancelTask();
            await Task.Yield();
            System.Environment.Exit(0);
        }

        private async Task Client_Log(LogMessage arg)
        {
            await Task.Yield();
        }

        public async Task Logout()
        {
            await this._client.LogoutAsync();
        }

        public void Dispose()
        {
            this._client.Dispose();
        }
    }
}
