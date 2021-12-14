using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Plants.Startup))]
namespace Plants
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
