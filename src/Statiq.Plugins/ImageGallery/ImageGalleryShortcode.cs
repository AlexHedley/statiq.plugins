using Statiq.Common;

namespace Statiq.Plugins;

// https://lokeshdhakar.com/projects/lightbox2/
public class ImageGalleryShortcode : SyncShortcode
{
    private const string Name = nameof(Name);
    private const string Class = nameof(Class);

    public override ShortcodeResult Execute(
        KeyValuePair<string, string>[] args,
        string content,
        IDocument document,
        IExecutionContext context)
    {
        var arguments = args.ToDictionary(Name, Class);

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
        html.AppendLine($"<div class=\"image-gallery{additionalClass}\">");

        foreach (var (src, alt) in images)
        {
            var altAttr = string.IsNullOrWhiteSpace(alt) ? string.Empty : alt;
            html.AppendLine($"  <a href=\"{src}\" data-lightbox=\"{galleryName}\" data-title=\"{altAttr}\">");
            html.AppendLine($"    <img src=\"{src}\" alt=\"{altAttr}\" />");
            html.AppendLine($"  </a>");
        }

        html.Append("</div>");

        return html.ToString();
    }

    private static List<(string Src, string Alt)> ParseImages(string content)
    {
        var images = new List<(string Src, string Alt)>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return images;
        }

        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            var pipeIndex = trimmed.IndexOf('|');
            if (pipeIndex >= 0)
            {
                var src = trimmed[..pipeIndex].Trim();
                var alt = trimmed[(pipeIndex + 1)..].Trim();
                images.Add((src, alt));
            }
            else
            {
                images.Add((trimmed, string.Empty));
            }
        }

        return images;
    }
}
