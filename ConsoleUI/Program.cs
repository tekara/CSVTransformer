using System;
using CSVLibrary;

namespace ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = System.IO.Directory.GetCurrentDirectory() + "\\ConsoleUI\\configuration.json";          
            CSVConfig config = new CSVConfig(path);

            string pathIn = System.IO.Directory.GetCurrentDirectory() + "\\ConsoleUI\\exampleIn.csv"; 
            string pathOut = System.IO.Directory.GetCurrentDirectory() + "\\ConsoleUI\\exampleOut.csv"; 

            try {
                CSVCleaner cleaner = new CSVCleaner(config, pathIn, pathOut);
                cleaner.Transform();
            }
            catch (Exception e){
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
