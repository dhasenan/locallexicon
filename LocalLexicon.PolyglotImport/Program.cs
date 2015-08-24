using System;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace LocalLexicon.PolyglotImport
{
    class MainClass
    {
        static XName word = XName.Get("word");
        static XName alphaOrder = XName.Get("alphaOrder");
        static XName langName = XName.Get("langName");
        static XName classNotes = XName.Get("classNotes");
        static XName conWord = XName.Get("conWord");
        static XName localWord = XName.Get("localWord");
        static XName definition = XName.Get("definition");


        public static void Main(string[] args)
        {
            var lang = new Language();
            using (ZipArchive archive = new ZipArchive(File.Open(args[0], FileMode.Open)))
            {
                var entry = archive.GetEntry("PGDictionary.xml");
                var text = new StreamReader(entry.Open()).ReadToEnd();
                var doc = XDocument.Parse(text);
                lang.AlphabetOrder = Contents(doc.Root, alphaOrder);
                lang.Name = Contents(doc.Root, langName);
                foreach (var elem in doc.Descendants(word))
                {
                    lang.Words.Add(ParseWord(elem));
                }
                foreach (var elem in doc.Descendants(classNotes))
                {
                    lang.Notes += elem.Value;
                    lang.Notes += "\n\n";
                }
                lang.Notes = lang.Notes.Trim();
            }
            File.WriteAllText(args[1], JsonConvert.SerializeObject(lang, Formatting.Indented));
        }

        static Word ParseWord(XElement elem)
        {
            var lex = Contents(elem, conWord);
            var gloss = Contents(elem, localWord);
            var def = Contents(elem, definition);
            return new Word { Lex = lex, Gloss = gloss, Definition = def };
        }

        static string Contents(XElement elem, XName name)
        {
            var kid = elem.Descendants(name).FirstOrDefault();
            if (kid == null)
            {
                return "";
            }
            return kid.Value;
        }
    }
}
