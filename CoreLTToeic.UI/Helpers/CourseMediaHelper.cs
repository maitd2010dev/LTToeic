namespace CoreLTToeic.UI.Helpers;

public static class CourseMediaHelper
{
    public static string? ToEmbedUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return null;

        var host = uri.Host.ToLowerInvariant();
        if (host is "youtu.be" or "www.youtu.be")
        {
            var videoId = uri.AbsolutePath.Trim('/');
            return IsYouTubeVideoId(videoId) ? $"https://www.youtube.com/embed/{videoId}" : null;
        }

        if (host is "youtube.com" or "www.youtube.com" or "m.youtube.com")
        {
            var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string? videoId = null;

            if (pathSegments.Length >= 2 && pathSegments[0] is "embed" or "shorts")
                videoId = pathSegments[1];
            else if (uri.AbsolutePath.Equals("/watch", StringComparison.OrdinalIgnoreCase))
                videoId = GetQueryValue(uri.Query, "v");

            return IsYouTubeVideoId(videoId) ? $"https://www.youtube.com/embed/{videoId}" : null;
        }

        return uri.ToString();
    }

    private static string? GetQueryValue(string query, string key)
    {
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 &&
                Uri.UnescapeDataString(parts[0]).Equals(key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(parts[1]);
        }

        return null;
    }

    private static bool IsYouTubeVideoId(string? value)
        => value?.Length == 11 && value.All(character =>
            char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
}
