namespace Statiq.Plugins;

public class RelatedArticlesModule : Module
{
    public const string RelatedArticlesKey = "RelatedArticles";

    private readonly int _maxArticles;

    public RelatedArticlesModule(int maxArticles = 5)
    {
        _maxArticles = maxArticles;
    }

    protected override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
    {
        var inputs = context.Inputs.ToList();
        var posts = inputs.Where(doc => doc.GetBool("IsPost")).ToList();

        var results = new List<IDocument>();
        foreach (var doc in inputs)
        {
            if (doc.GetBool("IsPost") && doc.ContainsKey("Tags"))
            {
                var currentTags = doc.GetList<string>("Tags") ?? new List<string>();
                var related = posts
                    .Where(other => other.Source != doc.Source)
                    .Select(other => new
                    {
                        Document = other,
                        SharedTags = (other.GetList<string>("Tags") ?? new List<string>())
                            .Intersect(currentTags, StringComparer.OrdinalIgnoreCase)
                            .Count()
                    })
                    .Where(x => x.SharedTags > 0)
                    .OrderByDescending(x => x.SharedTags)
                    .Take(_maxArticles)
                    .Select(x => x.Document)
                    .ToList();

                results.Add(doc.Clone(new MetadataItems
                {
                    { RelatedArticlesKey, related }
                }));
            }
            else
            {
                results.Add(doc);
            }
        }

        return Task.FromResult<IEnumerable<IDocument>>(results);
    }
}
