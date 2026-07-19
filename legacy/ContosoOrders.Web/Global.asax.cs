using System;
using System.Web;

namespace ContosoOrders.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Legacy behavior: swallow-and-log. Errors are written to the Windows event
            // log in production; here we just clear so customErrors redirects.
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                System.Diagnostics.Trace.WriteLine("Unhandled: " + ex);
            }
        }
    }
}
