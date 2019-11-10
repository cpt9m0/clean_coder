using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeCleaner
{
    class Cleaner
    {
        public struct Position
        {
            public override bool Equals(object obj)
            {
                Position mys = (Position)obj;
                return ((mys.col == this.col) && (mys.row == this.row));
            }
            public int row;
            public int col;
        }

        Dictionary<Position, string> Identifiers = new Dictionary<Position, string>();
        Dictionary<Position, string> Functions = new Dictionary<Position, string>();
        Dictionary<Position, string> ForIndexes = new Dictionary<Position, string>();

        static string code = System.IO.File.ReadAllText(@"..\..\source_code.txt");
        static SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        static SyntaxNode root = tree.GetRoot();

        public void CheckFunctionScope(MethodDeclarationSyntax function)
        {
            FileLinePositionSpan linePositionSpan = function.Identifier.GetLocation().GetLineSpan();
            BlockSyntax body = function.Body;
            FileLinePositionSpan fileLinePositionSpan = function.SyntaxTree.GetMappedLineSpan(body.Span);
            int start = fileLinePositionSpan.StartLinePosition.Line + 1;
            int end = fileLinePositionSpan.EndLinePosition.Line;
            int scope = end - start;

            if (scope > 24)
            {
                Console.WriteLine("function {0} in row {1} and col {2} has more than 24 lines.",
                    function.Identifier.ValueText, linePositionSpan.StartLinePosition.Line + 1, linePositionSpan.StartLinePosition.Character + 1);
            }
        }

        public void CheckFunctionInputs(MethodDeclarationSyntax function)
        {
            FileLinePositionSpan linePositionSpan = function.Identifier.GetLocation().GetLineSpan();
            int count = function.ParameterList.Parameters.Count;
            if (count > 4)
            {
                Console.WriteLine("function '{0}' in row {1} and col {2} has more than 4 arguments.",
                    function.Identifier.ValueText, linePositionSpan.StartLinePosition.Line + 1, linePositionSpan.StartLinePosition.Character + 1);
            }
        }

        public void CheckFunctions(IEnumerable<MethodDeclarationSyntax> functions)
        {
            foreach (var function in functions)
            {
                FileLinePositionSpan linePositionSpan = function.Identifier.GetLocation().GetLineSpan();
                BlockSyntax body = function.Body;
                FileLinePositionSpan fileLinePositionSpan = function.SyntaxTree.GetMappedLineSpan(body.Span);
                CheckFunctionScope(function);
                CheckFunctionInputs(function);
                Position position = new Position
                {
                    row = linePositionSpan.StartLinePosition.Line + 1,
                    col = linePositionSpan.StartLinePosition.Character + 1
                };
                Functions.Add(position, function.Identifier.ToString());
            }
            foreach (var item in Functions)
            {
                string line = MySpellChecker.SplitCamel(item.Value);
                string[] words = line.Split(' ');
                if (!MySpellChecker.HasCorrectSpell(item.Value))
                    Console.WriteLine("Please Check '{0}' in row {1} and col {2}", item.Value, item.Key.row, item.Key.col);
                if (item.Value != "Main")
                    if (!MySpellChecker.IsVerb(words[0]))
                        Console.WriteLine("\'{0}\' in row {1} and col {2} is not a verb", item.Value, item.Key.row, item.Key.col);
            }
        }

        public void CheckVariables(IEnumerable<VariableDeclarationSyntax> variables)
        {
            foreach (var item in variables)
            {
                foreach (var i in item.Variables)
                {
                    SyntaxToken token = i.Identifier;
                    Position position = new Position
                    {
                        row = token.GetLocation().GetLineSpan().StartLinePosition.Line,
                        col = token.GetLocation().GetLineSpan().StartLinePosition.Character
                    };
                    if (!ForIndexes.ContainsKey(position))
                        Identifiers.Add(position, token.ToString());
                }
            }
        }

        public void  CheckIdentifiers(IEnumerable<IdentifierNameSyntax> identifiers)
        {
            foreach (var item in identifiers)
            {
                SyntaxToken id_ = item.Identifier;
                Position position = new Position
                {
                    row = id_.GetLocation().GetLineSpan().StartLinePosition.Line,
                    col = id_.GetLocation().GetLineSpan().StartLinePosition.Character
                };
                if (!ForIndexes.ContainsKey(position))
                    Identifiers.Add(position, id_.ToString());
            }
            
            foreach (var item in Identifiers)
            {
                if (!MySpellChecker.HasCorrectSpell(item.Value))
                    Console.WriteLine("Please Check '{0}' in row {1} and col {2}", item.Value, item.Key.row, item.Key.col);
            }
        }

        public void AddForLoopIndexes(IEnumerable<ForStatementSyntax> forStatements)
        {
            foreach (var forLoop in forStatements)
            {
                foreach (var variable in forLoop.Declaration.Variables)
                {
                    var id_ = variable.Identifier;
                    string varName = id_.ToString();
                    Position position = new Position
                    {
                        row = id_.GetLocation().GetLineSpan().StartLinePosition.Line,
                        col = id_.GetLocation().GetLineSpan().StartLinePosition.Character
                    };
                    ForIndexes.Add(position, variable.Identifier.ToString());
                    var body = forLoop.DescendantNodes().OfType<IdentifierNameSyntax>();
                    foreach (var i in body)
                    {
                        if (i.Identifier.ToString() == varName)
                        {
                            Position position2 = new Position
                            {
                                row = i.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line,
                                col = i.Identifier.GetLocation().GetLineSpan().StartLinePosition.Character
                            };
                            ForIndexes.Add(position2, i.Identifier.ToString());
                        }
                    }
                }
            }
        }

        public void Run()
        {
            var forLoops = root.DescendantNodes().OfType<ForStatementSyntax>();
            AddForLoopIndexes(forLoops);

            var functions = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            CheckFunctions(functions);

            var variables = root.DescendantNodes().OfType<VariableDeclarationSyntax>();
            CheckVariables(variables);

            var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();
            CheckIdentifiers(identifiers);

            Console.WriteLine("\nEnd of analysis...");
        }
    }
}
