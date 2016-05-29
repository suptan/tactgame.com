using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(tactgame.com.Startup))]
namespace tactgame.com
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
