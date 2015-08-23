using System;
using Newtonsoft.Json;

namespace LocalLexicon
{
    [JsonObject(MemberSerialization.Fields)]
    public class Word
    {
        public Guid Id;

        /// <summary>
        /// The lexeme. For instance, if the language is Spanish, this might be "hermana".
        /// </summary>
        public string Lex;

        /// <summary>
        /// The short gloss. For instance, if the lex is "hermana", this might be "sister".
        /// Or if the person using the tool is Norwegian, maybe "søster".
        /// </summary>
        public string Gloss;

        /// <summary>
        /// The long definition.
        /// </summary>
        public string Definition;

        public override string ToString()
        {
            return string.Format("[Word: Id={0}, Lex={1}]", Id, Lex);
        }
    }
}

