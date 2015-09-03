using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CapstoneTaxiVisualization.Startup))]
namespace CapstoneTaxiVisualization
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
