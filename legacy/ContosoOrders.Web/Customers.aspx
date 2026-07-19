<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Customers.aspx.cs" Inherits="ContosoOrders.Web.CustomersPage" Title="Customers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Customers</h2>
    <p>
        <asp:CheckBox ID="chkIncludeInactive" runat="server" Text="Include inactive"
            AutoPostBack="true" OnCheckedChanged="chkIncludeInactive_CheckedChanged" />
        &nbsp;
        <asp:HyperLink ID="lnkNew" runat="server" NavigateUrl="~/CustomerEdit.aspx" Text="New customer" />
    </p>
    <asp:GridView ID="gvCustomers" runat="server" AutoGenerateColumns="false" CssClass="grid"
        DataKeyNames="CustomerId" OnRowCommand="gvCustomers_RowCommand">
        <Columns>
            <asp:BoundField DataField="CompanyName" HeaderText="Company" />
            <asp:BoundField DataField="ContactName" HeaderText="Contact" />
            <asp:BoundField DataField="Email" HeaderText="Email" />
            <asp:BoundField DataField="City" HeaderText="City" />
            <asp:BoundField DataField="Country" HeaderText="Country" />
            <asp:CheckBoxField DataField="IsActive" HeaderText="Active" />
            <asp:ButtonField CommandName="EditCustomer" Text="Edit" ButtonType="Link" />
        </Columns>
    </asp:GridView>
</asp:Content>
