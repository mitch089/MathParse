using System;
using System.IO;
using System.Threading.Tasks;

namespace MathParse
{
    class Program
    {
        static async Task Main(string[] args)
        {
            String expression;

            do
            {
                Console.Write(">> ");
                expression = Console.ReadLine();

                if (!String.IsNullOrEmpty(expression))
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
            }
            while(!String.IsNullOrEmpty(expression));
        }
    }
}
