using Statiq.App;

namespace Statiq.Plugins;

// https://lokeshdhakar.com/projects/lightbox2/
public class ImageGalleryConfigurator : IConfigurator<Bootstrapper>
{
    public void Configure(Bootstrapper configurable)
    {
        configurable.AddShortcode<ImageGalleryShortcode>("ImageGallery");
    }
}
