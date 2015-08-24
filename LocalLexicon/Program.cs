using System;
using Gtk;
using Newtonsoft.Json;

namespace LocalLexicon
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            Application.Init();
            MainWindow win = new MainWindow();
            win.Show();
            Application.Run();
        }
    }
}
