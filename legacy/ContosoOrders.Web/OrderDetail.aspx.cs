using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ContosoOrders.Web.Code.Dal;
using ContosoOrders.Web.Code.Models;

namespace ContosoOrders.Web
{
    public partial class OrderDetailPage : Page
    {
        protected Literal litOrderId;
        protected Literal litCustomer;
        protected Literal litOrderDate;
        protected Literal litShipTo;
        protected Literal litStatus;
        protected Literal litTotal;
        protected Label lblMessage;
        protected Panel pnlStatusUpdate;
        protected DropDownList ddlNewStatus;
        protected Button btnUpdateStatus;
        protected GridView gvItems;

        private int OrderId
        {
            get
            {
                int id;
                int.TryParse(Request.QueryString["id"], out id);
                return id;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindOrder();
            }
        }

        protected void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            // Only Admins may update status — a rule enforced ONLY here in the UI layer.
            // (Recorded in migration/inventory.md; the modern API enforces it server-side.)
            if (!User.IsInRole("Admin") && User.Identity.Name != "admin")
            {
                lblMessage.Text = "You do not have permission to update order status.";
                lblMessage.Visible = true;
                return;
            }

            bool updated = OrderDal.UpdateStatus(OrderId, ddlNewStatus.SelectedValue);
            if (!updated)
            {
                lblMessage.Text = "Status was not updated (order may no longer be Pending).";
                lblMessage.Visible = true;
            }
            BindOrder();
        }

        private void BindOrder()
        {
            List<OrderItem> items;
            Order order = OrderDal.GetOrder(OrderId, out items);
            if (order == null)
            {
                Response.Redirect("~/Orders.aspx");
                return;
            }

            litOrderId.Text = "#" + order.OrderId;
            litCustomer.Text = Server.HtmlEncode(order.CompanyName);
            litOrderDate.Text = order.OrderDate.ToString("yyyy-MM-dd HH:mm");
            litShipTo.Text = Server.HtmlEncode((order.ShipCity ?? "") + ", " + (order.ShipCountry ?? ""));
            litStatus.Text = order.Status;
            litTotal.Text = order.OrderTotal.ToString("C");

            pnlStatusUpdate.Visible = order.Status == "Pending";

            gvItems.DataSource = items;
            gvItems.DataBind();
        }
    }
}
