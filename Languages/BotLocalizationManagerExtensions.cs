using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Telegram.Bot.Helper.Languages
{
    internal static class BotLocalizationManagerExtensions
    {
        internal static Dictionary<string, string> ReadLocalizationDataFromFile(string basePath, string languageCode, Encoding enc)
        {
            var path = Path.Combine(basePath, $"{languageCode}.json");
            if (!File.Exists(path))
                throw new FileNotFoundException($"File {path} was not found");

            string json;
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var fileReader = enc != null ? new StreamReader(file, enc) : new StreamReader(file, true))
                {
                    json = fileReader.ReadToEnd();
                }
            }
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }
}
