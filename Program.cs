using GuildDiscordBot.Bot;
using GuildDiscordBot.Bot.Tools;

namespace GuildDiscordBot
{
    public static class Program
    {
        public static string GuildInfoPath = @"GuildInfo.json";
        public static GuildInfo Guild = Tools.LoadJSONFile<GuildInfo>(GuildInfoPath);
        static void Main(string[] args) => new GuildClient();

        public static void SaveGuildInfo()
        {
            Tools.SaveToJSONFile(Guild, GuildInfoPath);
        }
    }
}
