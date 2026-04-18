namespace Statiq.Plugins;

// https://blog.jermdavis.dev/posts/2023/adding-reading-progress-indicator
public class ReadingProgressBarConfigurator : IConfigurator<Bootstrapper>
{
    public void Configure(Bootstrapper configurable)
    {
        configurable.ModifyPipeline("Content", p =>
        {
            p.ProcessModules.Insert(3, new ReadingProgressBarModule());
        });
    }
}
