<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="ContosoOrders.Web.ProductsPage" Title="Products" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Products</h2>
    <p>
        <asp:CheckBox ID="chkIncludeDiscontinued" runat="server" Text="Include discontinued"
            AutoPostBack="true" OnCheckedChanged="chkIncludeDiscontinued_CheckedChanged" />
    </p>
    <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="false" CssClass="grid">
        <Columns>
            <asp:BoundField DataField="Sku" HeaderText="SKU" />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="UnitPrice" HeaderText="Unit price" DataFormatString="{0:C}" />
            <asp:BoundField DataField="UnitsInStock" HeaderText="In stock" />
            <asp:CheckBoxField DataField="Discontinued" HeaderText="Discontinued" />
        </Columns>
    </asp:GridView>
</asp:Content>
