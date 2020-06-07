using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Text.RegularExpressions;
//using System.Threading.Tasks;

namespace MathInterpreter
{
    class Expression
    {
        /// <summary>
        /// Initializes a new instance of the Expression class.
        /// </summary>
        public Expression() { }

        /// <summary>
        /// Initializes a new instance of the Expression class.
        /// </summary>
        /// <param name="value">String representation of a mathematical expression</param>
        public Expression(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the Expression class.
        /// </summary>
        /// <param name="value">String representation of a mathematical expression</param>
        /// <param name="variables">List of variables present in the expression</param>
        public Expression(string value, params string[] variables)
        {
            Value = value;

            // add new variables
            foreach (string variable in variables)
                Variables.Add(new Keyword(variable));

            // allocate space to hold variable values in instruction pointer
            pIndex = (uint)Variables.Count;
        }

        private uint pIndex = 0;

        private readonly List<IToken> tokens = new List<IToken>();

        private InstructionSet instructions = new InstructionSet();

        private readonly Hashtable tokenTable = new Hashtable()
        {
            [TokenType.Number]           = new Regex(@"[\d\.]", RegexOptions.Compiled),
            [TokenType.Exponent]         = new Regex(@"\^", RegexOptions.Compiled),
            [TokenType.Multiply]         = new Regex(@"\*", RegexOptions.Compiled),
            [TokenType.Divide]           = new Regex(@"\/", RegexOptions.Compiled),
            [TokenType.Add]              = new Regex(@"\+", RegexOptions.Compiled),
            [TokenType.Subtract]         = new Regex(@"\-", RegexOptions.Compiled),
            [TokenType.LeftParenthesis]  = new Regex(@"\(", RegexOptions.Compiled),
            [TokenType.RightParenthesis] = new Regex(@"\)", RegexOptions.Compiled),
            [TokenType.Separator]        = new Regex(@"\,", RegexOptions.Compiled),
            [TokenType.Word]             = new Regex(@"[A-Za-z]", RegexOptions.Compiled)
        };

        /// <summary>Expression value</summary>
        public string Value { get; private set; }

        /// <summary>List of variables present in the expression.</summary>
        public List<Keyword> Variables { get; } = new List<Keyword>();

        /// <summary>
        /// Evaluates the set expression and returns a value
        /// </summary>
        /// <returns></returns>
        public double Evaluate()
        {
            if (instructions.Count <= 0)
            {
                Parse();
                Compile(tokens);
            }

            if (Variables.Count > 0)
                throw new Exception("Cannot evaluate variable expression with undefined variable values.");

            return instructions.Evaluate();
        }

        /// <summary>
        /// Evaluates the set expression at the provided variable values
        /// and returns a value
        /// </summary>
        /// <param name="variableValues"></param>
        /// <returns></returns>
        public double Evaluate(Dictionary<string, double> variableValues)
        {
            if (instructions.Count <= 0)
            {
                Parse();
                Compile(tokens);
            }

            // convert from name-value to index-value pairs
            if (variableValues.Count > 0)
            {
                int varIndex;
                Dictionary<int, double> vars = new Dictionary<int, double>();

                foreach (KeyValuePair<string, double> pair in variableValues)
                {
                    // find the index of the variable
                    varIndex = Variables.FindIndex(variable => pair.Key == variable.Name);
                    if (varIndex < 0) continue;

                    // index the value at the... index
                    vars.Add(varIndex, pair.Value);
                }

                // evaluate set
                return instructions.Evaluate(vars);
            }

            // evaluate set
            return instructions.Evaluate();
        }

        /// <summary>
        /// Parses/tokenizes the expression string into a token list
        /// </summary>
        private void Parse()
        {
            if (string.IsNullOrEmpty(Value))
                throw new Exception("Cannot parse empty or invalid expression");

            // init
            TokenType? type;
            string buffer;

            // tokenize
            for (int i = 0; i < Value.Length;)
            {
                type = ParseType(Value[i]);

                if (type == null)
                {
                    i++;
                    continue;
                }
                
                buffer = string.Empty;

                if (type == TokenType.Number || type == TokenType.Word)
                {
                    do
                    {
                        buffer += Value[i++];
                    }
                    while (i < Value.Length && type == ParseType(Value[i]));
                }
                else
                    buffer += Value[i++];

                tokens.Add(CreateToken(buffer, type));
            }
        }

        /// <summary>
        /// Finds the token type of the character
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private TokenType? ParseType(char c)
        {
            if (string.IsNullOrWhiteSpace(c.ToString()))
                return null;

            foreach (DictionaryEntry temp in tokenTable)
            {
                if (((Regex)temp.Value).IsMatch(c.ToString()))
                    return (TokenType)temp.Key;
            }

            return null;
        }

        /// <summary>
        /// Creates a new token object
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IToken CreateToken(string buffer, TokenType? type)
        {
            switch (type)
            {
                case TokenType.Number:
                    return new Number(buffer);

                case TokenType.Exponent:
                    return new Token(TokenType.Exponent, TokenGroup.Operator);

                case TokenType.Multiply:
                    return new Token(TokenType.Multiply, TokenGroup.Operator);

                case TokenType.Divide:
                    return new Token(TokenType.Divide, TokenGroup.Operator);

                case TokenType.Add:
                    return new Token(TokenType.Add, TokenGroup.Operator);

                case TokenType.Subtract:
                    return new Token(TokenType.Subtract, TokenGroup.Operator);

                case TokenType.LeftParenthesis:
                    return new Token(TokenType.LeftParenthesis, TokenGroup.Boundary);

                case TokenType.RightParenthesis:
                    return new Token(TokenType.RightParenthesis, TokenGroup.Boundary);

                case TokenType.Word:
                    return new Keyword(buffer);

                case TokenType.Separator:
                    return new Token(TokenType.Separator, TokenGroup.Boundary);
            }

            throw new Exception(
                string.Format("Unexpected token type \"{0}\"", type.ToString()));
        }

        /// <summary>
        /// Compiles the parsed token list into an instruction set
        /// to be evaluated
        /// </summary>
        private uint Compile(List<IToken> tokens)
        {
            if (tokens.Count <= 0)
                throw new Exception("Cannot compile empty or invalid expression.");

            List<IToken> subexpression;
            int subexpIndex = 0;
            int subexpCount = 0;

            // Variables
            int varIndex;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.Variable) continue;

                // find variable reference index
                varIndex = Variables.FindIndex(variable => ((Keyword)tokens[i]).Name == variable.Name);

                if (varIndex < 0)
                    throw new Exception($"Unknown token \"{((Keyword)tokens[i]).Name}\" in expression.");

                // reformat tokens
                tokens.RemoveAt(i);
                tokens.Insert(i, new Reference((uint)varIndex));
            }

