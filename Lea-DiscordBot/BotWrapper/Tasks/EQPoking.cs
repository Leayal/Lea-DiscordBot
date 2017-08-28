using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LeaDiscordBot.BotWrapper.Tasks
{
    /// <summary>
    /// Example inheriting EQPoke class. Then you can create a new instance of this class to use.
    /// </summary>
    public class EQPoking : EQPoke
    {
        protected override Dictionary<string, object> ReadValueFromServer(Stream sourceStream)
        {
            // Implement your codes to read data here
            throw new NotImplementedException();

            /* It should return a list of:
             * - "0"->"EQ Name" : "EQ Name" is happening
             * - "30"->"EQ Name" : 30mins left to "EQ Name"
             * - "60"->"EQ Name"
             * - "90"->"EQ Name"
             * - "120"->"EQ Name"
             * - "150"->"EQ Name"
             * - "180"->"EQ Name"
             * - "210"->"EQ Name"
             * 
             * [Hour]->String (You can fill this with any like "10 EDT" or "about 200mins left" or any string you like.
             * Ship[N]->"EQ Name" will will parse at random "EQ Name" for "Ship[N]" at "[Hour]"
            */
        }
    }
}
