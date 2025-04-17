using CommunityToolkit.Mvvm.DependencyInjection;
using ShadowSession.Data;

namespace ShadowSession.Helpers
{
    public static class UserSettingReader
    {
        public static bool IsAutomaticRecordingEnabled()
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.AutomaticRecordingEnabledKey);

            return bool.Parse(userSetting.Value!);
        }

        public static string GetRecordingsDirectory()
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.RecordingsDirectoryKey);

            return userSetting.Value!;
        }

        public static int GetDefaultRecordingsFramerate()
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.DefaultRecordingsFramerateKey);

            return int.Parse(userSetting.Value!);
        }

        public static int GetDefaultRecordingsBitrate()
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.DefaultRecordingsBitrateKey);

            return int.Parse(userSetting.Value!);
        }

        public static int GetMaxExecutableDetectionDepth()
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.MaxExecutableDetectionDepthKey);

            return int.Parse(userSetting.Value!);
        }

        public static bool AreNotificationsEnabled()
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.NotificationsEnabledKey);

            return bool.Parse(userSetting.Value!);
        }

        public static bool ShouldShowAboutPageInitially()
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.ShowAboutPageInitiallyKey);

            return bool.Parse(userSetting.Value!);
        }

        public static void SetShowAboutPageInitially(bool value)
        {
            using var context = Ioc.Default.GetRequiredService<ShadowSessionContext>();

            var userSetting = context.UserSettings.Single(x => x.Key == UserSettingKeys.ShowAboutPageInitiallyKey);

            userSetting.Value = value.ToString();

            context.SaveChanges();
        }


    }
}
