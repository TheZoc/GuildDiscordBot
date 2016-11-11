using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace GuildDiscordBot.Bot
{
    public static class RuntimeStatics
    {
        public static GuildClient Client { get; set; }

        public static string QuestionPath = @"Questions.json";
        private static List<string> _questions;
        public static List<string> Questions {
            get
            {
                if (_questions == null)
                    _questions = Tools.Tools.LoadJSONFile<List<string>>(QuestionPath);

                return _questions;
            }}
    
        public static Dictionary<Channel, int> ActiveChannels = new Dictionary<Channel, int>();
        
    }
}
