using System.Collections.Generic;
using System.IO;

namespace Trash.Radarr.CustomFormat.Guide
{
    public class ParserState
    {
        public ParserState()
        {
            ResetParserState();
        }

        public int? Score { get; set; }
        public string? CodeBlockIndentation { get; set; }
        public int LineNumber { get; set; }
        public List<CustomFormatData> Results { get; } = new();
        public StringWriter JsonStream { get; } = new();

        public void ResetParserState()
        {
            CodeBlockIndentation = null;
            JsonStream.GetStringBuilder().Clear();
            Score = null;
        }
    }
}
