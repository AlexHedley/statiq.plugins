using System.Threading.Tasks;
using Statiq.App;
using Statiq.Plugins;
using Statiq.Web;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .AddConfigurator<Bootstrapper, ReadingTimeConfigurator>()
  .AddConfigurator<Bootstrapper, HeadingSummaryConfigurator>()
  .RunAsync();