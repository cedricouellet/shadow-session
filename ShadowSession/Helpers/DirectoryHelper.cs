using ShadowSession.Data;
using System.IO;

namespace ShadowSession.Helpers
{
    public static class DirectoryHelper
    {
        public static bool CanWriteInDirectory(string directoryPath)
        {
            // maybe not the cleanest way to check this, but certainly the most reliable

            // If the directory exists
            // try to write a temporary file inside it, then delete the file
            // if that works, then we can write inside the directory
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    var tempFilePath = Path.Join(directoryPath, "ShadowSession.temp");
                    File.Create(tempFilePath).Close();
                    File.Delete(tempFilePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            // If the directory doesn't exists
            // Try to create a temporary sub directory inside it, then delete the subdirectory
            // if that works, then we can write inside the directory
            else
            {
                try
                {
                    var tempSubDirectory = Path.Join(directoryPath, ".ShadowSession_Temp");
                    Directory.CreateDirectory(tempSubDirectory);
                    Directory.Delete(tempSubDirectory);
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }

        public static string GetProgramRecordingDirectory(Program program)
        {
            var baseDirectory = UserSettingReader.GetRecordingsDirectory();

            return Path.Join(baseDirectory, program.DisplayName);
        }
    }
}
