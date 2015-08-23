using System;
using Gtk;
using Newtonsoft.Json;

namespace LocalLexicon
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            var lang = new Language();
            lang.AlphabetOrder = "bpvfwdtðþsƹkghaāeēoui";
            lang.Name = "Sturnan";
            lang.Notes = "Here are language notes!";
            lang.Words.Add(new Word {
                Id = Guid.NewGuid(),
                Lex = "pehur",
                Gloss = "fire",
                Definition = "Exothermic oxidation reactions resulting in the generation of visible light.\nAlso, hot."
            });
            lang.Words.Add(new Word {
                Id = Guid.NewGuid(),
                Lex = "ðivros",
                Gloss = "beaver",
                Definition = "Cute critters with slappy tails."
            });
            Console.WriteLine("{0}", JsonConvert.SerializeObject(lang, Formatting.Indented));
            
            Application.Init();
            MainWindow win = new MainWindow();
            win.ShowLang(lang);
            win.Show();
            Application.Run();
        }
    }
}
