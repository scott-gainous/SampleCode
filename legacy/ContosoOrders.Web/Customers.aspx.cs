using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using ContosoOrders.Web.Code.Dal;

namespace ContosoOrders.Web
{
    public partial class CustomersPage : Page
    {
        protected CheckBox chkIncludeInactive;
        protected HyperLink lnkNew;
        protected GridView gvCustomers;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        protected void chkIncludeInactive_CheckedChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void gvCustomers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditCustomer")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int customerId = (int)gvCustomers.DataKeys[rowIndex].Value;
                Response.Redirect("~/CustomerEdit.aspx?id=" + customerId);
            }
        }

        private void BindGrid()
        {
            gvCustomers.DataSource = CustomerDal.GetCustomers(chkIncludeInactive.Checked);
            gvCustomers.DataBind();
        }
    }
}
