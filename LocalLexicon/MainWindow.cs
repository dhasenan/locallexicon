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
        wordListFilter = new TreeModelFilter(wordStore, null);
        wordListFilter.VisibleFunc = SearchFilter;
        wordList.Model = wordListFilter;

        statusbar1.Push(0, "No language loaded");
        currentWord.WordChanged += OnCurrentWordChanged;

        langFileFilter = new FileFilter();
        langFileFilter.AddPattern("*.lang");
    }

    void NotesWindowDirty (bool sortOrderChanged)
    {
        if (_lang != null)
        {
            statusbar1.Push(0, "Modified");
            if (sortOrderChanged)
            {
                ShowSortedWords(_lang);
            }
        }
    }

    TreeModelFilter wordListFilter;
    FileFilter langFileFilter;

    Language _lang;
    bool _dirty;
    string _searchTerm = "";

    bool SearchFilter(TreeModel model, TreeIter iter)
    {
        if (_lang == null)
        {
            return true;
        }
        if (string.IsNullOrWhiteSpace(_searchTerm))
        {
            return true;
        }
        var id = (string) model.GetValue(iter, 1);
        if (id == null)
        {
            return false;
        }
        Guid guid;
        if (Guid.TryParse(id, out guid))
        {
            var word = _lang.Words.Where(x => x.Id == guid).FirstOrDefault();
            if (word == null)
            {
                // Wha? Just show it.
                return true;
            }
            if (word.Gloss.Contains(_searchTerm) || word.Lex.Contains(_searchTerm) || word.Definition.Contains(_searchTerm))
            {
                return true;
            }
        }
        return false;
    }

    void OpenLang(string filename)
    {
        try
        {
            var text = File.ReadAllText(filename);
            var lang = JsonConvert.DeserializeObject<Language>(text);
            lang.Filename = filename;
            ShowLang(lang);
        }
        catch (FileNotFoundException ex)
        {
            statusbar1.Push(0, "File not found: " + filename);
        }
        catch (Exception ex)
        {
            Console.WriteLine("failed to open language file {0}: {1}", filename, ex);
            statusbar1.Push(0, "Failed to open language file " + filename);
        }
    }

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
            sf.AddFilter(langFileFilter);
            sf.AddButton("Save", ResponseType.Accept);
            sf.AddButton("Cancel", ResponseType.Cancel);
            sf.Response += (o, args) =>
            {
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
                sf.Destroy();
            };
            sf.Show();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_lang.Name))
            {
                _lang.Name = System.IO.Path.GetFileNameWithoutExtension(_lang.Filename);
            }
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
        md.Response += (o, args) =>
        {
            if (args.ResponseId == ResponseType.Yes)
            {
                Save(after);
            }
            else if (after != null)
            {
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
            var guidStr = (string)wordListFilter.GetValue(iter, 1);
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

    private void SaveIfDirtyThen(System.Action after)
    {
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
        SaveIfDirtyThen(() =>
            {
                var fd = new FileChooserDialog("Open language", this, FileChooserAction.Open);
                fd.AddButton("Open", ResponseType.Accept);
                fd.AddButton("Cancel", ResponseType.Cancel);
                fd.AddFilter(langFileFilter);
                fd.Response += (o, args) =>
                {
                    var filename = fd.Filename;
                    fd.Destroy();
                    if (args.ResponseId == ResponseType.Accept)
                    {
                        OpenLang(filename);
                    }
                };
                fd.Show();
            });
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
        wordStore.SetValues(iter, "", word.Id.ToString());
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
        if (_lang != null)
        {
            var notesWindow = new LangNotesWindow();
            notesWindow.ShowLang(_lang);
            notesWindow.Dirty += NotesWindowDirty;
        }
    }

    protected void OnButton2Activated(object sender, EventArgs e)
    {
        Search();
    }

    protected void OnSearchEntryActivated(object sender, EventArgs e)
    {
        Search();
    }

    void Search()
    {
        _searchTerm = searchEntry.Text;
        wordListFilter.Refilter();
    }
}
