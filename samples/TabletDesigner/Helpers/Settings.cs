using System.Runtime.CompilerServices;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace TabletDesigner.Helpers
{
    public static class Settings
    {
        static ISettings AppSettings => CrossSettings.Current;

        public static string Xaml
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        static T Get<T>(T defaultValue = default(T), [CallerMemberName] string propertyName = null)
        {
            return AppSettings.GetValueOrDefault(propertyName, defaultValue);
        }

        static void Set<T>(T value, [CallerMemberName]string propertyName = null)
        {
            AppSettings.AddOrUpdateValue(propertyName, value);
        }
    }
}