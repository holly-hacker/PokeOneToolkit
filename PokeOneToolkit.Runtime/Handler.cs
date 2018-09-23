using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PokeOneToolkit.Runtime
{
    public static class Handler
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        static Handler()
        {
            AllocConsole();
            Log("Allocated console");
        }

        public static void HandleRecv(ref PSXAPI.Proto proto) => Log("<- " + proto._Name);
        public static void HandleSend(ref PSXAPI.Proto proto) => Log("-> " + proto._Name);

        private static void Log(string s)
        {
            // recreating sw is probably bad, but it only prints if it gets closed (not flushed)
            using (var sw = new StreamWriter(Console.OpenStandardOutput()))
                sw.WriteLine(s);
        }
    }
}
