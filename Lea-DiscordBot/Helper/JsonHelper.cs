using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LeaDiscordBot.Helper
{
    public static class JsonHelper
    {
        public static Dictionary<string, object> FlattenJson(JsonReader reader)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            string currentPropertyname = null;
            while (reader.Read())
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    // Since this is a flat JSON, this is safe
                    currentPropertyname = reader.Value as string;
                    if (reader.Read())
                        result.Add(currentPropertyname, reader.Value);
                }
            return result;
        }
    }
}
