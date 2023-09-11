using System.Text;
using System.Text.RegularExpressions;

namespace Lc.Linca.Sdk;

/// <summary>
/// Utilities for console/terminal input/output
/// </summary>
internal static class Terminal
{
    internal const string AnsiRemover = @"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])";

    public class Info
    {
        private readonly Regex ansiRemover = new Regex(AnsiRemover);
        private readonly StringBuilder _content = new();

        public void WriteLine(string line)
        {
            _content.AppendLine(line);
        }

        public void HorizontalRule()
        {
            _content.AppendLine("-");
        }

        public void Show()
        {
            var lines = _content.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var maxLen = lines.Max(line => ansiRemover.Replace(line, string.Empty).Length);

            if (maxLen > Console.BufferWidth - 4)
            {
                maxLen = Console.BufferWidth - 4;
            }

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine($"╭{string.Concat(Enumerable.Repeat("─", maxLen + 2))}╮");
            foreach (var line in lines)
            {
                if (line == "-")
                {
                    Console.WriteLine($"┝{string.Concat(Enumerable.Repeat("━", maxLen + 2))}┥");
                    continue;
                }

                Console.Write($"│ {line}");
                Console.SetCursorPosition(maxLen + 3, Console.CursorTop);
                Console.WriteLine("│");
            }

            Console.WriteLine($"╰{string.Concat(Enumerable.Repeat("─", maxLen + 2))}╯");
            _content.Clear();
        }
    }
}
