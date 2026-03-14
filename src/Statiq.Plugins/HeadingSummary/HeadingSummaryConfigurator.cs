namespace Statiq.Plugins;

// https://blog.jermdavis.dev/posts/2024/extracting-article-headings
public class HeadingSummaryConfigurator : IConfigurator<Bootstrapper>
{
    public void Configure(Bootstrapper configurable)
    {
        configurable.ModifyPipeline("Content", p =>
        {
            // Insert after standard markdown/razor processing modules (index 3)
            p.ProcessModules.Insert(3, new HeadingSummaryModule());
        });
    }
}
