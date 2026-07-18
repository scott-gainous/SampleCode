<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OrderDetail.aspx.cs" Inherits="ContosoOrders.Web.OrderDetailPage" Title="Order Detail" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Order <asp:Literal ID="litOrderId" runat="server" /></h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="error" Visible="false" />
    <table class="form">
        <tr><td>Customer</td><td><asp:Literal ID="litCustomer" runat="server" /></td></tr>
        <tr><td>Order date</td><td><asp:Literal ID="litOrderDate" runat="server" /></td></tr>
        <tr><td>Ship to</td><td><asp:Literal ID="litShipTo" runat="server" /></td></tr>
        <tr><td>Status</td><td><asp:Literal ID="litStatus" runat="server" /></td></tr>
    </table>

    <asp:Panel ID="pnlStatusUpdate" runat="server">
        <p>
            Change status:
            <asp:DropDownList ID="ddlNewStatus" runat="server">
                <asp:ListItem Value="Shipped" Text="Shipped" />
                <asp:ListItem Value="Cancelled" Text="Cancelled" />
            </asp:DropDownList>
            <asp:Button ID="btnUpdateStatus" runat="server" Text="Update" OnClick="btnUpdateStatus_Click" />
        </p>
    </asp:Panel>

    <h3>Items</h3>
    <asp:GridView ID="gvItems" runat="server" AutoGenerateColumns="false" CssClass="grid">
        <Columns>
            <asp:BoundField DataField="Sku" HeaderText="SKU" />
            <asp:BoundField DataField="Name" HeaderText="Product" />
            <asp:BoundField DataField="Quantity" HeaderText="Qty" />
            <asp:BoundField DataField="UnitPrice" HeaderText="Unit price" DataFormatString="{0:C}" />
            <asp:BoundField DataField="LineTotal" HeaderText="Line total" DataFormatString="{0:C}" />
        </Columns>
    </asp:GridView>
    <p class="total">Order total: <asp:Literal ID="litTotal" runat="server" /></p>
</asp:Content>
