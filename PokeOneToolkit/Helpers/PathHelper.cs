using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PokeOneToolkit.Helpers
{
    internal static class PathHelper
    {
        public static string GetProperLauncherPath(string unsafePath)
        {
            string temp;
            bool isFile = File.Exists(unsafePath);
            bool isDirectory = Directory.Exists(unsafePath);

            if (!isFile && !isDirectory)
                throw new Exception("Passed launcher path is neither a file nor a directory");

            string directory = isFile ? Path.GetDirectoryName(unsafePath) : unsafePath;

            if (IsLauncherDirectory(directory))
                return directory;
            if (IsUnityDirectory(directory) && !string.IsNullOrWhiteSpace(temp = PopLastFromPath(directory)))
                return temp;
            
            // TODO: support shortcut files? (.lnk, .url)

            throw new Exception("Did not get a valid launcher or unity path");
        }

        private static bool IsLauncherDirectory(string dir)
        {
            Debug.Assert(!File.Exists(dir), "Passed launcher directory is a file");

            return Directory.Exists(dir) &&
                   Directory.GetFiles(dir).Select(Path.GetFileName).ContainsAll("Launcher.exe", "unins000.dat", "unins000.exe") &&
                   Directory.GetDirectories(dir).Select(Path.GetFileName).Contains("files");
        }

        private static bool IsUnityDirectory(string dir)
        {
            Debug.Assert(!File.Exists(dir), "Passed Unity directory is a file");

            return Directory.Exists(dir) &&
                   Directory.GetFiles(dir).Select(Path.GetFileName).ContainsAll("PokeOne.exe", "UnityPlayer.dll") &&
                   Directory.GetDirectories(dir).Select(Path.GetFileName).ContainsAll("PokeOne_Data", "MonoBleedingEdge");
        }

        private static string PopLastFromPath(string path)
        {
            char[] separators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

            string trimmed = path.TrimEnd(separators);
            string[] split = trimmed.Split(separators);

            if (split.Length <= 1)
                return null;

            return string.Join(separators[0].ToString(), split.AllButLast());
        }
    }
}
