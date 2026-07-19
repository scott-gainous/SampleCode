<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ContosoOrders.Web.DefaultPage" Title="Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Dashboard</h2>
    <div class="summary">
        <div class="tile">
            <span class="big"><asp:Literal ID="litCustomerCount" runat="server" /></span>
            <span>Active customers</span>
        </div>
        <div class="tile">
            <span class="big"><asp:Literal ID="litProductCount" runat="server" /></span>
            <span>Products</span>
        </div>
        <div class="tile">
            <span class="big"><asp:Literal ID="litOpenOrders" runat="server" /></span>
            <span>Open orders</span>
        </div>
        <div class="tile">
            <span class="big"><asp:Literal ID="litRevenue" runat="server" /></span>
            <span>Revenue to date</span>
        </div>
    </div>
</asp:Content>
