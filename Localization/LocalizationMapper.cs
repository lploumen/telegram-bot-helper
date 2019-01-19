using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Telegram.Bot.Helper.Localization
{
    internal sealed class LocalizationMapper<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly string _directoryPath;
        
        internal LocalizationMapper(string directoryPath)
        {
            _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        }

        internal IEnumerable<(string key, TLocalizationModel value)> GetLocalizationModels()
        {
            foreach (var fileName in Directory.GetFiles(_directoryPath, "*.json", SearchOption.AllDirectories))
                yield return (Path.GetFileNameWithoutExtension(fileName), ReadFromJsonFile(Path.GetFileName(fileName)));
        }

        private TLocalizationModel ReadFromJsonFile(string fileName)
        {
            var path = Path.Combine(_directoryPath, fileName);

            TLocalizationModel model;
            using (var fileStream = File.Open(Path.Combine(_directoryPath, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                model = JsonConvert.DeserializeObject<TLocalizationModel>(streamReader.ReadToEnd());

            return model;
        }
    }
}
