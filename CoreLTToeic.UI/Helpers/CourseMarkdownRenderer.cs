using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace CoreLTToeic.UI.Helpers
{
    public static class CourseMarkdownRenderer
    {
        private static readonly Regex HtmlTagPattern = new(
            @"<\s*/?\s*(?:p|br|strong|b|em|i|u|s|ol|ul|li|blockquote|h[1-6]|a|span|figure|figcaption|img)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HashSet<string> AllowedHtmlTags =
        [
            "p", "br", "strong", "b", "em", "i", "u", "s",
            "ol", "ul", "li", "blockquote", "h1", "h2", "h3",
            "h4", "h5", "h6", "a", "span", "figure", "figcaption", "img"
        ];

        public static MarkupString Render(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return new MarkupString(string.Empty);
            if (IsHtml(markdown)) return new MarkupString(SanitizeHtml(markdown));

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

                if (Regex.IsMatch(line, @"^\s*>\s*"))
                {
                    output.Append("<blockquote>");
                    while (index < lines.Length && Regex.IsMatch(lines[index], @"^\s*>\s*"))
                    {
                        output.Append("<p>")
                            .Append(Inline(Regex.Replace(lines[index], @"^\s*>\s*", "")))
                            .Append("</p>");
                        index++;
                    }
                    output.Append("</blockquote>");
                    continue;
                }

                if (Regex.IsMatch(line, @"^\s*(?:-{3,}|\*{3,}|_{3,})\s*$"))
                {
                    output.Append("<hr>");
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

        public static bool IsHtml(string? value)
            => !string.IsNullOrWhiteSpace(value) && HtmlTagPattern.IsMatch(value);

        public static string ToPlainText(string? value, int maxLength = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var html = IsHtml(value) ? SanitizeHtml(value) : Render(value).Value;
            html = Regex.Replace(
                html,
                @"</(?:p|li|h[1-6]|blockquote)>|<br\s*/?>",
                " ",
                RegexOptions.IgnoreCase);
            var plainText = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", " "));
            plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

            if (maxLength <= 0 || plainText.Length <= maxLength) return plainText;
            var cutoff = plainText.LastIndexOf(' ', maxLength);
            if (cutoff < maxLength / 2) cutoff = maxLength;
            return $"{plainText[..cutoff].TrimEnd()}\u2026";
        }

        private static string SanitizeHtml(string html)
        {
            html = Regex.Replace(
                html,
                @"<\s*(script|style|iframe|object|embed)\b[^>]*>.*?<\s*/\s*\1\s*>",
                string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return Regex.Replace(html, @"<\s*(/?)\s*([a-zA-Z0-9]+)([^>]*)>", match =>
            {
                var closing = match.Groups[1].Value == "/";
                var tag = match.Groups[2].Value.ToLowerInvariant();
                if (!AllowedHtmlTags.Contains(tag)) return string.Empty;
                if (closing) return tag is "br" or "img" ? string.Empty : $"</{tag}>";
                if (tag == "br") return "<br>";
                if (tag == "img") return SanitizeImage(match.Groups[3].Value);
                if (tag != "a") return $"<{tag}>";

                var hrefMatch = Regex.Match(
                    match.Groups[3].Value,
                    @"\bhref\s*=\s*(?:""([^""]*)""|'([^']*)')",
                    RegexOptions.IgnoreCase);
                var href = WebUtility.HtmlDecode(
                    hrefMatch.Groups[1].Success ? hrefMatch.Groups[1].Value : hrefMatch.Groups[2].Value);
                return Uri.TryCreate(href, UriKind.Absolute, out var uri) &&
                       (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    ? $"<a href=\"{WebUtility.HtmlEncode(uri.ToString())}\" target=\"_blank\" rel=\"noopener noreferrer\">"
                    : "<a>";
            });
        }

        private static string SanitizeImage(string attributes)
        {
            var sourceMatch = Regex.Match(
                attributes,
                @"\bsrc\s*=\s*(?:""([^""]*)""|'([^']*)')",
                RegexOptions.IgnoreCase);
            var source = WebUtility.HtmlDecode(
                sourceMatch.Groups[1].Success ? sourceMatch.Groups[1].Value : sourceMatch.Groups[2].Value);

            var isLocal = source.StartsWith("/", StringComparison.Ordinal) &&
                          !source.StartsWith("//", StringComparison.Ordinal);
            var isRemote = Uri.TryCreate(source, UriKind.Absolute, out var uri) &&
                           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!isLocal && !isRemote) return string.Empty;

            var altMatch = Regex.Match(
                attributes,
                @"\balt\s*=\s*(?:""([^""]*)""|'([^']*)')",
                RegexOptions.IgnoreCase);
            var alt = altMatch.Groups[1].Success ? altMatch.Groups[1].Value : altMatch.Groups[2].Value;
            return $"<img src=\"{WebUtility.HtmlEncode(source)}\" alt=\"{WebUtility.HtmlEncode(WebUtility.HtmlDecode(alt))}\">";
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
