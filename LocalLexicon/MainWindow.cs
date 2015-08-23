using System;
using System.Linq;
using Gtk;
using LocalLexicon;

public partial class MainWindow: Gtk.Window
{
    ListStore wordStore;

    public MainWindow()
        : base(Gtk.WindowType.Toplevel)
    {
        Build();
        wordStore = new ListStore(typeof(string), typeof(string));
        var lexColumn = new TreeViewColumn();
        var cellRendererText = new CellRendererText();
        lexColumn.PackStart(cellRendererText, true);
        lexColumn.AddAttribute(cellRendererText, "text", 0);
        wordList.AppendColumn(lexColumn);
        wordList.Model = wordStore;
    }

    Language _lang;

    public void ShowLang(Language lang)
    {
        _lang = lang;
        wordStore.Clear();
        foreach (var word in lang.Words)
        {
            wordStore.AppendValues(word.Lex, word.Id.ToString());
        }
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void OnWordListCursorChanged(object sender, EventArgs e)
    {
        // This one fires when a person clicks a row
        //throw new NotImplementedException();
        TreeIter iter;
        if (wordList.Selection.GetSelected(out iter))
        {
            var guidStr = (string) wordStore.GetValue(iter, 1);
            var guid = Guid.Parse(guidStr);
            var word = _lang.Words.Where(x => x.Id == guid).FirstOrDefault();
            Console.WriteLine("found word {0}", word);
            this.currentWord.ShowWord(word);
        }
    }

    protected void OnNewActionActivated(object sender, EventArgs e)
    {
    }

    protected void OnOpenActionActivated(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected void OnSaveActionActivated(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected void OnNewWordActionActivated(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected void OnDeleteWordActionActivated(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected void OnLanguagePropertiesActionActivated(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}
