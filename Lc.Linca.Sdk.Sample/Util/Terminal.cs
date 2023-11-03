/***********************************************************************************
 * Project:   Linked Care AP5
 * Component: LINCA FHIR SDK and Demo Client
 * Copyright: 2023 LOIDL Consulting & IT Services GmbH
 * Authors:   Annemarie Goldmann, Daniel Latikaynen
 * Purpose:   Sample code to test LINCA and template for client prototypes
 * Licence:   BSD 3-Clause
 * ---------------------------------------------------------------------------------
 * The Linked Care project is co-funded by the Austrian FFG
 ***********************************************************************************/

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
        private readonly object lockObject = new();
        private readonly Regex ansiRemover = new(AnsiRemover);
        private readonly StringBuilder _content = new();

        public Info(string? initialContent = null)
        {
            if (initialContent != null)
            {
                WriteLine(Outdent(initialContent));
            }
        }

        public int WriteLine(string line, bool flush = false)
        {
            var yPos = PeekY();

            _content.AppendLine(line);
            if(flush)
            {
                Flush();
            }

            return yPos;
        }

        public void HorizontalRule()
        {
            _content.AppendLine("-");
        }

        public void Flush(string terminator = "")
        {
            Console.Write(_content.ToString());
            if (!string.IsNullOrEmpty(terminator))
            {
                Console.Write(terminator);
            }

            _content.Clear();
        }

        public bool Outcome(bool status, int y)
        {
            lock (lockObject)
            {
                var restoreX = Console.CursorLeft;
                var restoreY = Console.CursorTop;

                Console.CursorLeft = 3;
                Console.CursorTop = y;
                Console.Write(status ? Constants.AnsiSuccess : Constants.AnsiFail);
                Console.SetCursorPosition(restoreX, restoreY);
            }

            return status;
        }

        public static int PeekY() => Console.CursorTop;

        public void Show(bool clear = false)
        {
            var lines = _content.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var maxLen = lines.Max(line => ansiRemover.Replace(line, string.Empty).Length);

            if (maxLen > Console.BufferWidth - 4)
            {
                maxLen = Console.BufferWidth - 4;
            }

            Console.OutputEncoding = Encoding.UTF8;
            if(clear)
            {
                Console.Clear();
            }

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

        private static string Outdent(string input)
        {
            return string.Join(Environment.NewLine, input.Split(Environment.NewLine).Select(l => l.TrimStart()));
        }
    }
}
