using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LocalLexicon
{
    [JsonObject(MemberSerialization.Fields)]
    public class Language
    {
        public string Name = "";
        public string AlphabetOrder = "";
        public string Notes = "";
        public readonly List<Word> Words = new List<Word>();

        /// <summary>
        /// The filename is set when we read the language from disk.
        /// </summary>
        [NonSerialized]
        public string Filename;

        public int CompareWords(Word a, Word b) {
            int d;
            if (string.IsNullOrWhiteSpace(AlphabetOrder))
            {
                d = a.Lex.CompareTo(b.Lex);
                if (d != 0)
                {
                    return d;
                }
            }
            else
            {
                for (int i = 0; i < a.Lex.Length && i < b.Lex.Length; i++)
                {
                    var ia = AlphabetOrder.IndexOf(char.ToLower(a.Lex[i]));
                    var ib = AlphabetOrder.IndexOf(char.ToLower(b.Lex[i]));
                    d = ia.CompareTo(ib);
                    if (d != 0)
                    {
                        return d;
                    }
                }
            }
            d = a.Lex.Length.CompareTo(b.Lex.Length);
            if (d != 0)
            {
                return d;
            }
            return a.Gloss.CompareTo(b.Gloss);
        }
    }
}

