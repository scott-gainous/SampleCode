<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="ContosoOrders.Web.OrdersPage" Title="Orders" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Orders</h2>
    <p>
        Status:
        <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
            <asp:ListItem Value="" Text="(all)" />
            <asp:ListItem Value="Pending" Text="Pending" />
            <asp:ListItem Value="Shipped" Text="Shipped" />
            <asp:ListItem Value="Cancelled" Text="Cancelled" />
        </asp:DropDownList>
    </p>
    <asp:GridView ID="gvOrders" runat="server" AutoGenerateColumns="false" CssClass="grid"
        DataKeyNames="OrderId" OnRowCommand="gvOrders_RowCommand">
        <Columns>
            <asp:BoundField DataField="OrderId" HeaderText="Order #" />
            <asp:BoundField DataField="CompanyName" HeaderText="Customer" />
            <asp:BoundField DataField="OrderDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="Status" HeaderText="Status" />
            <asp:BoundField DataField="OrderTotal" HeaderText="Total" DataFormatString="{0:C}" />
            <asp:ButtonField CommandName="ViewOrder" Text="View" ButtonType="Link" />
        </Columns>
    </asp:GridView>
</asp:Content>
