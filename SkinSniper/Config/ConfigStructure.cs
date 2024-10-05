namespace SkinSniper.Config
{
    public class ConfigBuff
    {
        public string Cookie { get; set; } = default!;
    }

    public class ConfigSkinport
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Cookie { get; set; } = default!;
    }

    public class ConfigTelegram
    {
        public string Token { get; set; } = default!;
        public List<long> Users { get; set; } = default!;
    }

    public class ConfigStructure
    {
        public ConfigBuff Buff { get; set; } = default!;
        public ConfigSkinport Skinport { get; set; } = default!;
        public ConfigTelegram Telegram { get; set; } = default!;
        public bool Status { get; set; } = default!;
    }
}
