using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

        public async static Task<bool> Allow(Bot self, SocketMessage message)
        {
            if (!IsAllowed(message.Author)) return false;
            bool bump = false;
            foreach (var mention in message.MentionedUsers.Where((mentioned) => { return (!mentioned.IsBot && (mentioned.Id != self.Owner.Id)); }))
            {
                if (!bump)
                    bump = allowedUsers.TryAdd(mention.Id, mention.Username);
                else
                    allowedUsers.TryAdd(mention.Id, mention.Username);
            }
            if (bump)
                using (var fs = new FileStream(WhiteListMods, FileMode.Create))
                using (var sw = new StreamWriter(fs))
                {
                    foreach (ulong id in allowedUsers.Keys)
                        await sw.WriteLineAsync(id.ToString());
                    await sw.FlushAsync();
                }
            return bump;
        }

        public async static Task<bool> Disallow(Bot self, SocketMessage message)
        {
            if (!IsAllowed(message.Author)) return false;
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
                using (var fs = new FileStream(WhiteListMods, FileMode.Create))
                using (var sw = new StreamWriter(fs))
                {
                    foreach (ulong id in allowedUsers.Keys)
                        await sw.WriteLineAsync(id.ToString());
                    await sw.FlushAsync();
                }
            return bump;
        }

        public async static Task<bool> Add(SocketMessage message, IEnumerable<string> usernames)
        {
            if (!IsAllowed(message.Author)) return false;
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
                }
                return success;
            }
            else
                return false;
        }

        public async static Task<bool> Add(SocketMessage message, params string[] usernames)
        {
            if (!IsAllowed(message.Author)) return false;
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
                }
                return success;
            }
            else
                return false;
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

        public async static Task<bool> Remove(SocketMessage message, IEnumerable< string > usernames)
        {
            if (!IsAllowed(message.Author)) return false;
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
                }
                return success;
            }
            else
                return false;
        }

        public async static Task<bool> Remove(SocketMessage message, params string[] usernames)
        {
            if (!IsAllowed(message.Author)) return false;
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
                }
                return success;
            }
            else
                return false;
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
