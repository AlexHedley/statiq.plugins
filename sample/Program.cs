using Statiq.App;
using Statiq.Plugins;
using Statiq.Web;
using Statiq.Plugins;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .AddConfigurator(new ReadingTimeConfigurator())
  .AddConfigurator(new SocialImageConfigurator())
  .AddConfigurator<Bootstrapper>(new ImageGalleryConfigurator())
  .RunAsync();
