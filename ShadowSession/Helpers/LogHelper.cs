using System.IO;

namespace ShadowSession.Helpers
{
    public static class LogHelper
    {
        private static string GetLogsDirectory()
        {
            return Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cedricao",
                "ShadowSession",
                "logs");
        }

        public static string GetRecordingLogDirectory()
        {
            return Path.Join(GetLogsDirectory(), "recorder");
        }
    }
}
