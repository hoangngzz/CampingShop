<%@ Page Title="Sản phẩm" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="Demo_btl_10_10.Products" %>
<%@ Register Src="~/Controls/ProductCards.ascx" TagPrefix="uc" TagName="ProductCards" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container shop-layout">

        <aside>
            <div class="side-box">
                <h2 class="side-title">Danh mục sản phẩm</h2>
                <ul class="cat-list">
                    <li><a href="<%: Link(Q, "", Gia, Sort) %>" class="<%= Loai == "" ? "active" : "" %>">Tất cả sản phẩm</a></li>
                    <% foreach (var c in Categories) { %>
                    <li><a href="<%: Link(Q, c.Key, Gia, Sort) %>" class="<%= Loai == c.Key ? "active" : "" %>"><%: c.Value %></a></li>
                    <% } %>
                </ul>
            </div>

            <div class="side-box">
                <h2 class="side-title">Bộ lọc sản phẩm</h2>
                <details class="filter-group" <%= Gia != "" ? "open" : "" %>>
                    <summary>Chọn mức giá</summary>
                    <ul class="filter-options">
                        <% foreach (var r in PriceRanges) { %>
                        <li><a href="<%: Link(Q, Loai, r.Key, Sort) %>" class="<%= Gia == r.Key ? "active" : "" %>"><%: r.Label %></a></li>
                        <% } %>
                    </ul>
                </details>
            </div>
        </aside>

        <section>
            <div class="shop-toolbar">
                <div class="sort">
                    <span>Sắp xếp:</span>
                    <div class="sort-dropdown">
                        <button type="button" class="sort-toggle"><%: CurrentSortLabel %></button>
                        <div class="sort-menu">
                            <% foreach (var s in SortOptions) { %>
                            <a href="<%: Link(Q, Loai, Gia, s.Key) %>" class="<%= Sort == s.Key ? "active" : "" %>"><%: s.Label %></a>
                            <% } %>
                        </div>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlInfo" runat="server" CssClass="result-info" Visible="false">
                <asp:Literal ID="litInfo" runat="server" />
            </asp:Panel>

            <uc:ProductCards ID="gridProducts" runat="server" />

            <asp:Panel ID="pnlNoResult" runat="server" CssClass="no-result" Visible="false">
                Không tìm thấy sản phẩm phù hợp. <a href="Products.aspx">Xem tất cả sản phẩm</a>
            </asp:Panel>
        </section>

    </div>

</asp:Content>
