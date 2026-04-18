using System.Threading.Tasks;
using Statiq.App;
using Statiq.Plugins;
using Statiq.Web;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .AddConfigurator(new ReadingTimeConfigurator())
  .AddConfigurator(new ReadingProgressBarConfigurator())
  .RunAsync();