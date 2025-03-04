using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace MediaControlDistributionCenter.Services
{

    public class LocalizationManager
    {
        private Dictionary<string, string> _localizedStrings;

        // 移除原来的 LanguageCode 属性的 set 访问器
        public string LanguageCode { get; private set; }

        public LocalizationManager(string languageCode)
        {
            LanguageCode = languageCode;
            _localizedStrings = new Dictionary<string, string>();
            LoadLanguageResources(languageCode);
        }

        public void LoadLanguageResources(string languageCode)
        {
            LanguageCode = languageCode;
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources/{languageCode}.json");

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _localizedStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
            }
            else
            {
                MessageBox.Show("Language file not found: " + filePath);
            }
        }

        public string GetLocalizedString(string key)
        {
            if (_localizedStrings.ContainsKey(key))
            {
                return _localizedStrings[key];
            }
            return key;
        }
    }

}
