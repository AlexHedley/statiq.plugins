namespace Statiq.Plugins;

public class RelatedArticlesConfigurator : IConfigurator<Bootstrapper>
{
    public void Configure(Bootstrapper configurable)
    {
        configurable.ModifyPipeline("Content", p =>
        {
            p.ProcessModules.Add(new RelatedArticlesModule());
        });
    }
}
