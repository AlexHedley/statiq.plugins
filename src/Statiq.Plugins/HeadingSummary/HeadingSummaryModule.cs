using HtmlAgilityPack;

namespace Statiq.Plugins;

// https://blog.jermdavis.dev/posts/2024/extracting-article-headings
public class HeadingSummaryModule : ParallelModule
{
    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        if (input.Source.Extension == ".md")
        {
            var doc = new HtmlDocument();
            var content = await input.GetContentStringAsync();
            doc.LoadHtml(content);

            var headings = ExtractHeadings(doc);

            return input
                .Clone(new MetadataItems {
                    { "Headings", headings }
                }).Yield();
        }
        return input.Yield();
    }

    private List<Heading> ExtractHeadings(HtmlDocument doc)
    {
        var headings = new List<Heading>();
        var nodes = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var level = int.Parse(node.Name.Substring(1));
                var id = node.GetAttributeValue("id", null);
                var text = node.InnerText?.Trim();
                headings.Add(new Heading
                {
                    Level = level,
                    Id = id,
                    Text = text
                });
            }
        }
        return headings;
    }
}
