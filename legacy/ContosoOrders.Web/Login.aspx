<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ContosoOrders.Web.LoginPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Contoso Orders - Log in</title>
    <link href="Styles/site.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-box">
            <h2>Log in</h2>
            <asp:Label ID="lblError" runat="server" CssClass="error" Visible="false" />
            <p>
                <asp:Label ID="lblUsername" runat="server" AssociatedControlID="txtUsername" Text="Username" /><br />
                <asp:TextBox ID="txtUsername" runat="server" />
            </p>
            <p>
                <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" Text="Password" /><br />
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
            </p>
            <asp:Button ID="btnLogin" runat="server" Text="Log in" OnClick="btnLogin_Click" />
        </div>
    </form>
</body>
</html>
