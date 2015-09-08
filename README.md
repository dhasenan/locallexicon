LocalLexicon is a simple dictionary manager targeted at constructed languages. It's designed to run on Mono on Linux and uses GTK for a UI.

This is intended as a personal project and isn't necessarily safe for general use. The code is terrible and could use an injection of MVC, better separation of concerns, and probably a switch from Stetic to a monolithic Glade interface description (Stetic seems to be removing action handlers at random, and having subwidgets breaks keyboard accelerators).

While for my own sake I will try to keep the save file format consistent and forward-compatible, I make no guarantees.
