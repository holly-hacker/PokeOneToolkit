using System;
using System.IO;

namespace PokeOneToolkit.Helpers
{
    internal static class NotQuiteIOC
    {
        public static string LauncherPath => launcherPath.Value;

        private static readonly Lazy<string> launcherPath = new Lazy<string>(() => {
            // TODO: use proper config
            const string unityPath = "unitypath";

            if (File.Exists(unityPath)) return File.ReadAllText(unityPath);

            Console.Write("Pass me the path to the PokeOne launcher: ");
            string proper = PathHelper.GetProperLauncherPath(Console.ReadLine());

            File.WriteAllText(unityPath, proper);

            return proper;
        });
    }
}
