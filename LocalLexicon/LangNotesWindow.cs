using System;

namespace LocalLexicon
{
    public partial class LangNotesWindow : Gtk.Window
    {
        public LangNotesWindow()
            : base(Gtk.WindowType.Toplevel)
        {
            this.Build();
            this.notesText.Buffer.Changed += NotesTextBufferChanged;
        }

        Language _lang;

        public void ShowLang(Language lang)
        {
            _lang = null;
            if (lang == null)
            {
                nameEntry.Text = "";
                alphaOrderEntry.Text = "";
                notesText.Buffer.Text = "";
                nameEntry.IsEditable = false;
                alphaOrderEntry.IsEditable = false;
                notesText.Editable = false;
            }
            else
            {
                nameEntry.Text = lang.Name;
                alphaOrderEntry.Text = lang.AlphabetOrder;
                notesText.Buffer.Text = lang.Notes;
                nameEntry.IsEditable = true;
                alphaOrderEntry.IsEditable = true;
                notesText.Editable = true;
            }
            _lang = lang;
        }

        public event Action<bool> Dirty;

        void FireDirty(bool resort)
        {
            if (_lang == null)
            {
                return;
            }
            var dirty = Dirty;
            if (dirty != null)
            {
                dirty(resort);
            }
        }

        protected void NotesTextBufferChanged (object sender, EventArgs e)
        {
            if (_lang != null)
            {
                _lang.Notes = notesText.Buffer.Text;
                FireDirty(false);
            }
        }

        protected void OnNameEntryChanged(object sender, EventArgs e)
        {
            if (_lang != null)
            {
                _lang.Name = nameEntry.Text;
                FireDirty(false);
            }
        }

        protected void OnAlphaOrderEntryChanged(object sender, EventArgs e)
        {
            if (_lang != null)
            {
                _lang.AlphabetOrder = alphaOrderEntry.Text;
                FireDirty(false);
            }
        }

        protected void OnAlphaOrderEntryEditingDone(object sender, EventArgs e)
        {
            if (_lang != null)
            {
                _lang.AlphabetOrder = alphaOrderEntry.Text;
                FireDirty(true);
            }
        }
    }
}

