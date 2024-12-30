namespace Statiq.Plugins;

// https://blog.jermdavis.dev/posts/2023/reading-time-estimates-statiq
public class ReadingTimeConfigurator : IConfigurator<Bootstrapper>
{
    public void Configure(Bootstrapper configurable)
    {
        configurable.ModifyPipeline("Content", p =>
        {
            p.ProcessModules.Insert(3, new ReadingTimeModule());
        });
    }
}