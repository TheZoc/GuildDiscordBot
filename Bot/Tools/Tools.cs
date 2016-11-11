using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GuildDiscordBot.Bot.Tools
{
    public static class Tools
    {
        public static void SaveToJSONFile<T>(T obj, string path)
        {
            using (StreamWriter writer = File.CreateText(path))
            {
                string jsonStr = JsonConvert.SerializeObject(obj);
                writer.Write(jsonStr);
                writer.Close();
            }
        }

        public static T LoadJSONFile<T>(string path) where T : new()
        {
            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }
            else
            {
                Console.WriteLine($"No JSON Found for path: {path}. Creating default object and saving it.");
                T returnObj = new T();
                SaveToJSONFile(returnObj, path);
                return returnObj;
            }
        }
    }
}
