namespace Statiq.Plugins;

// https://blog.jermdavis.dev/posts/2024/extracting-article-headings
public class Heading
{
    /// <summary>
    /// The heading level (1–6).
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// The id attribute of the heading element, used as an anchor for navigation.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The visible text of the heading.
    /// </summary>
    public string? Text { get; set; }
}
