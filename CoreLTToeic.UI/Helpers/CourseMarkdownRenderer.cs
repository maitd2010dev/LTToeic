using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace CoreLTToeic.UI.Helpers
{
    public static class CourseMarkdownRenderer
    {
        public static MarkupString Render(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return new MarkupString(string.Empty);
            var lines = markdown.Replace("\r\n", "\n").Split('\n');
            var output = new StringBuilder();
            var index = 0;
            while (index < lines.Length)
            {
                var line = lines[index];
                if (string.IsNullOrWhiteSpace(line)) { index++; continue; }

                if (line.StartsWith("|") && index + 1 < lines.Length && IsTableSeparator(lines[index + 1]))
                {
                    var headers = Cells(line);
                    output.Append("<div class=\"table-responsive\"><table class=\"table table-bordered\"><thead><tr>");
                    foreach (var cell in headers) output.Append("<th>").Append(Inline(cell)).Append("</th>");
                    output.Append("</tr></thead><tbody>");
                    index += 2;
                    while (index < lines.Length && lines[index].StartsWith("|"))
                    {
                        output.Append("<tr>");
                        foreach (var cell in Cells(lines[index])) output.Append("<td>").Append(Inline(cell)).Append("</td>");
                        output.Append("</tr>");
                        index++;
                    }
                    output.Append("</tbody></table></div>");
                    continue;
                }

                var heading = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (heading.Success)
                {
                    var level = heading.Groups[1].Value.Length;
                    output.Append($"<h{level}>").Append(Inline(heading.Groups[2].Value)).Append($"</h{level}>");
                    index++;
                    continue;
                }

                if (Regex.IsMatch(line, @"^\s*[-*+]\s+"))
                {
                    output.Append("<ul>");
                    while (index < lines.Length && Regex.IsMatch(lines[index], @"^\s*[-*+]\s+"))
                    {
                        output.Append("<li>").Append(Inline(Regex.Replace(lines[index], @"^\s*[-*+]\s+", ""))).Append("</li>");
                        index++;
                    }
                    output.Append("</ul>");
                    continue;
                }

                if (Regex.IsMatch(line, @"^\s*\d+\.\s+"))
                {
                    output.Append("<ol>");
                    while (index < lines.Length && Regex.IsMatch(lines[index], @"^\s*\d+\.\s+"))
                    {
                        output.Append("<li>").Append(Inline(Regex.Replace(lines[index], @"^\s*\d+\.\s+", ""))).Append("</li>");
                        index++;
                    }
                    output.Append("</ol>");
                    continue;
                }

                output.Append("<p>").Append(Inline(line)).Append("</p>");
                index++;
            }
            return new MarkupString(output.ToString());
        }

        private static string Inline(string value)
        {
            var encoded = WebUtility.HtmlEncode(value);
            encoded = Regex.Replace(encoded, @"\[([^\]]+)\]\(([^\)]+)\)", match =>
            {
                var url = WebUtility.HtmlDecode(match.Groups[2].Value);
                return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                       (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    ? $"<a href=\"{WebUtility.HtmlEncode(uri.ToString())}\" target=\"_blank\" rel=\"noopener noreferrer\">{match.Groups[1].Value}</a>"
                    : match.Groups[1].Value;
            });
            encoded = Regex.Replace(encoded, @"\*\*([^*]+)\*\*", "<strong>$1</strong>");
            encoded = Regex.Replace(encoded, @"(?<!\*)\*([^*]+)\*(?!\*)", "<em>$1</em>");
            encoded = Regex.Replace(encoded, @"__([^_]+)__", "<strong>$1</strong>");
            encoded = Regex.Replace(encoded, @"(?<!_)_([^_]+)_(?!_)", "<em>$1</em>");
            return encoded;
        }

        private static bool IsTableSeparator(string line)
            => Cells(line).Count > 0 && Cells(line).All(cell => Regex.IsMatch(cell, @"^:?-{3,}:?$"));

        private static List<string> Cells(string line)
            => line.Trim().Trim('|').Split('|').Select(cell => cell.Trim()).ToList();
    }
}
