<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CustomerEdit.aspx.cs" Inherits="ContosoOrders.Web.CustomerEditPage" Title="Edit Customer" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><asp:Literal ID="litTitle" runat="server" /></h2>
    <asp:Label ID="lblError" runat="server" CssClass="error" Visible="false" />
    <table class="form">
        <tr>
            <td>Company name</td>
            <td>
                <asp:TextBox ID="txtCompanyName" runat="server" MaxLength="100" />
                <asp:RequiredFieldValidator ID="rfvCompany" runat="server" ControlToValidate="txtCompanyName"
                    ErrorMessage="Required" Display="Dynamic" />
            </td>
        </tr>
        <tr>
            <td>Contact name</td>
            <td>
                <asp:TextBox ID="txtContactName" runat="server" MaxLength="100" />
                <asp:RequiredFieldValidator ID="rfvContact" runat="server" ControlToValidate="txtContactName"
                    ErrorMessage="Required" Display="Dynamic" />
            </td>
        </tr>
        <tr>
            <td>Email</td>
            <td>
                <asp:TextBox ID="txtEmail" runat="server" MaxLength="256" />
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                    ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                    ErrorMessage="Invalid email" Display="Dynamic" />
            </td>
        </tr>
        <tr><td>Phone</td><td><asp:TextBox ID="txtPhone" runat="server" MaxLength="30" /></td></tr>
        <tr><td>City</td><td><asp:TextBox ID="txtCity" runat="server" MaxLength="60" /></td></tr>
        <tr><td>Country</td><td><asp:TextBox ID="txtCountry" runat="server" MaxLength="60" /></td></tr>
        <tr><td>Active</td><td><asp:CheckBox ID="chkIsActive" runat="server" Checked="true" /></td></tr>
    </table>
    <p>
        <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
        <asp:HyperLink ID="lnkCancel" runat="server" NavigateUrl="~/Customers.aspx" Text="Cancel" />
    </p>
</asp:Content>
