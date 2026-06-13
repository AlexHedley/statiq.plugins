namespace Statiq.Plugins;

// https://wellsb.com/csharp/aspnet/generate-images-statiq-imagesharp
/// <summary>
/// Configures the Statiq <c>Bootstrapper</c> to automatically generate social share images
/// for blog posts by injecting <see cref="SocialImageModule"/> into the Content pipeline's
/// Process phase.
/// </summary>
public class SocialImageConfigurator : IConfigurator<Bootstrapper>
{
    /// <inheritdoc />
    public void Configure(Bootstrapper configurable)
    {
        configurable.ModifyPipeline("Content", p =>
        {
            p.ProcessModules.Add(new SocialImageModule());
        });
    }
}
