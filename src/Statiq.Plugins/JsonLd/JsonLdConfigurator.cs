namespace Statiq.Plugins;

// https://json-ld.org/
// https://jsonld.com/
public class JsonLdConfigurator : IConfigurator<Bootstrapper>
{
    public void Configure(Bootstrapper configurable)
    {
        configurable.ModifyPipeline("Content", p =>
        {
            p.ProcessModules.Add(new JsonLdModule());
        });
    }
}
