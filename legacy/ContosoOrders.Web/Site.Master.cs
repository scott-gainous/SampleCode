using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ContosoOrders.Web
{
    public partial class SiteMaster : MasterPage
    {
        protected HyperLink lnkHome;
        protected HyperLink lnkCustomers;
        protected HyperLink lnkOrders;
        protected HyperLink lnkProducts;
        protected LinkButton btnLogout;
        protected Label lblUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.User.Identity.IsAuthenticated)
            {
                lblUser.Text = "Signed in as " + Page.User.Identity.Name;
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Response.Redirect("~/Login.aspx");
        }
    }
}
