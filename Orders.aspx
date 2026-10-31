<%@ Page Title="Theo dõi đơn hàng" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="Demo_btl_10_10.MyOrders" %>
<%@ Import Namespace="Demo_btl_10_10" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container">
        <h1 class="page-title">Theo dõi đơn hàng</h1>

        <% if (Items.Count == 0) { %>
        <div class="cart-empty">
            <p>Bạn chưa có đơn hàng nào.</p>
            <a class="btn" href="Products.aspx">Mua sắm ngay</a>
        </div>
        <% } else { %>

        <% foreach (Order o in Items) { %>
        <details class="my-order" id="don-<%= o.Id %>" <%= o.Id == OpenId ? "open" : "" %>>
            <summary class="my-order-head">
                <span class="my-order-id">Đơn #<%= o.Id %></span>
                <span class="my-order-date"><%: o.CreatedAt.ToString("dd/MM/yyyy HH:mm") %></span>
                <span class="my-order-sum"><%: Money(o.Total) %> · <%= o.TotalQty %> sản phẩm</span>
                <span class="<%= StatusCss(o.Status) %>"><%: o.Status %></span>
            </summary>

            <div class="my-order-body">
                <ul class="my-order-items">
                    <% foreach (OrderItem i in o.Items) { %>
                    <li>
                        <img src="<%: i.Image %>" alt="" />
                        <div class="my-order-item-info">
                            <a href="ProductDetail.aspx?id=<%= i.ProductId %>"><%: i.ProductName %></a>
                            <%= ColorHtml(i.Color) %>
                            <small>Số lượng: <%= i.Qty %> × <%: Money(i.Price) %></small>
                        </div>
                        <strong><%: Money(i.LineTotal) %></strong>
                    </li>
                    <% } %>
                </ul>

                <div class="my-order-foot">
                    <div class="my-order-ship">
                        <div><span>Người nhận:</span> <%: o.ReceiverName %></div>
                        <div><span>Số điện thoại:</span> <%: o.Phone %></div>
                        <div><span>Địa chỉ:</span> <%: o.Address %></div>
                        <% if (!string.IsNullOrEmpty(o.Note)) { %><div><span>Ghi chú:</span> <%: o.Note %></div><% } %>
                    </div>
                    <div class="my-order-total">Tổng cộng: <strong><%: Money(o.Total) %></strong></div>
                </div>
            </div>
        </details>
        <% } %>

        <% } %>
    </div>

</asp:Content>
