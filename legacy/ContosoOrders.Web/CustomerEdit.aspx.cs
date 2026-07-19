using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using ContosoOrders.Web.Code.Dal;
using ContosoOrders.Web.Code.Models;

namespace ContosoOrders.Web
{
    public partial class CustomerEditPage : Page
    {
        protected Literal litTitle;
        protected Label lblError;
        protected TextBox txtCompanyName;
        protected TextBox txtContactName;
        protected TextBox txtEmail;
        protected TextBox txtPhone;
        protected TextBox txtCity;
        protected TextBox txtCountry;
        protected CheckBox chkIsActive;
        protected Button btnSave;
        protected HyperLink lnkCancel;
        protected RequiredFieldValidator rfvCompany;
        protected RequiredFieldValidator rfvContact;
        protected RegularExpressionValidator revEmail;

        private int CustomerId
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
                if (CustomerId > 0)
                {
                    litTitle.Text = "Edit Customer";
                    Customer customer = CustomerDal.GetCustomer(CustomerId);
                    if (customer == null)
                    {
                        Response.Redirect("~/Customers.aspx");
                        return;
                    }
                    txtCompanyName.Text = customer.CompanyName;
                    txtContactName.Text = customer.ContactName;
                    txtEmail.Text = customer.Email;
                    txtPhone.Text = customer.Phone;
                    txtCity.Text = customer.City;
                    txtCountry.Text = customer.Country;
                    chkIsActive.Checked = customer.IsActive;
                }
                else
                {
                    litTitle.Text = "New Customer";
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            Customer customer = new Customer();
            customer.CustomerId = CustomerId;
            customer.CompanyName = txtCompanyName.Text.Trim();
            customer.ContactName = txtContactName.Text.Trim();
            customer.Email = txtEmail.Text.Trim();
            customer.Phone = NullIfEmpty(txtPhone.Text);
            customer.City = NullIfEmpty(txtCity.Text);
            customer.Country = NullIfEmpty(txtCountry.Text);
            customer.IsActive = chkIsActive.Checked;

            try
            {
                CustomerDal.SaveCustomer(customer);
                Response.Redirect("~/Customers.aspx");
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                // 2601/2627 = unique index violation (duplicate email).
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    lblError.Text = "A customer with that email already exists.";
                    lblError.Visible = true;
                }
                else
                {
                    throw;
                }
            }
        }

        private static string NullIfEmpty(string value)
        {
            string trimmed = value == null ? null : value.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }
    }
}
