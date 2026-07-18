using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using ContosoOrders.Web.Code.Dal;

namespace ContosoOrders.Web
{
    public partial class DefaultPage : Page
    {
        protected Literal litCustomerCount;
        protected Literal litProductCount;
        protected Literal litOpenOrders;
        protected Literal litRevenue;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DashboardSummary summary = DashboardDal.GetSummary();
                litCustomerCount.Text = summary.CustomerCount.ToString();
                litProductCount.Text = summary.ProductCount.ToString();
                litOpenOrders.Text = summary.OpenOrderCount.ToString();
                // Legacy formatting quirk: server-culture currency string, e.g. "$1,234.50".
                litRevenue.Text = summary.RevenueToDate.ToString("C");
            }
        }
    }
}
