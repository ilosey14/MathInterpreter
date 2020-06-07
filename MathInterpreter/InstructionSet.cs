using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace MathInterpreter
{
    class InstructionSet : List<IInstruction>
    {
        private Pointer p;

        /// <summary>
        /// Evaluates the instruction set
        /// </summary>
        /// <returns></returns>
        public double Evaluate()
        {
            if (Count <= 0)
                throw new Exception("Cannot evaluate empty or invalid expression.");

            p = new Pointer();

            foreach (var i in this)
                i.Evaluate(ref p);

            return p.Value;
        }

        /// <summary>
        /// Evaluates the instruction set for defined variable values.
        /// </summary>
        /// <param name="vars">
        /// Index-value pairs where the index references the pointer
        /// location where the value of the variable is stored.
        /// </param>
        /// <returns></returns>
        public double Evaluate(Dictionary<int, double> vars)
        {
            if (Count <= 0)
                throw new Exception("Cannot evaluate empty or invalid expression.");

            p = new Pointer();

            // define variable values
            foreach (KeyValuePair<int, double> var in vars)
            {
                p.Index = (uint)var.Key;
                p.Value = var.Value;
            }

            // evaluate set
            foreach (var i in this)
                i.Evaluate(ref p);

            return p.Value;
        }
    }

    /// <summary>
    /// Instruction set memory container
    /// </summary>
    class Pointer
    {
        private readonly List<double> values = new List<double>() { 0 };

        /// <summary>Gets or set the current pointer index</summary>
        public uint Index
        {
            get => _Index;
            set
            {
                if (value < 0)
                {
                    _Index = 0;
                }
                else
                {
                    if (value >= values.Count)
                    {
                        for (int i = values.Count; i <= value; i++)
                            values.Add(0);
                    }
                    
                    _Index = value;
                }
            }
        }
        private uint _Index = 0;

        /// <summary>Gets or sets the current pointer value</summary>
        public double Value
        {
            get => values[(int)_Index];
            set => values[(int)_Index] = value;
        }

        /// <summary>
        /// Gets a pointer values at an index.
        /// Kind of cheating but whatever
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double GetAt(uint index)
        {
            return values[(int)index];
        }
    }

    /// <summary>
    /// Set pointer location
    /// </summary>
    class SetP : IInstruction
    {
        private readonly uint index;

        public SetP(uint index)
        {
            this.index = index;
        }

        public bool IsRef { get; } = true;

        public void Evaluate(ref Pointer p)
        {
            p.Index = index;
        }
    }

    /// <summary>
    /// Adds a values to the pointer location
    /// </summary>
    class Add : Instruction, IInstruction
    {
        public Add(IValue value) : base(value) { }

        public void Evaluate(ref Pointer p)
        {
            p.Value += IsRef ? p.GetAt(Index) : Value;
        }
    }

    /// <summary>
    /// Subtracts a values from the pointer location
    /// </summary>
    class Sub : Instruction, IInstruction
    {public Sub(IValue value) : base(value) { }

        public void Evaluate(ref Pointer p)
        {
            p.Value -= IsRef ? p.GetAt(Index) : Value;
        }
    }

    /// <summary>
    /// Multiplies the pointer location by a value
    /// </summary>
    class Mult : Instruction, IInstruction
    {
        public Mult(IValue value) : base(value) { }

        public void Evaluate(ref Pointer p)
        {
            p.Value *= IsRef ? p.GetAt(Index) : Value;
        }
    }

    /// <summary>
    /// Divides the pointer location by a value
    /// </summary>
    class Div : Instruction, IInstruction
    {
        public Div(IValue value) : base(value) { }

        public void Evaluate(ref Pointer p)
        {
            p.Value /= IsRef ? p.GetAt(Index) : Value;
        }
    }

    /// <summary>
    /// Raises the pointer value to a power
    /// </summary>
    class Pow : Instruction, IInstruction
    {
        public Pow(IValue value) : base(value) { }

        public void Evaluate(ref Pointer p)
        {
            p.Value = Math.Pow(p.Value, IsRef ? p.GetAt(Index) : Value);
        }
    }

    /// <summary>
    /// Evaluates a function on a list of arguments
    /// </summary>
    class Func : IInstruction
    {
        public Func(string name, params IValue[] args)
        {
            this.name = name;
            this.args = new List<IValue>(args);
        }

        public Func(string name, List<IValue> args)
        {
            this.name = name;
            this.args = args;
        }

        readonly string name;
        readonly List<IValue> args;

        public void Evaluate(ref Pointer p)
        {
            // get args
            double[] args = new double[this.args.Count];

            for (int i = 0; i < args.Length; i++)
                args[i] = (this.args[i].Type == TokenType.Number)
                    ? this.args[i].Value
                    : p.GetAt(this.args[i].Index);

            // evaluate function
            try
            {
                p.Value = Functions.Evaluate(name, args);
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("Function"))
                    throw;
                else
                    throw new Exception($"Invalid argumenst for \"{name}\".");
            }
        }
    }

    /// <summary>
    /// Abstract instruction class to be inherited by a
    /// IInstruction class
    /// </summary>
    abstract class Instruction
    {
        public uint Index { get; }
        public  double Value { get; }

        /// <summary>
        /// Whether the instruction references a pointer location
        /// or is a descrete value.
        /// </summary>
        public readonly bool IsRef;

        public Instruction(IValue value)
        {
            if (value.Type == TokenType.Number)
            {
                Value = value.Value;
                IsRef = false;
            }
            else
            {
                Index = value.Index;
                IsRef = true;
            }
        }
    }

    /// <summary>
    /// Instruction object interface
    /// </summary>
    interface IInstruction
    {
        /// <summary>
        /// Evaluate the instruction on a reference
        /// </summary>
        /// <param name="p"></param>
        void Evaluate(ref Pointer p);
    }


    /// <summary>
    /// Possible instruction
    /// </summary>
    enum InstructionType
    {
        SetP,
        GetP,
        Add,
        Sub,
        Mult,
        Div,
        Pow
    }
}
