using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Leayal;

namespace LeaDiscordBot.BotWrapper.Cmds
{
    /// <summary>
    /// To help the admin/owner add or remove player from PSO2Proxy's whitelist. Create the symlink or a file with the name in the const (or edit the file name)
    /// </summary>
    public static class PSO2ProxyWhitelist
    {
        public const string WhiteListFilename = "pso2proxy.whitelist.json";
        public const string WhiteListMods = "pso2proxy.whitelist.mods";
        public static readonly ConcurrentDictionary<ulong, string> allowedUsers = ReadConfig();

        private static ConcurrentDictionary<ulong, string> ReadConfig()
        {
            var result = new ConcurrentDictionary<ulong, string>();
            if (File.Exists(WhiteListMods))
            {
                string linebuffer;
                using (var fs = new FileStream(WhiteListMods, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                using (var sr = new StreamReader(fs))
                    while (!sr.EndOfStream)
                    {
                        linebuffer = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(linebuffer))
                            result.TryAdd(ulong.Parse(linebuffer), string.Empty);
                    }
            }
            return result;
        }

        public static bool IsAllowed(Discord.IUser target)
        {
            return allowedUsers.ContainsKey(target.Id);
        }

        public async static Task Allow(Bot self, SocketMessage message)
        {
            if (!IsAllowed(message.Author))
                await message.Channel.SendMessageAsync("You don't have permission to use this command.");
            else
            {
                bool bump = false;
                foreach (var mention in message.MentionedUsers.Where((mentioned) => { return (!mentioned.IsBot && (mentioned.Id != self.Owner.Id)); }))
                {
                    if (!bump)
                        bump = allowedUsers.TryAdd(mention.Id, mention.Username);
                    else
                        allowedUsers.TryAdd(mention.Id, mention.Username);
                }
                if (bump)
                {
                    using (var fs = new FileStream(WhiteListMods, FileMode.Create))
                    using (var sw1 = new StreamWriter(fs))
                    using (var sw = new StreamWriter(fs))
                    {
                        foreach (ulong id in allowedUsers.Keys)
                            await sw.WriteLineAsync(id.ToString());
                        await sw.FlushAsync();
                    }
                    await message.Channel.SendMessageAsync("Allowed mentioned users from PSO2Proxy's whitelist permission.");
                }
                else
                    await message.Channel.SendMessageAsync("None of mentioned user was needed to be added to permission list.");
            }
        }

        public async static Task Disallow(Bot self, SocketMessage message)
        {
            if (!IsAllowed(message.Author))
                await message.Channel.SendMessageAsync("You don't have permission to use this command.");
            else
            {
                bool bump = false;
                string target = null;
                foreach (var mention in message.MentionedUsers.Where((mentioned) => { return (!mentioned.IsBot && (mentioned.Id != self.Owner.Id)); }))
                {
                    if (!bump)
                        bump = allowedUsers.TryRemove(mention.Id, out target);
                    else
                        allowedUsers.TryRemove(mention.Id, out target);
                }
                if (bump)
                {
                    using (var fs = new FileStream(WhiteListMods, FileMode.Create))
                    using (var sw = new StreamWriter(fs))
                    {
                        foreach (ulong id in allowedUsers.Keys)
                            await sw.WriteLineAsync(id.ToString());
                        await sw.FlushAsync();
                    }
                    await message.Channel.SendMessageAsync("Removed mentioned users from PSO2Proxy's whitelist permission.");
                }
                else
                    await message.Channel.SendMessageAsync("None of mentioned user was found in permission list.");
            }
        }

        public async static Task Add(SocketMessage message, IEnumerable<string> usernames)
        {
            if (!IsAllowed(message.Author))
            {
                await message.Channel.SendMessageAsync("You don't have permission to use this command.");
            }
            else
            {
                if (File.Exists(WhiteListFilename))
                {
                    bool success = false;
                    JArray jt;
                    using (var fs = new FileStream(WhiteListFilename, FileMode.Open))
                    using (var sr = new StreamReader(fs))
                    using (var stream = new Newtonsoft.Json.JsonTextReader(sr))
                    {
                        jt = JArray.Load(stream);
                        foreach (string username in usernames)
                        {
                            if (!success)
                                success = Add(jt, username);
                            else
                                Add(jt, username);
                        }
                    }
                    if (success)
                    {
                        using (var fs = new FileStream(WhiteListFilename, FileMode.Create))
                        using (var sw = new StreamWriter(fs))
                        using (var stream = new Newtonsoft.Json.JsonTextWriter(sw))
                        {
                            await jt.WriteToAsync(stream);
                            await stream.FlushAsync();
                        }
                        await message.Channel.SendMessageAsync($"Added '{usernames.Join(",")}' from PSO2Proxy's whitelist.");
                    }
                    else
                        await message.Channel.SendMessageAsync("None of mentioned user was needed to be added to whitelist.");
                }
                else
                    System.Console.WriteLine("PSO2Proxy's whitelist config file not found.");
            }
        }

        public async static Task Add(SocketMessage message, params string[] usernames)
        {
            if (!IsAllowed(message.Author))
            {
                await message.Channel.SendMessageAsync("You don't have permission to use this command.");
            }
            else
            {
                if (File.Exists(WhiteListFilename))
                {
                    bool success = false;
                    JArray jt;
                    using (var fs = new FileStream(WhiteListFilename, FileMode.Open))
                    using (var sr = new StreamReader(fs))
                    using (var stream = new Newtonsoft.Json.JsonTextReader(sr))
                    {
                        jt = JArray.Load(stream);
                        for (int i = 0; i < usernames.Length; i++)
                        {
                            {
                                if (!success)
                                    success = Add(jt, usernames[i]);
                                else
                                    Add(jt, usernames[i]);
                            }
                        }
                    }
                    if (success)
                    {
                        using (var fs = new FileStream(WhiteListFilename, FileMode.Create))
                        using (var sw = new StreamWriter(fs))
                        using (var stream = new Newtonsoft.Json.JsonTextWriter(sw))
                        {
                            await jt.WriteToAsync(stream);
                            await stream.FlushAsync();
                        }
                        await message.Channel.SendMessageAsync($"Added '{usernames.Join(",")}' from PSO2Proxy's whitelist.");
                    }
                    else
                        await message.Channel.SendMessageAsync("None of mentioned user was needed to be added to whitelist.");
                }
                else
                    System.Console.WriteLine("PSO2Proxy's whitelist config file not found.");
            }
        }

        private static bool Add(JArray jsonOjbect, string username)
        {
            bool alreadyIn = jsonOjbect.Children().Any((token) => { return (token.Value<object>().ToString() == username); });
            if (!alreadyIn)
            {
                jsonOjbect.Add(username);
                return true;
            }
            else
                return false;
        }

        public async static Task Remove(SocketMessage message, IEnumerable< string > usernames)
        {
            if (!IsAllowed(message.Author))
            {
                await message.Channel.SendMessageAsync("You don't have permission to use this command.");
            }
            else
            {
                if (File.Exists(WhiteListFilename))
                {
                    bool success = false;
                    JArray jt;
                    using (var fs = new FileStream(WhiteListFilename, FileMode.Open))
                    using (var sr = new StreamReader(fs))
                    using (var stream = new Newtonsoft.Json.JsonTextReader(sr))
                    {
                        jt = await JArray.LoadAsync(stream);
                        foreach (string username in usernames)
                        {
                            if (!success)
                                success = Remove(jt, username);
                            else
                                Remove(jt, username);
                        }
                    }
                    if (success)
                    {
                        using (var fs = new FileStream(WhiteListFilename, FileMode.Create))
                        using (var sw = new StreamWriter(fs))
                        using (var stream = new Newtonsoft.Json.JsonTextWriter(sw))
                        {
                            await jt.WriteToAsync(stream);
                            await stream.FlushAsync();
                        }
                        await message.Channel.SendMessageAsync($"Removed '{usernames.Join(",")}' from PSO2Proxy's whitelist.");
                    }
                    else
                        await message.Channel.SendMessageAsync("None of mentioned user was found in list.");
                }
                else
                    System.Console.WriteLine("PSO2Proxy's whitelist config file not found.");
            }
        }

        public async static Task Remove(SocketMessage message, params string[] usernames)
        {
            if (!IsAllowed(message.Author))
            {
                await message.Channel.SendMessageAsync("You don't have permission to use this command.");
            }
            else
            {
                if (File.Exists(WhiteListFilename))
                {
                    bool success = false;
                    JArray jt;
                    using (var fs = new FileStream(WhiteListFilename, FileMode.Open))
                    using (var sr = new StreamReader(fs))
                    using (var stream = new Newtonsoft.Json.JsonTextReader(sr))
                    {
                        jt = await JArray.LoadAsync(stream);
                        for (int i = 0; i < usernames.Length; i++)
                        {
                            {
                                if (!success)
                                    success = Remove(jt, usernames[i]);
                                else
                                    Remove(jt, usernames[i]);
                            }
                        }
                    }
                    if (success)
                    {
                        using (var fs = new FileStream(WhiteListFilename, FileMode.Create))
                        using (var sw = new StreamWriter(fs))
                        using (var stream = new Newtonsoft.Json.JsonTextWriter(sw))
                        {
                            await jt.WriteToAsync(stream);
                            await stream.FlushAsync();
                        }
                        await message.Channel.SendMessageAsync($"Removed '{usernames.Join(",")}' from PSO2Proxy's whitelist.");
                    }
                    else
                        await message.Channel.SendMessageAsync("None of mentioned user was found in list.");
                }
                else
                    System.Console.WriteLine("PSO2Proxy's whitelist config file not found.");
            }
        }

        private static bool Remove(JArray jsonOjbect, string username)
        {
            foreach (var token in jsonOjbect.Children().Where((token) => { return (token.Value<object>().ToString() == username); }))
            {
                token.Remove();
                return true;
            }
            return false;
        }
    }
}
