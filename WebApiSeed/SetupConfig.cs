namespace WedApiSeed
{
    public static class SetupConfig
    {
        public static Setting Setting { get; set; }
    }

    public class Setting
    {
        public long ServiceTimer { get; set; }
        public string Expiry { get; set; }
        public Connections Connections { get; set; }
    }

    public class Connections
    {
        public string Name { get; set; }
        public string ApiKey { get; set; }
    }
}