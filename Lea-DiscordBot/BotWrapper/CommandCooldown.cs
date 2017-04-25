using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace LeaDiscordBot.BotWrapper
{
    public class CommandCooldown
    {
        ConcurrentDictionary<string, ConcurrentDictionary<ulong, CooldownInfo>> innerDict;
        public CommandCooldown()
        {
            this.innerDict = new ConcurrentDictionary<string, ConcurrentDictionary<ulong, CooldownInfo>>();
        }

        public bool HasCooledDown(string tag, ulong channelID)
        {
            if (this.innerDict.TryGetValue(tag, out var item))
                if (item.TryGetValue(channelID, out var theCooldown))
                {
                    if (theCooldown.HasCooldown)
                    {
                        item.TryRemove(channelID, out theCooldown);
                        return true;
                    }
                    else
                        return false;
                }
            return true;
        }

        public bool RequestCooldown(string tag, ulong channelID, int milisecond)
        {
            ConcurrentDictionary<ulong, CooldownInfo> item;
            if (!this.innerDict.TryGetValue(tag, out item))
            {
                item = new ConcurrentDictionary<ulong, CooldownInfo>();
                this.innerDict.TryAdd(tag, item);
            }
            CooldownInfo dun = new CooldownInfo(milisecond);
            dun.ReachedCooldown += (sender, e) =>
            {
                item.TryRemove(channelID, out var asd);
            };
            return item.TryAdd(channelID, dun);
        }

        public void Clear()
        {
            this.innerDict.Clear();
        }

        private class CooldownInfo
        {
            Timer myTimer;
            private bool _hasCooldown;
            public bool HasCooldown { get { return this._hasCooldown; } }
            internal CooldownInfo(int cooldownMilisecond)
            {
                this._hasCooldown = false;
                myTimer = new Timer(new TimerCallback((obj)=> {
                    this._hasCooldown = true;
                    this.ReachedCooldown?.Invoke(this, EventArgs.Empty);
                    myTimer.Dispose();
                }), null, Timeout.Infinite, Timeout.Infinite);
                myTimer.Change(cooldownMilisecond, Timeout.Infinite);
            }

            public event EventHandler ReachedCooldown;
        }
    }
}
