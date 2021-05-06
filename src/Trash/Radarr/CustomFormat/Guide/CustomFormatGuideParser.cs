using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using Serilog;
using Trash.Extensions;

namespace Trash.Radarr.CustomFormat.Guide
{
    public class CustomFormatGuideParser : ICustomFormatGuideParser
    {
        private readonly Regex _regexFence = BuildRegex(@"(\s*)```(json)?");
        private readonly Regex _regexPotentialScore = BuildRegex(@"\[(-?[\d]+)\]");
        private readonly Regex _regexScore = BuildRegex(@"score.*?\[(-?[\d]+)\]");

        public CustomFormatGuideParser(ILogger logger)
        {
            Log = logger;
        }

        private ILogger Log { get; }

        public async Task<string> GetMarkdownData()
        {
            return await
                "https://raw.githubusercontent.com/TRaSH-/Guides/master/docs/Radarr/V3/Radarr-collection-of-custom-formats.md"
                    .GetStringAsync();
        }

        public IList<CustomFormatData> ParseMarkdown(string markdown)
        {
            var state = new ParserState();

            var reader = new StringReader(markdown);
            for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                state.LineNumber++;
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                // Always check if we're starting a fenced code block. Whether we are inside one or not greatly affects
                // the logic we use.
                if (_regexFence.Match(line, out Match match))
                {
                    ProcessCodeBlockBoundary(match.Groups, state);
                    continue;
                }

                if (state.CodeBlockIndentation != null)
                {
                    InsideFence_ParseMarkdown(line, state);
                }
                else
                {
                    OutsideFence_ParseMarkdown(line, state);
                }
            }

            return state.Results;
        }

        private static Regex BuildRegex(string regex)
        {
            return new(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private void OutsideFence_ParseMarkdown(string line, ParserState state)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            Match match;

            if (_regexScore.Match(line, out match))
            {
                state.Score = int.Parse(match.Groups[1].Value);
            }
            else if (_regexPotentialScore.Match(line, out match))
            {
                Log.Warning("Found a potential score on line #{Line} that will be ignored because the " +
                            "word 'score' is missing (This is probably a bug in the guide itself): {ScoreMatch}",
                    state.LineNumber, match.Groups[0].Value);
            }
        }

        private void ProcessCodeBlockBoundary(GroupCollection groups, ParserState state)
        {
            if (groups[2].Value == "json")
            {
                state.CodeBlockIndentation = groups[1].Value;
            }
            else
            {
                // Record previously captured JSON data since we're leaving the code block
                var json = state.JsonStream.ToString();
                if (!string.IsNullOrEmpty(json))
                {
                    state.Results.Add(new CustomFormatData {Json = json, Score = state.Score});
                }

                state.ResetParserState();
            }
        }

        private static void InsideFence_ParseMarkdown(string line, ParserState state)
        {
            state.JsonStream.WriteLine(line[state.CodeBlockIndentation!.Length..]);
        }

        // private void OutsideFence_ParseMarkdown(string line, RadarrParserState state)
        // {
        //     // ReSharper disable once InlineOutVariableDeclaration
        //     Match match;
        //
        //     // Header Processing. Never do any additional processing to headers, so return after processing it
        //     if (_regexHeader.Match(line, out match))
        //     {
        //         OutsideFence_ParseHeader(state, match);
        //         // return here if we add more logic below
        //     }
        // }

        // private void OutsideFence_ParseHeader(RadarrParserState state, Match match)
        // {
        //     var headerDepth = match.Groups[1].Length;
        //     var headerText = match.Groups[2].Value;
        //
        //     var stack = state.HeaderStack;
        //     while (stack.Count > 0 && stack.Peek().Item1 >= headerDepth)
        //     {
        //         stack.Pop();
        //     }
        //
        //     if (headerDepth == 0)
        //     {
        //         return;
        //     }
        //
        //     if (state.HeaderStack.TryPeek(out var header))
        //     {
        //         headerText = $"{header.Item2}|{headerText}";
        //     }
        //
        //     Log.Debug("> Process Header: {HeaderPath}", headerText);
        //     state.HeaderStack.Push((headerDepth, headerText));
        // }
    }
}
