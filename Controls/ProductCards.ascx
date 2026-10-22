<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductCards.ascx.cs" Inherits="Demo_btl_10_10.ProductCards" %>

<%-- Lưới thẻ sản phẩm dùng chung (trang chủ, danh sách, sản phẩm liên quan). Trang cha gán thuộc tính Products. --%>
<div class="p-grid">
    <asp:Repeater ID="rptCards" runat="server" OnItemCommand="rptCards_ItemCommand">
        <ItemTemplate>
            <div class="p-card">
                <a class="p-link" href='ProductDetail.aspx?id=<%# Eval("Id") %>'>
                    <%# SaleBadge(Eval("OldPrice")) %>
                    <img src='<%#: Eval("Thumb") %>' alt='<%#: Eval("Name") %>' />
                    <h3 class="p-name"><%#: Eval("Name") %></h3>
                    <div class="p-price">
                        <span class="p-now"><%#: Money(Eval("Price")) %></span>
                        <%# OldPrice(Eval("OldPrice")) %>
                    </div>
                </a>
                <asp:Button runat="server" CssClass="btn btn-add" CommandName="add" CommandArgument='<%# Eval("Id") %>'
                    Text='<%# (bool)Eval("InStock") ? "Thêm" : "Hết hàng" %>'
                    Enabled='<%# (bool)Eval("InStock") %>' />
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
