using HtmlAgilityPack;

namespace Statiq.Plugins;

// https://blog.jermdavis.dev/posts/2023/reading-time-estimates-statiq
public class ReadingTimeModule : ParallelModule
{
    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        if (input.Source.Extension == ".md")
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            var content = await input.GetContentStringAsync();
            doc.LoadHtml(content);
            RemoveUnwantedElements(doc);
            var text = doc.DocumentNode.InnerText;
            var ReadingSpeed = context.GetInt("ReadingSpeed");
            var count = WordCount(text);
            var readingTime = MathF.Ceiling(count / ReadingSpeed);
            return input
                .Clone(new MetadataItems {
                    { "ReadingTime", readingTime.ToString() }
                }).Yield();
        }
        return input.Yield();
    }

    private void RemoveUnwantedElements(HtmlDocument d)
    {
        var toRemove = d.DocumentNode.SelectNodes("//pre");
        if (toRemove != null)
        {
            foreach (var element in toRemove)
            {
                element.Remove();
            }
        }
    }

    private int WordCount(string text)
    {
        int wordCount = 0, index = 0;
        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }
        while (index < text.Length)
        {
            while (index < text.Length && !char.IsWhiteSpace(text[index]))
            {
                index++;
            }
            wordCount++;
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }
        }
        return wordCount;
    }
}