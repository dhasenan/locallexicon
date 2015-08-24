using System;

namespace LocalLexicon
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class WordWidget : Gtk.Bin
    {
        public WordWidget()
        {
            this.Build();
            ShowWord(null);
            definitionText.Buffer.Changed += OnDefinitionTextFocusOutEvent;
        }

        private Word _word;

        public event Action<Word, bool> WordChanged;

        string _lexOnEntry;

        private void FireWordChanged(bool sortMaybeChanged)
        {
            if (_word == null)
            {
                return;
            }
            Action<Word, bool> evt = WordChanged;
            if (evt != null)
            {
                evt(_word, sortMaybeChanged);
            }
        }

        public void ShowWord(Word word)
        {
            _word = null; // So we don't accidentally modify it.
            if (word == null)
            {
                this.definitionText.Editable = false;
                this.definitionText.Buffer.Clear();
                this.glossEntry.Text = "";
                this.glossEntry.IsEditable = false;
                this.lexEntry.Text = "";
                this.lexEntry.IsEditable = false;
            }
            else
            {
                this.definitionText.Editable = true;
                this.definitionText.Buffer.Text = word.Definition;
                this.glossEntry.Text = word.Gloss;
                this.glossEntry.IsEditable = true;
                this.lexEntry.Text = word.Lex;
                this.lexEntry.IsEditable = true;
                this.lexEntry.GrabFocus();
            }
            _word = word;
        }

        protected void OnDefinitionTextFocusOutEvent(object o, EventArgs args)
        {
            if (_word == null)
            {
                return;
            }
            var text = this.definitionText.Buffer.Text;
            if (_word.Definition != text)
            {
                _word.Definition = this.definitionText.Buffer.Text;
                FireWordChanged(false);
            }
        }

        protected void OnGlossEntryChanged(object sender, EventArgs e)
        {
            if (_word == null)
            {
                return;
            }
            if (glossEntry.Text != _word.Gloss)
            {
                _word.Gloss = glossEntry.Text;
                FireWordChanged(false);
            }
        }

        protected void OnLexEntryChanged(object sender, EventArgs e)
        {
            if (_word == null)
            {
                return;
            }
            if (lexEntry.Text != _word.Lex)
            {
                _word.Lex = lexEntry.Text;
                FireWordChanged(false);
            }
        }

        protected void OnLexEntryFocusOutEvent(object o, Gtk.FocusOutEventArgs args)
        {
            if (_word == null)
            {
                return;
            }
            if (_word.Lex != _lexOnEntry)
            {
                FireWordChanged(true);
            }
        }

        protected void OnLexEntryFocusInEvent(object o, Gtk.FocusInEventArgs args)
        {
            if (_word != null)
            {
                _lexOnEntry = _word.Lex;
            }
        }
    }
}

