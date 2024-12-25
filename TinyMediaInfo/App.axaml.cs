using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TinyMediaInfo.Localization;

namespace TinyMediaInfo
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            LoadStringResource();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void LoadStringResource()
        {
            var language = CultureInfo.CurrentCulture.Name;
            if (language == "zh-CN")
            {
                return;
            }

            // 避免动态加载
            Current!.Resources.MergedDictionaries[0] = new EnglishResourceDictionary();
        }

        public static string GetLocalizedString(string name)
        {
            try
            {
                Current!.TryFindResource(name, out var value);
                if (value is string s)
                {
                    return s;
                }
                return "!!";
            }
            catch
            {
                return "??";
            }
        }
    }
}