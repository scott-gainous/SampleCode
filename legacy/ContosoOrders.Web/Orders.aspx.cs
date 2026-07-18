using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using ContosoOrders.Web.Code.Dal;

namespace ContosoOrders.Web
{
    public partial class OrdersPage : Page
    {
        protected DropDownList ddlStatus;
        protected GridView gvOrders;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Optional ?customerId= filter, used by links from customer screens.
                BindGrid();
            }
        }

        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void gvOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewOrder")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int orderId = (int)gvOrders.DataKeys[rowIndex].Value;
                Response.Redirect("~/OrderDetail.aspx?id=" + orderId);
            }
        }

        private void BindGrid()
        {
            int? customerId = null;
            int parsed;
            if (int.TryParse(Request.QueryString["customerId"], out parsed))
            {
                customerId = parsed;
            }
            gvOrders.DataSource = OrderDal.GetOrders(customerId, ddlStatus.SelectedValue);
            gvOrders.DataBind();
        }
    }
}
