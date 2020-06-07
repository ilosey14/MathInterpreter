# Math Interpreter

*VS 15, .NET 4.6.1*

This project challenged me to teach myself about interpreters, having no prior knowledge.

## Goals

My goal for this project was to make a working interpreter with as little knowledge as possible.
It had to create reusable logic capable of evaluating a parsed expression multiple times.
This is useful when evaluating a variable expression over a domain.

## Development

### Past Iterations
1. Tokenize and evalaute each term one by one
   - Must re-parse the raw expression each time
   - Inefficient when evaluating the same expression over a variable domain
2. Tokenize and compile instruction set
   - Good because a stand-alone set of instructions can return a value referencing a variable value location in which the value can change
   - No real direction for compiling tokens as well as poorly described token types
3. Tokenize and compile to struction set with order of operations
   - Now we can define tokens appropriately and parse accordingly
   - Have priority/direction when searching for tokens

I got off the ground after finally read up on how and why tokenizing as a separate pass is important to parsing.
From there I developed the logic to build a set of basic instructions which would be evaluated.

### Next Iteration / Refactor
- Implement refined lexical parsing by defined finite token states
- Also implement finite-state grammar in the compile pass

## Known Issues

The current build logic is flawed when handling functions in certain orders.

Test examples:
- `@x = 1, 1 + exp x` where `1 + exp 1` does work correctly
- `exp(2) + exp(3)`

## Current Shortfalls

The current shortfalls, as mentioned in the development section, involve a lack of proper lexing and grammar.
Also, many working aspects rely on hard-coded logic rather than clearly defined, modular algorithms.

The next working version should contain more robust and clear methods for parsing, compiling, and executing.
