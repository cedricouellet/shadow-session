using System.Diagnostics;
using System.IO;

namespace ShadowSession.Helpers
{
    /// <summary>
    /// A helper for detecting executables on the current local computer
    /// </summary>
    public static class ExecutableDetector
    {
        /// <summary>
        /// The cached installed executables
        /// </summary>
        private static IEnumerable<Executable>? executablesInstalledCache;

        /// <summary>
        /// The enumeration of excluded file patterns
        /// </summary>
        /// <remarks>Absolute or relative paths allowed - Lowercase values only</remarks>
        private static readonly IEnumerable<string> _excludedFilePatterns =
        [
            // Hidden
            @"\.",
            // System
            @"c:\windows\", 
            // Development, builds & redistributables
            @"\debug\",
            @"\obj\",
            @"\bin\",
            @"\dist\",
            @"\distribution\",
            @"\mingw64\",
            @"\lib\",
            @"\libexec\",
            @"\lib64\",
            @"\win64\",
            @"\dotnet\packs\",
            @"\dotnet\shared\",
            @"\android-sdk\",
            @"\microsoft sdks\",
            @"\ide\extensions\",
            @"\ide\commonextensions\",
            @"\msbuild\",
            @"\unity\hub\editor\",
            @"\sdk\",
            @"\sdks\",
            @"\redist\",
            @"\redists\",
            @"\vcredist\",
            @"\redistributables\",
            @"\_commonredist\",
            @"\directx\",
            @"\binaries\",
            // Uninstallers
            @"\uninst", 
            // Temporary
            @"\Temp\", 
        ];

        /// <summary>
        /// Detects programs that are currently running on the computer (having a dedicated main window).
        /// </summary>
        /// <returns>An enumeration of program informations</returns>
        public static IEnumerable<Executable> GetExecutablesRunning()
        {
            List<Executable> executables = [];

            foreach (var process in Process.GetProcesses())
            {
                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }

                try
                {
                    string? processPath = process.MainModule?.FileName;

                    if (!string.IsNullOrEmpty(processPath) && !_excludedFilePatterns.Any(processPath.ToLower().Contains))
                    {
                        executables.Add(new Executable
                        {
                            Name = process.ProcessName,
                            Path = processPath,
                        });
                    }
                }
                catch
                {
                    continue;
                }
            }

            return executables.DistinctBy(x => x.Path).OrderBy(x => x.Name);
        }

        public static IntPtr GetProcessWindowHandle(string executablePath)
        {
            var processes = Process.GetProcesses()
                .Where(x => x.MainWindowHandle != IntPtr.Zero)
                .DistinctBy(x => x.ProcessName);

            foreach (var process in processes)
            {
                try
                {
                    if (process.MainModule?.FileName == executablePath)
                    {
                        return process.MainWindowHandle;
                    }
                }
                catch
                {
                    continue;
                }
                
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Detects programs that are installed on the computer.
        /// </summary>
        /// <param name="invalidateCache">Whether or not to invalidate the cache and perform the search (warning: expensive and lengthy operation)</param>
        /// <returns>An enumeration of program informations</returns>
        public static IEnumerable<Executable> GetExecutablesInstalled(bool invalidateCache = false)
        {
            if (!invalidateCache && executablesInstalledCache != null)
            {
                return executablesInstalledCache;
            }

            Initialize(out executablesInstalledCache);

            return executablesInstalledCache;
          
        }

        public static void Initialize()
        {
            Initialize(out executablesInstalledCache);
        }

        private static void Initialize(out IEnumerable<Executable> installedExecutables)
        {
            var executables = new List<Executable>();

            var drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                // Skip non-ready or non-physical drives (eg: network drives, CD/USB drives)
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                {
                    continue;
                }

                try
                {
                    var executableFiles = Directory.GetFiles(drive.RootDirectory.FullName, "*.exe", new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true,
                        AttributesToSkip = FileAttributes.Hidden | FileAttributes.Encrypted | FileAttributes.Temporary | FileAttributes.System | FileAttributes.SparseFile,
                        MaxRecursionDepth = UserSettingReader.GetMaxExecutableDetectionDepth(),
                    });

                    foreach (var executableFile in executableFiles)
                    {
                        if (_excludedFilePatterns.Any(executableFile.ToLower().Contains))
                        {
                            continue;
                        }

                        executables.Add(new Executable
                        {
                            Name = executableFile.Split(@"\", StringSplitOptions.TrimEntries).Last().Replace(".exe", ""),
                            Path = executableFile,
                        });
                    }
                }
                catch
                {
                    continue;
                }
            }

            installedExecutables = executables.OrderBy(x => x.Name);
        }
    }
}
