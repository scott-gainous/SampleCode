using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using ContosoOrders.Web.Code.Dal;
using ContosoOrders.Web.Code.Security;

namespace ContosoOrders.Web
{
    public partial class LoginPage : Page
    {
        protected Label lblError;
        protected Label lblUsername;
        protected Label lblPassword;
        protected TextBox txtUsername;
        protected TextBox txtPassword;
        protected Button btnLogin;

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string hash = AuthHelper.HashPassword(txtPassword.Text);
            string role = UserDal.ValidateUser(username, hash);

            if (role == null)
            {
                lblError.Text = "Invalid username or password.";
                lblError.Visible = true;
                return;
            }

            FormsAuthentication.SetAuthCookie(username, false);
            Response.Redirect(FormsAuthentication.GetRedirectUrl(username, false));
        }
    }
}
