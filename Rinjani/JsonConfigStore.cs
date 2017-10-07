using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Rinjani
{
    public class JsonConfigStore : IConfigStore
    {
        public JsonConfigStore(string path, IList<IConfigValidator> configValidators)
        {
            var s = File.ReadAllText(path);
            Config = JsonConvert.DeserializeObject<ConfigRoot>(s);
            if (Config == null)
            {
                throw new InvalidOperationException("Failed to deserialize the config file.");
            }
            foreach (var configValidator in configValidators)
            {
                configValidator.Validate(Config);
            }
        }

        public ConfigRoot Config { get; }        
    }
}