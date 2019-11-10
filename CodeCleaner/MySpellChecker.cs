using System;
using System.Linq;
using System.Text.RegularExpressions;
using LAIR.ResourceAPIs.WordNet;

namespace CodeCleaner
{
   static class MySpellChecker
    {
        private static WordNetEngine WordNetEngine = new WordNetEngine(Environment.CurrentDirectory + @"\resources\", false);
        private static string wordToCheck;
        private static readonly NetSpell.SpellChecker.Dictionary.WordDictionary oDict = new NetSpell.SpellChecker.Dictionary.WordDictionary();
        private static NetSpell.SpellChecker.Spelling oSpell = new NetSpell.SpellChecker.Spelling();
        private static int counter;

        public static string SplitCamel(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        public static int CheckWords(string[] Words)
        {
            counter = 0;
            if (Words != null)
            {
                foreach (string W in Words)
                {
                    wordToCheck = W;
                    oSpell.Dictionary = oDict;

                    if (oSpell.TestWord(wordToCheck))
                    {
                        if (wordToCheck.Length == 1 && Words.Length == 1)
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
            }
            else
                throw new Exception("The function input is null!");

            return counter;
        }

        public static bool HasCorrectSpell(string word)
        {
            oDict.DictionaryFile = @"..\..\..\packages\NetSpell.2.1.7\dic\en-US.dic";
            oDict.Initialize();
            wordToCheck = word;
            string CamelChecker = wordToCheck.SplitCamel();
            string[] Words = CamelChecker.Split(' ');
            counter = CheckWords(Words);
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
                    counter = CheckWords(Words);
                    if (counter == Words.Length)
                        return true;
                }

                return false;
            }

        }

        public static bool IsVerb(string word)
        {
            var verbPos = WordNetEngine.POS.Verb;
            SynSet synSet = WordNetEngine.GetMostCommonSynSet(word, verbPos);
            if (synSet == null)
                return false;
            return true;
        }
    }
}
