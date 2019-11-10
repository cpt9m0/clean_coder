using System;

namespace CodeCleaner
{    
    class Program
    {
        static void Main()
        {
            Cleaner cleaner = new Cleaner();
            cleaner.Run();
            Console.WriteLine("END");

            Console.ReadKey();
        }
    }
}
