using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace MediaControlDistributionCenter.Services
{
    public class LocalizedExtension: MarkupExtension
    {
        public string Key { get; set; }

        public LocalizedExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 从 Application 资源中获取 LocalizationManager
            var localizationManager = (LocalizationManager)App.Current.Resources["LocalizationManager"];
            return localizationManager.GetLocalizedString(Key);
        }
    }
}
