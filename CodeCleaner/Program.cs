using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeCleaner
{
    public static class MySpellChecker
    {
        private static string wordToCheck;
        private static NetSpell.SpellChecker.Dictionary.WordDictionary oDict = new NetSpell.SpellChecker.Dictionary.WordDictionary();
        private static int counter;

        public static string SplitCamel(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        public static int checkWords(string[] Words)
        {
            counter = 0;
            
            foreach (string W in Words)
            {
                wordToCheck = W;
                NetSpell.SpellChecker.Spelling oSpell = new NetSpell.SpellChecker.Spelling();

                oSpell.Dictionary = oDict;
                if (oSpell.TestWord(wordToCheck))
                {
                    if (wordToCheck.Length == 1)
                    {
                        counter -= 1;
                    }
                    else
                    {
                        counter += 1;

                    }

                }
                else
                {
                    counter -= 1;
                }
            }

            return counter;
        }

        public static bool checkSpell(string word)
        {
            oDict.DictionaryFile = @"E:\Dev\C#\CodeCleaner\packages\NetSpell.2.1.7\dic\en-US.dic";
            oDict.Initialize();

            wordToCheck = word;
            string CamelChecker = wordToCheck.SplitCamel();
            string[] Words = CamelChecker.Split(' ');
            
            counter = checkWords(Words);
            
            if (counter == Words.Length)
            {
                return true;
            }
            else
            {
                if (word.Contains('_'))
                {
                    counter = 0;
                    Words = word.Split('_');
                    counter = checkWords(Words);
                    if (counter == Words.Length)
                        return true;
                }

                return false;
            }

        }
    }

    class Cleaner
    {
        public struct Position
        {
            public int row;
            public int col;
        }

        private const string seprator = "\n================================\n";
        Dictionary<Position, string> Identifiers = new Dictionary<Position, string>();

        public Cleaner()
        {
        }

        public void run()
        {
            var hello_world_code = System.IO.File.ReadAllText(@"E:\Dev\C#\CodeCleaner\CodeCleaner\source_code.txt");
            var tree = CSharpSyntaxTree.ParseText(hello_world_code);
            
            var root = tree.GetRoot();
            var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();
            // var functions = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var variables = root.DescendantNodes().OfType<VariableDeclarationSyntax>();
            foreach (var item in variables)
            {
                foreach (var i in item.Variables)
                {
                    SyntaxToken token = i.Identifier;
                    Position position = new Position();
                    position.row = token.GetLocation().GetLineSpan().StartLinePosition.Line;
                    position.col = token.GetLocation().GetLineSpan().StartLinePosition.Character;
                    Identifiers.Add(position, token.ToString());
                }
            }
            
            // Console.WriteLine(seprator);
            
            foreach (var item in identifiers)
            {
                // Console.WriteLine(item.Identifier.ValueText);
                SyntaxToken id_ = item.Identifier;
                Position position = new Position();
                position.row = id_.GetLocation().GetLineSpan().StartLinePosition.Line;
                position.col = id_.GetLocation().GetLineSpan().StartLinePosition.Character;
                Identifiers.Add(position, id_.ToString());
            }

            /*
            Console.WriteLine("\nFunctions: \n");
            foreach (var item in functions)
            {
                Console.WriteLine("Identifier: {0}", item.Identifier);               
                Console.WriteLine("row {0}", item.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line);
                Console.WriteLine("col {0}", item.Identifier.GetLocation().GetLineSpan().StartLinePosition.Character);
            }
            */

            Console.WriteLine("\n");
            foreach(var item in Identifiers)
            {
                if (!MySpellChecker.checkSpell(item.Value))
                    Console.WriteLine("Please Check '{0}' in row {1} and col {2}", item.Value, item.Key.row, item.Key.col);
            }
            Console.WriteLine("End of analysis...");

        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Cleaner cleaner = new Cleaner();
            cleaner.run();

            Console.ReadKey();
        }
    }
}
