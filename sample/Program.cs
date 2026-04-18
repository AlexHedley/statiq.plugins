using System.Threading.Tasks;
using Statiq.App;
using Statiq.Web;
using Statiq.Plugins;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .AddConfigurator(new ReadingTimeConfigurator())
  .AddConfigurator(new RelatedArticlesConfigurator())
  .RunAsync();