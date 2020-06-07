using System;
using System.Collections.Generic;

namespace MathInterpreter
{
    /// <summary>
    /// Mathemtaical function dictionary
    /// </summary>
    static class Functions
    {
        private static readonly Dictionary<string, Func<double[], double>> functions = new Dictionary<string, Func<double[], double>>
        {
            ["abs"]   = (double[] x) => Math.Abs(x[0]),
            ["acos"]  = (double[] x) => Math.Acos(x[0]),
            ["asin"]  = (double[] x) => Math.Asin(x[0]),
            ["atan"]  = (double[] x) => Math.Atan(x[0]),
            ["atan2"] = (double[] x) => Math.Atan2(x[0], x[1]),
            ["cos"]   = (double[] x) => Math.Cos(x[0]),
            ["cosh"]  = (double[] x) => Math.Cosh(x[0]),
            ["exp"]   = (double[] x) => Math.Exp(x[0]),
            ["ln"]    = (double[] x) => Math.Log(x[0]),
            ["log"]   = (double[] x) => Math.Log(x[0]) / Math.Log(x[1]),
            ["sign"]  = (double[] x) => Math.Sign(x[0]),
            ["sin"]   = (double[] x) => Math.Sin(x[0]),
            ["sinh"]  = (double[] x) => Math.Sinh(x[0]),
            ["sqrt"]  = (double[] x) => Math.Sqrt(x[0]),
            ["tan"]   = (double[] x) => Math.Tan(x[0]),
            ["tanh"]  = (double[] x) => Math.Tanh(x[0])
        };

        /// <summary>
        /// Evaluates the function with the given values.
        /// Returns null if function does not exist.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Evaluate(string name, double[] values)
        {
            if (!functions.ContainsKey(name))
                throw new Exception($"Function does not exist with name \"{name}\".");

            return functions[name](values);
        }

        /// <summary>
        /// Checks whether the function exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Exists(string name)
        {
            return functions.ContainsKey(name);
        }
    }
}
