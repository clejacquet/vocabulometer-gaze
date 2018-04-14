using Microsoft.Owin.Cors;
using Owin;
using System;

namespace VocabulometerProvider
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}
