using System;
using System.IO;
using System.Threading.Tasks;

namespace MathParse
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Write(">> ");
                String expression = Console.ReadLine();

                if (!String.IsNullOrWhiteSpace(expression))
                {
                    try
                    {
                        var result = await MathParse.Parser.Evaluate(expression, Console.Out);
                        Console.WriteLine($"{expression} = {result}");
                    }
                    catch(Exception ex)
                    { Console.WriteLine($"Error: {ex}"); }

                    Console.WriteLine();
                }
                else
                {
                    break;
                }
            }
        }
    }
}
