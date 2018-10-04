namespace OnePaceCore.Enums
{
    public enum SonyVegasVersion
    {
        Fourteen = 140,
        Fifteen = 150
    }
    public static class SonyVegasVersionExtension
    {
        public static string GetProcessName(this SonyVegasVersion instance)
        {
            return "vegas" + (int)instance;
        }
    }
}