            // Functions
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.Function) continue;
                // for a single argument function without parenthesis,
                // the next token after the function name will be a value
                else if (tokens[i + 1].Group == TokenGroup.Value)
                {
                    // instructions
                    instructions.Add(new Func(((Keyword)tokens[i]).Name, (IValue)tokens[i + 1]));

                    // reformat tokens
                    tokens.RemoveRange(i, 2);
                    tokens.Insert(i, new Reference(pIndex));

                    pIndex++;
                    i++;
                    continue;
                }
                // for functions with 1 to N args,
                // parentheses are required
                else if (tokens[i + 1].Type == TokenType.LeftParenthesis)
                {
                    string funcName = ((Keyword)tokens[i]).Name;

                    subexpIndex = ++i;
                    subexpression = GetSubexpression(ref tokens, ref i);
                    subexpCount = subexpression.Count + 2;

                    // split args by separators
                    int argIndex = 0;
                    List<List<IToken>> argsBuffer = new List<List<IToken>>();
                    List<IValue> args = new List<IValue>();

                    argsBuffer.Add(new List<IToken>());

                    // group by function args
                    foreach (IToken token in subexpression)
                    {
                        if (token.Type == TokenType.Separator)
                        {
                            argIndex++;
                            argsBuffer.Add(new List<IToken>());
                        }
                        else
                        {
                            argsBuffer[argIndex].Add(token);
                        }
                    }

                    // compile multi-token args
                    foreach (List<IToken> buffer in argsBuffer)
                    {
                        if (buffer.Count > 1)
                        {
                            pIndex = Compile(buffer);
                            args.Add(new Reference(pIndex++));
                        }
                        else if (buffer.Count == 1 && buffer[0].Group == TokenGroup.Value)
                        {
                            args.Add((IValue)buffer[0]);
                        }
                    }

                    // instructions
                    instructions.Add(new SetP(pIndex));
                    instructions.Add(new Func(funcName, args));

                    // reformat tokens
                    tokens.RemoveRange(subexpIndex - 1, subexpCount + 1);
                    tokens.Insert(subexpIndex - 1, new Reference(pIndex));
                    pIndex++;
                    i++;
                }
            }

            // Parentheses
            // compile sub-expressions and replace them with a reference
            // to their pointer location
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.LeftParenthesis) continue;

                // grab sub expression
                subexpIndex = i;
                subexpression = GetSubexpression(ref tokens, ref i);
                subexpCount = subexpression.Count + 2;

                if (subexpCount <= 1)
                    throw new Exception("Cannot compile invalid expression. (Expression.Compile//parentheses)");

                // compile and reformat tokens
                pIndex = Compile(subexpression);
                tokens.RemoveRange(subexpIndex, subexpCount);
                tokens.Insert(subexpIndex, new Reference(pIndex));
                
                i = subexpIndex;
                pIndex++;
            }

            // Exponents
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.Exponent)
                    continue;
                else if (i < 1 || i >= tokens.Count - 1)
                    throw new Exception("Unexpected operator \"^\" at expression boundary.");
                else if (tokens[i - 1].Group != TokenGroup.Value || tokens[i + 1].Group != TokenGroup.Value)
                    throw new Exception("Invalid value passed to \"^\".");

                // instructions
                IValue arg1 = (IValue)tokens[i - 1];
                IValue arg2 = (IValue)tokens[i + 1];

                instructions.Add(new SetP(pIndex));
                instructions.Add(new Add(arg1));
                instructions.Add(new Pow(arg2));

                // reformat tokens
                tokens.RemoveRange(i - 1, 3);
                tokens.Insert(i - 1, new Reference(pIndex));
                pIndex++;
            }

            // Multiplication/Division
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.Multiply && tokens[i].Type != TokenType.Divide)
                    continue;
                else if (i < 1 || i >= tokens.Count - 1)
                    throw new Exception("Unexpected operator at expression boundary. (Expression.Compile//Multi/Div:1)");
                else if (tokens[i - 1].Group != TokenGroup.Value || tokens[i + 1].Group != TokenGroup.Value)
                    throw new Exception("Cannot compile invalid expression. (Expression.Compile//Multi/Div:2)");

                // instructions
                IValue arg1 = (IValue)tokens[i - 1];
                IValue arg2 = (IValue)tokens[i + 1];

                instructions.Add(new SetP(pIndex));
                instructions.Add(new Add(arg1));

                if (tokens[i].Type == TokenType.Multiply)
                    instructions.Add(new Mult(arg2));
                else
                    instructions.Add(new Div(arg2));

                // reformat tokens
                tokens.RemoveRange(i - 1, 3);
                tokens.Insert(i - 1, new Reference(pIndex));
                pIndex++;
            }

            // Addition/Subtraction
            instructions.Add(new SetP(pIndex));

            if (tokens[0].Group == TokenGroup.Value)
            {
                instructions.Add(new Add(((IValue)tokens[0])));
                tokens.RemoveAt(0);
            }

            for (int i = 0; i < tokens.Count; i += 2)
            {
                if (tokens[i].Type != TokenType.Add && tokens[i].Type != TokenType.Subtract)
                    throw new Exception("Cannot compile invalid expression. (Expression.Compile//Add/Sub:1)");
                else if (i >= tokens.Count - 1)
                    throw new Exception("Unexpected operator at expression boundary. (Expression.Compile//Add/Sub:2)");
                else if (tokens[i + 1].Group != TokenGroup.Value)
                    throw new Exception("Cannot compile invalid expression. (Expression.Compile//Add/Sub:3)");

                // instructions
                IValue arg = (IValue)tokens[i + 1];

                if (tokens[i].Type == TokenType.Add)
                    instructions.Add(new Add(arg));
                else
                    instructions.Add(new Sub(arg));
            }

            return pIndex++;
        }

        /// <summary>
        /// Get a subexpression in a set of parentheses.
        /// </summary>
        /// <param name="index">Index of left parenthesis to start at.</param>
        /// <returns></returns>
        private List<IToken> GetSubexpression(ref List<IToken> tokens, ref int index)
        {
            int inPars = 1;
            List<IToken> subexp = new List<IToken>();

            while (++index < tokens.Count)
            {
                if (tokens[index].Type == TokenType.LeftParenthesis)
                    inPars++;
                else if (tokens[index].Type == TokenType.RightParenthesis)
                    inPars--;

                if (inPars <= 0) break;

                subexp.Add(tokens[index]);
            }

            return subexp;
        }
    }

    /// <summary>
    /// Token representing a number
    /// </summary>
    class Number : IValue
    {
        /// <summary>
        /// Initializes a new instance of the Number class
        /// </summary>
        /// <param name="value">Token value</param>
        public Number(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the Number class
        /// </summary>
        /// <param name="value">String representation of the token value</param>
        public Number(string value)
        {
            if (double.TryParse(value, out double result))
                Value = result;
            else
                throw new Exception(
                    string.Format("Unexpected value \"{0}\" in type \"Number\"", value));
        }

        /// <summary>Token's index value.</summary>
        public uint Index { get; private set; }

        /// <summary>Token's numeric value.</summary>
        public double Value { get; private set; }

        public TokenType Type { get; } = TokenType.Number;
        public TokenGroup Group { get; } = TokenGroup.Value;
    }

    /// <summary>
    /// Token representing a pointer value reference
    /// </summary>
    class Reference : IValue
    {
        /// <summary>
        /// Initializes a new instance of the Reference class
        /// </summary>
        /// <param name="index"></param>
        public Reference(uint index)
        {
            Index = index;
        }

        /// <summary>Token's index value.</summary>
        public uint Index { get; private set; }

        /// <summary>Token's numeric value</summary>
        public double Value { get; private set; }

        public TokenType Type { get; } = TokenType.Reference;
        public TokenGroup Group { get; } = TokenGroup.Value;
    }

    /// <summary>
    /// Token representing a keyword such as a
    /// variable or a function name.
    /// </summary>
    class Keyword : IValue
    {
        public Keyword(string name)
        {
            Name = name;
            Type = Functions.Exists(name)
                ? TokenType.Function
                : TokenType.Variable;
            Group = TokenGroup.Value;
        }

        /// <summary>Token's name.</summary>
        public string Name { get; private set; }

        /// <summary>Token's index value.</summary>
        public uint Index { get; set; }

        /// <summary>Token's numeric value.</summary>
        public double Value { get; set; }

        public TokenType Type { get; private set; }
        public TokenGroup Group { get; private set; }
    }

    /// <summary>
    /// Generic token class
    /// </summary>
    class Token : IToken
    {
        public Token(TokenType type, TokenGroup group)
        {
            Type = type;
            Group = group;
        }

        public TokenType Type { get; private set; }
        public TokenGroup Group { get; private set; }
    }

    /// <summary>Interface for a token containing a value.</summary>
    interface IValue : IToken
    {
        uint Index { get; }
        double Value { get; }
    }

    /// <summary>Token interface</summary>
    interface IToken
    {
        TokenType Type { get; }
        TokenGroup Group { get;}
    }

    /// <summary>Groups for similar token types</summary>
    enum TokenGroup
    {
        Value,
        Boundary,
        Operator,
        Keyword
    }

    /// <summary>Token types</summary>
    enum TokenType
    {
        Number,
        Reference,
        LeftParenthesis,
        RightParenthesis,
        Exponent,
        Multiply,
        Divide,
        Add,
        Subtract,
        Word,
        Function,
        Variable,
        Separator
    }
}
