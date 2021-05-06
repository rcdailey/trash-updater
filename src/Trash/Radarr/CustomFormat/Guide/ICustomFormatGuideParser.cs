using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trash.Radarr.CustomFormat.Guide
{
    public interface ICustomFormatGuideParser
    {
        Task<string> GetMarkdownData();
        IList<CustomFormatData> ParseMarkdown(string markdown);
    }
}
