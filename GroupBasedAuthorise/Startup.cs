using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GroupBasedAuthorise.Startup))]
namespace GroupBasedAuthorise
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
