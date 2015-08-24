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
        if (_lang != lang)
        {
            currentWord.ShowWord(null);
        }
        _lang = lang;
        statusbar1.Push(0, string.Format("Opening language {0}", _lang.Name));
        ShowSortedWords(lang);
        statusbar1.Push(0, string.Format("Opened {0}", _lang.Name));
    }

    void ShowSortedWords(Language lang)
    {
        wordStore.Clear();
        lang.Words.Sort(lang.CompareWords);
        foreach (var word in lang.Words)
        {
            wordStore.AppendValues(word.Lex, word.Id.ToString());
        }
    }

    void Save(System.Action after)
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
            sf.Response += (o, args) => {
                switch (args.ResponseId)
                {
                    case ResponseType.Yes:
                    case ResponseType.Accept:
                    case ResponseType.Apply:
                    case ResponseType.Ok:
                        _lang.Filename = sf.Filename;
                        Save(after);
                        break;
                    default:
                        break;
                }
            };
            sf.Show();
        }
        else
        {
            string json = JsonConvert.SerializeObject(_lang, Formatting.Indented);
            File.WriteAllText(_lang.Filename, json);
            _dirty = false;
            statusbar1.Push(0, string.Format("Saved as {0}", _lang.Filename));
            if (after != null)
            {
                after();
            }
        }
    }

    void OnCurrentWordChanged(Word word, bool sortOrderMaybeChanged)
    {
        SetDirty();
        if (sortOrderMaybeChanged)
        {
            ShowSortedWords(_lang);
        }
        else
        {
            TreeIter iter;
            if (wordList.Selection.GetSelected(out iter))
            {
                wordStore.SetValue(iter, 0, word.Lex);
            }
        }
    }

    void PromptToSave(System.Action after)
    {
        var md = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, "You have unsaved changes. Save first?");
        md.Response += (o, args) => {
            if (args.ResponseId == ResponseType.Yes) {
                Save(after);
            } else if (after != null) {
                after();
            }
        };
        md.Show();
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        if (_dirty)
        {
            a.RetVal = false;
            PromptToSave(() => Application.Quit());
        }
        else
        {
            Application.Quit();
            a.RetVal = true;
        }
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
        else
        {
            Console.WriteLine("no selection found");
        }
    }

    private void SaveIfDirtyThen(System.Action after) {
        if (_dirty)
        {
            PromptToSave(after);
        }
        else
        {
            after();
        }
    }

    protected void OnNewActionActivated(object sender, EventArgs e)
    {
        SaveIfDirtyThen(() => ShowLang(new Language()));
    }

    protected void OnOpenActionActivated(object sender, EventArgs e)
    {
        SaveIfDirtyThen(() => ShowLang(new Language()));
    }

    protected void OnSaveActionActivated(object sender, EventArgs e)
    {
        Save(null);
    }

    protected void OnNewWordActionActivated(object sender, EventArgs e)
    {
        if (_lang == null)
        {
            return;
        }
        var word = new Word();
        _lang.Words.Add(word);
        var iter = wordStore.Insert(0);
        wordStore.SetValues(iter, "", word.Id);
        currentWord.ShowWord(word);
        wordList.Selection.SelectIter(iter);
        SetDirty();
    }

    void SetDirty()
    {
        _dirty = true;
        statusbar1.Push(0, "Modified");
    }

    protected void OnDeleteWordActionActivated(object sender, EventArgs e)
    {
    }

    protected void OnLanguagePropertiesActionActivated(object sender, EventArgs e)
    {
    }
}
