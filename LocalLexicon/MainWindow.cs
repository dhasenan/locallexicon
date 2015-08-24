using System;
using System.Linq;
using Gtk;
using LocalLexicon;
using Newtonsoft.Json;
using System.IO;

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
        lexColumn.Title = "Con Word";
        wordList.AppendColumn(lexColumn);
        wordList.Model = wordStore;
        statusbar1.Push(0, "No language loaded");
        currentWord.WordChanged += OnCurrentWordChanged;
    }

    Language _lang;

    bool _dirty;

    public void ShowLang(Language lang)
    {
        _lang = lang;
        statusbar1.Push(0, string.Format("Opening language {0}", _lang.Name));
        wordStore.Clear();
        foreach (var word in lang.Words)
        {
            wordStore.AppendValues(word.Lex, word.Id.ToString());
        }
        statusbar1.Push(0, string.Format("Opened {0}", _lang.Name));
    }

    void Save()
    {
        if (_lang == null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(_lang.Filename))
        {
            var sf = new FileChooserDialog("Save As", this, FileChooserAction.Save);
            sf.AddButton("Save", ResponseType.Accept);
            sf.AddButton("Cancel", ResponseType.Cancel);
            sf.Response += SaveAs;
            sf.Show();
        }
        else
        {
            string json = JsonConvert.SerializeObject(_lang, Formatting.Indented);
            File.WriteAllText(_lang.Filename, json);
            _dirty = false;
            statusbar1.Push(0, string.Format("Saved as {0}", _lang.Filename));
        }
    }

    void SaveAs(object o, ResponseArgs args)
    {
        var sf = (FileChooserDialog)o;
        switch (args.ResponseId)
        {
            case ResponseType.Yes:
            case ResponseType.Accept:
            case ResponseType.Apply:
            case ResponseType.Ok:
                _lang.Filename = sf.Filename;
                Save();
                break;
            default:
                break;
        }
        sf.Destroy();
    }

    void OnCurrentWordChanged(Word word, bool sortOrderMaybeChanged)
    {
        _dirty = true;
        statusbar1.Push(0, string.Format("Modified."));
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        if (_dirty)
        {
            a.RetVal = false;
            var md = new MessageDialog(
                         this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, "You have unsaved changes. Save before quitting?");
            md.Response += ConfirmQuit;
            md.Show();
        }
        else
        {
            Application.Quit();
            a.RetVal = true;
        }
    }

    void ConfirmQuit(object o, ResponseArgs args)
    {
        if (args.ResponseId == ResponseType.Yes)
        {
            Save();
        }
        Application.Quit();
    }

    protected void OnWordListCursorChanged(object sender, EventArgs e)
    {
        // This one fires when a person clicks a row
        TreeIter iter;
        if (wordList.Selection.GetSelected(out iter))
        {
            var guidStr = (string)wordStore.GetValue(iter, 1);
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
        Save();
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
