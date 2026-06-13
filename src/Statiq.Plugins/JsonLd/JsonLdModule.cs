using System.Text.Json;
using System.Text.Json.Nodes;

namespace Statiq.Plugins;

// https://json-ld.org/
// https://jsonld.com/
// https://schema.org/BlogPosting
public class JsonLdModule : ParallelModule
{
    protected override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
    {
        if (input.GetBool("IsPost") && !input.GetBool("IsPostArchive"))
        {
            var jsonLd = BuildJsonLd(input, context);
            return Task.FromResult(input
                .Clone(new MetadataItems {
                    { "JsonLd", jsonLd }
                }).Yield());
        }
        return Task.FromResult(input.Yield());
    }

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    private static string BuildJsonLd(IDocument input, IExecutionContext context)
    {
        var obj = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BlogPosting"
        };

        var title = input.GetString("Title");
        if (!string.IsNullOrEmpty(title))
        {
            obj["headline"] = title;
        }

        var description = input.GetString("Lead") ?? input.GetString(WebKeys.Description);
        if (!string.IsNullOrEmpty(description))
        {
            obj["description"] = description;
        }

        var published = input.GetDateTime(WebKeys.Published);
        if (published != default)
        {
            obj["datePublished"] = published.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        var image = input.GetString(WebKeys.Image);
        if (!string.IsNullOrEmpty(image))
        {
            obj["image"] = context.GetLink(image, true);
        }

        var url = context.GetLink(input, true);
        if (!string.IsNullOrEmpty(url))
        {
            obj["url"] = url;
        }

        var author = input.GetString("Author") ?? context.GetString("Author");
        if (!string.IsNullOrEmpty(author))
        {
            obj["author"] = new JsonObject
            {
                ["@type"] = "Person",
                ["name"] = author
            };
        }

        var siteTitle = context.GetString("SiteTitle");
        if (!string.IsNullOrEmpty(siteTitle))
        {
            obj["publisher"] = new JsonObject
            {
                ["@type"] = "Organization",
                ["name"] = siteTitle
            };
        }

        var tags = input.GetList<string>("Tags");
        if (tags != null && tags.Count > 0)
        {
            obj["keywords"] = string.Join(", ", tags);
        }

        return JsonSerializer.Serialize(obj, SerializerOptions);
    }
}
