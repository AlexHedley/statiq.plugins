namespace Statiq.Plugins;

// https://github.com/madmurphy/cookies.js/
public class CookiesNoticeConfigurator : IConfigurator<Bootstrapper>
{
    public void Configure(Bootstrapper configurable)
    {
        configurable.ModifyPipeline("Content", p =>
        {
            p.PostProcessModules.Add(new CookiesNoticeModule());
        });
    }
}
