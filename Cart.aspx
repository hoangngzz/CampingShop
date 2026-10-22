<%@ Page Title="Giỏ hàng" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Cart.aspx.cs" Inherits="Demo_btl_10_10.Cart" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container">
        <h1 class="page-title">Giỏ hàng</h1>

        <%-- Vừa đặt hàng xong --%>
        <asp:Panel ID="pnlDone" runat="server" CssClass="cart-empty" Visible="false">
            <p>Cảm ơn bạn đã đặt hàng! Mã đơn của bạn là <strong>#<asp:Literal ID="litOrderId" runat="server" /></strong>. Chúng tôi sẽ liên hệ để xác nhận sớm nhất.</p>
            <a class="btn" href="Products.aspx">Tiếp tục mua sắm</a>
        </asp:Panel>

        <%-- Có sản phẩm --%>
        <asp:Panel ID="pnlFilled" runat="server" CssClass="cart-layout">
            <div class="cart-table-wrap">
                <table class="cart-table">
                    <thead>
                        <tr>
                            <th>Sản phẩm</th>
                            <th>Đơn giá</th>
                            <th>Số lượng</th>
                            <th>Thành tiền</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCart" runat="server" OnItemCommand="rptCart_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="cart-item">
                                            <a href='ProductDetail.aspx?id=<%# Eval("ProductId") %>'><img src='<%#: Eval("Image") %>' alt="" /></a>
                                            <div>
                                                <a class="cart-name" href='ProductDetail.aspx?id=<%# Eval("ProductId") %>'><%#: Eval("Name") %></a>
                                                <%# ColorHtml(Eval("Color")) %>
                                            </div>
                                        </div>
                                    </td>
                                    <td><%#: Money(Eval("Price")) %></td>
                                    <td>
                                        <div class="qty">
                                            <asp:Button runat="server" CssClass="qty-btn" Text="−" CommandName="minus" CommandArgument='<%# Eval("Key") %>' ToolTip="Giảm" />
                                            <span><%# Eval("Qty") %></span>
                                            <asp:Button runat="server" CssClass="qty-btn" Text="+" CommandName="plus" CommandArgument='<%# Eval("Key") %>' ToolTip="Tăng" />
                                        </div>
                                    </td>
                                    <td><%#: Money(Eval("LineTotal")) %></td>
                                    <td>
                                        <asp:Button runat="server" CssClass="cart-remove" Text="×" CommandName="del" CommandArgument='<%# Eval("Key") %>' ToolTip="Xóa" />
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <aside class="cart-summary">
                <h2>Tóm tắt đơn hàng</h2>
                <div class="summary-row"><span>Tạm tính</span><span><asp:Literal ID="litSub" runat="server" /></span></div>
                <div class="summary-row"><span>Vận chuyển</span><span>Miễn phí</span></div>
                <div class="summary-row summary-total"><span>Tổng cộng</span><span><asp:Literal ID="litTotal" runat="server" /></span></div>

                <asp:Panel ID="pnlCheckout" runat="server" CssClass="checkout-form" DefaultButton="btnCheckout">
                    <h3>Thông tin nhận hàng</h3>
                    <asp:Label ID="lblCheckoutMsg" runat="server" EnableViewState="false" />
                    <div class="field">
                        <label for="<%= txtName.ClientID %>">Họ và tên</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="input" MaxLength="100" />
                    </div>
                    <div class="field">
                        <label for="<%= txtPhone.ClientID %>">Số điện thoại</label>
                        <asp:TextBox ID="txtPhone" runat="server" CssClass="input" TextMode="Phone" MaxLength="20" />
                    </div>
                    <div class="field">
                        <label for="<%= txtAddress.ClientID %>">Địa chỉ</label>
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="input" MaxLength="250" />
                    </div>
                    <div class="field">
                        <label for="<%= txtNote.ClientID %>">Ghi chú (không bắt buộc)</label>
                        <asp:TextBox ID="txtNote" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" MaxLength="500" />
                    </div>
                    <asp:Button ID="btnCheckout" runat="server" Text="Đặt hàng" CssClass="btn" OnClick="btnCheckout_Click" />
                </asp:Panel>
            </aside>
        </asp:Panel>

        <%-- Giỏ trống --%>
        <asp:Panel ID="pnlEmpty" runat="server" CssClass="cart-empty" Visible="false">
            <p>Giỏ hàng của bạn đang trống.</p>
            <a class="btn" href="Products.aspx">Tiếp tục mua sắm</a>
        </asp:Panel>
    </div>

</asp:Content>
