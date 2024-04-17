using Avalonia;
using Avalonia.Media;
using System;
using System.IO;

namespace TinyMediaInfo
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                File.WriteAllText("mediaerror.log", e.Message);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .With(new FontManagerOptions
                {
                    FontFallbacks = new []
                    {
                        new FontFallback
                        {
                            FontFamily = new FontFamily("Microsoft Yahei UI")
                        },
                        new FontFallback
                        {
                            FontFamily = new FontFamily("微软雅黑")
                        },
                        new FontFallback
                        {
                            FontFamily = new FontFamily("PingFang SC")
                        },
                        new FontFallback
                        {
                            FontFamily = new FontFamily("Hiragino Sans GB")
                        },
                        new FontFallback
                        {
                            FontFamily = new FontFamily("WenQuanYi Micro Hei")
                        },
                        new FontFallback
                        {
                            FontFamily = new FontFamily("Segoe UI")
                        }
                    }
                });
    }
}
