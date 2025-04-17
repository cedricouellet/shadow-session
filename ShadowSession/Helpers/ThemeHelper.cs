using ControlzEx.Theming;
using System.Windows;
using System.Windows.Media;

namespace ShadowSession.Helpers
{
    public static class ThemeColors
    {
        private static ResourceDictionary _dict;

        static ThemeColors()
        {
            _dict = ThemeManager.Current.DetectTheme()?.GetAllResources()?.FirstOrDefault() ?? [];
        }

        public static Color Highlight => GetColor("MahApps.Colors.Highlight");
        public static Color AccentBase => GetColor("MahApps.Colors.AccentBase");
        public static Color Accent => GetColor("MahApps.Colors.Accent");
        public static Color Accent2 => GetColor("MahApps.Colors.Accent2");
        public static Color Accent3 => GetColor("MahApps.Colors.Accent3");
        public static Color Accent4 => GetColor("MahApps.Colors.Accent4");
        public static Color ThemeForeground => GetColor("MahApps.Colors.ThemeForeground");
        public static Color ThemeBackground => GetColor("MahApps.Colors.ThemeBackground");
        public static Color IdealForeground => GetColor("MahApps.Colors.IdealForeground");
        public static Color Gray => GetColor("MahApps.Colors.Gray");
        public static Color Gray1 => GetColor("MahApps.Colors.Gray1");
        public static Color Gray2 => GetColor("MahApps.Colors.Gray2");
        public static Color Gray3 => GetColor("MahApps.Colors.Gray3");
        public static Color Gray4 => GetColor("MahApps.Colors.Gray4");
        public static Color Gray5 => GetColor("MahApps.Colors.Gray5");
        public static Color Gray6 => GetColor("MahApps.Colors.Gray6");
        public static Color Gray7 => GetColor("MahApps.Colors.Gray7");
        public static Color Gray8 => GetColor("MahApps.Colors.Gray8");
        public static Color Gray9 => GetColor("MahApps.Colors.Gray9");
        public static Color Gray10 => GetColor("MahApps.Colors.Gray10");
        public static Color GrayMouseOver => GetColor("MahApps.Colors.Gray.MouseOver");
        public static Color GraySemiTransparent => GetColor("MahApps.Colors.Gray.SemiTransparent");
        public static Color Flyout => GetColor("MahApps.Colors.Flyout");
        public static Color ProgressIndeterminate1 => GetColor("MahApps.Colors.ProgressIndeterminate1");
        public static Color ProgressIndeterminate2 => GetColor("MahApps.Colors.ProgressIndeterminate2");
        public static Color ProgressIndeterminate3 => GetColor("MahApps.Colors.ProgressIndeterminate3");
        public static Color ProgressIndeterminate4 => GetColor("MahApps.Colors.ProgressIndeterminate4");

        private static Color GetColor(string key)
        {
            return (Color)_dict[key];
        }
    }
}
