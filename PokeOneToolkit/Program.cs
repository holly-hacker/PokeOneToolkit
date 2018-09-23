using System;
using PokeOneToolkit.Tools;

namespace PokeOneToolkit
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("PokeOneToolkit\n");

            Console.WriteLine("Running patcher...");
            new Patcher().Run();

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
