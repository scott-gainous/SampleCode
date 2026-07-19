using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using ContosoOrders.Web.Code.Dal;

namespace ContosoOrders.Web
{
    public partial class ProductsPage : Page
    {
        protected CheckBox chkIncludeDiscontinued;
        protected GridView gvProducts;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        protected void chkIncludeDiscontinued_CheckedChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        private void BindGrid()
        {
            gvProducts.DataSource = ProductDal.GetProducts(chkIncludeDiscontinued.Checked);
            gvProducts.DataBind();
        }
    }
}
