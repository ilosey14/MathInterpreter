using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace MathInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            // prompt expression
            Console.WriteLine("Math Interpreter\nUse \"@<var>=<value>, ...\" to define variables.\n");
            Console.WriteLine("Enter expression:");
            string input = Console.ReadLine();

            // implementation
            //if (args.Length <= 0 || string.IsNullOrWhiteSpace(args[0]))
            //    throw new Exception("Cannot evaluate empty or invalid expresssion.");

            // check for vars
            var pVar = new Regex(@"\@(\w+)\s*=\s*([\d\.]+)\s*(?:,\s*)?");
            var varList = new Dictionary<string, double>();

            var matches = pVar.Matches(input);

            foreach (Match match in matches)
            {
                if (!match.Success) continue;

                varList.Add(match.Groups[1].Value, double.Parse(match.Groups[2].Value));
                input = input.Replace(match.Value, string.Empty);
            }

            // evaluate expression
            var expression = (varList.Count > 0)
                    ? new Expression(input, varList.Keys.ToArray())
                    : new Expression(input);
            object result;

            try
            {
                result = (varList.Count > 0)
                    ? expression.Evaluate(varList)
                    : expression.Evaluate();
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            // show result
            Console.WriteLine("= " + result.ToString());
            Console.WriteLine();

            // ask for another expression
            Main(null);
        }
    }

    /**
     * Exceptions
     */
    
    [Serializable]
    public class UnexpectedSymbolException : Exception
    {
        public UnexpectedSymbolException() { }

        public UnexpectedSymbolException(string token)
            : base("Unexpected symbol " + token) { }
    }

    [Serializable]
    public class ExpectedNumberException : Exception
    {
        public ExpectedNumberException() { }

        public ExpectedNumberException(string symbol)
            : base("Expected number at " + symbol) { }
    }

    [Serializable]
    public class ExpectedOperatorException : Exception
    {
        public ExpectedOperatorException() { }

        public ExpectedOperatorException(string symbol)
            : base("Expected operator at " + symbol) { }
    }

    [Serializable]
    public class InvalidExpressionException : Exception
    {
        public InvalidExpressionException()
            : base("The expression is invalid") { }
    }

    [Serializable]
    public class UnknownTokenException : Exception
    {
        public UnknownTokenException() { }

        public UnknownTokenException(string value)
            : base("Unknown token at " + value) { }
    }
}
