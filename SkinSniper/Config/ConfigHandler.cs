using Newtonsoft.Json;
using System.Diagnostics;

namespace SkinSniper.Config
{
    public static class ConfigHandler
    {
        private static ConfigStructure? _structure;

        public static bool Load()
        {
            using (var reader = new StreamReader("config.json"))
            {
                _structure = JsonConvert.DeserializeObject<ConfigStructure>(reader.ReadToEnd());

                if (_structure != null)
                {
                    Trace.WriteLine("(Config): The data has been loaded!");
                    return true;
                } 
                else
                {
                    Trace.WriteLine("(Config): The data has failed to load!");
                    return false;
                }
            }
        }

        public static ConfigStructure Get()
        {
            if (_structure != null)
            {
                return _structure;
            }
            else
            {
                _structure = new ConfigStructure();
                return _structure;
            }
        }
    }
}
