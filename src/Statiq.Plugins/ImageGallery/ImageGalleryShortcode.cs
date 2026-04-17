using Statiq.Common;

namespace Statiq.Plugins;

// https://lokeshdhakar.com/projects/lightbox2/
public class ImageGalleryShortcode : SyncShortcode
{
    private const string Name = nameof(Name);
    private const string Class = nameof(Class);

    // Lightbox2 global options
    // https://lokeshdhakar.com/projects/lightbox2/#options
    private const string AlbumLabel = nameof(AlbumLabel);
    private const string WrapAround = nameof(WrapAround);
    private const string DisableScrolling = nameof(DisableScrolling);
    private const string FadeDuration = nameof(FadeDuration);
    private const string ImageFadeDuration = nameof(ImageFadeDuration);
    private const string ResizeDuration = nameof(ResizeDuration);
    private const string FitImagesInViewport = nameof(FitImagesInViewport);
    private const string ShowImageNumberLabel = nameof(ShowImageNumberLabel);
    private const string MaxWidth = nameof(MaxWidth);
    private const string MaxHeight = nameof(MaxHeight);

    public override ShortcodeResult Execute(
        KeyValuePair<string, string>[] args,
        string content,
        IDocument document,
        IExecutionContext context)
    {
        var arguments = args.ToDictionary(
            Name, Class,
            AlbumLabel, WrapAround, DisableScrolling,
            FadeDuration, ImageFadeDuration, ResizeDuration,
            FitImagesInViewport, ShowImageNumberLabel,
            MaxWidth, MaxHeight);

        var galleryName = arguments.ContainsKey(Name)
            ? arguments.GetString(Name)
            : "gallery";

        var additionalClass = arguments.ContainsKey(Class)
            ? " " + arguments.GetString(Class)
            : string.Empty;

        var images = ParseImages(content);

        if (images.Count == 0)
        {
            return string.Empty;
        }

        var html = new System.Text.StringBuilder();
        html.AppendLine($"<div class=\"image-gallery{System.Net.WebUtility.HtmlEncode(additionalClass)}\">");

        foreach (var (src, title, alt) in images)
        {
            var encodedSrc = System.Net.WebUtility.HtmlEncode(src);
            var encodedGalleryName = System.Net.WebUtility.HtmlEncode(galleryName);
            var encodedTitle = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(title) ? string.Empty : title);
            var encodedAlt = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(alt) ? title : alt);

            var anchor = $"  <a href=\"{encodedSrc}\" data-lightbox=\"{encodedGalleryName}\"";
            if (!string.IsNullOrEmpty(encodedTitle))
            {
                anchor += $" data-title=\"{encodedTitle}\"";
            }
            if (!string.IsNullOrEmpty(encodedAlt))
            {
                anchor += $" data-alt=\"{encodedAlt}\"";
            }
            anchor += ">";

            html.AppendLine(anchor);
            html.AppendLine($"    <img src=\"{encodedSrc}\" alt=\"{encodedAlt}\" />");
            html.AppendLine($"  </a>");
        }

        html.AppendLine("</div>");

        AppendLightboxOptions(html, arguments, galleryName);

        return html.ToString();
    }

    private static void AppendLightboxOptions(
        System.Text.StringBuilder html,
        IMetadata arguments,
        string galleryName)
    {
        var options = new System.Collections.Generic.Dictionary<string, string>();

        if (arguments.ContainsKey(AlbumLabel))
        {
            options["albumLabel"] = $"\"{EscapeJsString(arguments.GetString(AlbumLabel))}\"";
        }
        if (arguments.ContainsKey(WrapAround))
        {
            options["wrapAround"] = arguments.GetBool(WrapAround) ? "true" : "false";
        }
        if (arguments.ContainsKey(DisableScrolling))
        {
            options["disableScrolling"] = arguments.GetBool(DisableScrolling) ? "true" : "false";
        }
        if (arguments.ContainsKey(FadeDuration))
        {
            options["fadeDuration"] = arguments.GetInt(FadeDuration).ToString();
        }
        if (arguments.ContainsKey(ImageFadeDuration))
        {
            options["imageFadeDuration"] = arguments.GetInt(ImageFadeDuration).ToString();
        }
        if (arguments.ContainsKey(ResizeDuration))
        {
            options["resizeDuration"] = arguments.GetInt(ResizeDuration).ToString();
        }
        if (arguments.ContainsKey(FitImagesInViewport))
        {
            options["fitImagesInViewport"] = arguments.GetBool(FitImagesInViewport) ? "true" : "false";
        }
        if (arguments.ContainsKey(ShowImageNumberLabel))
        {
            options["showImageNumberLabel"] = arguments.GetBool(ShowImageNumberLabel) ? "true" : "false";
        }
        if (arguments.ContainsKey(MaxWidth))
        {
            options["maxWidth"] = arguments.GetInt(MaxWidth).ToString();
        }
        if (arguments.ContainsKey(MaxHeight))
        {
            options["maxHeight"] = arguments.GetInt(MaxHeight).ToString();
        }

        if (options.Count == 0)
        {
            return;
        }

        html.AppendLine("<script>");
        html.AppendLine("lightbox.option({");
        var entries = new System.Collections.Generic.List<string>();
        foreach (var kvp in options)
        {
            entries.Add($"  '{kvp.Key}': {kvp.Value}");
        }
        html.AppendLine(string.Join(",\n", entries));
        html.AppendLine("});");
        html.AppendLine("</script>");
    }

    private static string EscapeJsString(string? value)
    {
        if (value is null)
        {
            return string.Empty;
        }
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static readonly System.Text.RegularExpressions.Regex HtmlTagRegex =
        new("<[^>]+>", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static List<(string Src, string Title, string Alt)> ParseImages(string content)
    {
        var images = new List<(string Src, string Title, string Alt)>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return images;
        }

        foreach (var line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            // Strip any HTML tags that may have been injected by Markdown pre-processing
            var trimmed = HtmlTagRegex.Replace(line, string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            var parts = trimmed.Split('|');
            var src = parts[0].Trim();
            var title = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            var alt = parts.Length > 2 ? parts[2].Trim() : string.Empty;

            images.Add((src, title, alt));
        }

        return images;
    }
}
