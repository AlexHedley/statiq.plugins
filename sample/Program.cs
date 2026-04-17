using System.Threading.Tasks;
using Statiq.App;
using Statiq.Web;
using Statiq.Plugins;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .AddConfigurator<Bootstrapper>(new ImageGalleryConfigurator())
  .RunAsync();
