using System;
using System.IO;
using Newtonsoft.Json;

namespace Rinjani
{
    public class JsonConfigStore : IConfigStore
    {
        public JsonConfigStore(string path)
        {
            var s = File.ReadAllText(path);
            Config = JsonConvert.DeserializeObject<ConfigRoot>(s);
            if (Config == null)
            {
                throw new InvalidOperationException("Failed to deserialize the config file.");
            }
        }

        public ConfigRoot Config { get; }
    }
}