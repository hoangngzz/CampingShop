<%@ Page Title="Chi tiết sản phẩm" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProductDetail.aspx.cs" Inherits="Demo_btl_10_10.ProductDetail" %>
<%@ Register Src="~/Controls/ProductCards.ascx" TagPrefix="uc" TagName="ProductCards" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container">

        <%-- ================= CÓ SẢN PHẨM ================= --%>
        <asp:Panel ID="pnlDetail" runat="server">

            <p class="breadcrumb"><a href="Default.aspx">Trang chủ</a> / <a href="Products.aspx">Sản phẩm</a> / <span><%: Product.Name %></span></p>

            <div class="detail" id="detail">

                <div class="detail-gallery">
                    <div class="detail-img">
                        <% if (Product.OldPrice > 0) { %><span class="badge-sale">Sale</span><% } %>
                        <img src="<%: MainImage %>" alt="<%: Product.Name %>" />
                    </div>
                    <%-- Ảnh phụ: bấm vào để đổi ảnh chính (?img=) --%>
                    <% if (Product.Images.Count > 1) { %>
                    <div class="thumbs">
                        <% for (int i = 0; i < Product.Images.Count; i++) { %>
                        <a class="thumb<%= i == ImageIndex ? " active" : "" %>" href="ProductDetail.aspx?id=<%= Product.Id %>&amp;img=<%= i %>#detail" title="Ảnh <%= i + 1 %>">
                            <img src="<%: Product.Images[i] %>" alt="<%: Product.Name %> - ảnh <%= i + 1 %>" />
                        </a>
                        <% } %>
                    </div>
                    <% } %>
                </div>

                <div class="detail-info">
                    <h1><%: Product.Name %></h1>

                    <a class="rating-line" href="#reviews">
                        <% if (RatingCount > 0) { %>
                        <span class="stars"><%= Stars(AvgRating) %></span>
                        <span><%: AvgText %>/5 (<%= RatingCount %> đánh giá)</span>
                        <% } else { %>
                        <span class="stars"><%= Stars(0) %></span>
                        <span>Chưa có đánh giá</span>
                        <% } %>
                    </a>

                    <div class="p-price">
                        <span class="p-now"><%: Money(Product.Price) %></span>
                        <% if (Product.OldPrice > 0) { %><span class="p-old"><%: Money(Product.OldPrice) %></span><% } %>
                    </div>

                    <p class="detail-desc"><%: Product.Description %></p>
                    <ul class="detail-meta">
                        <li>Chất liệu: <%: Product.Material %></li>
                        <li>Kích cỡ: <%: Product.Sizes %></li>
                        <li>Tình trạng: <%: Product.Status %></li>
                    </ul>

                    <asp:Panel ID="pnlActions" runat="server" DefaultButton="btnAdd">

                        <%-- Chọn màu (chỉ hiện khi sản phẩm có màu) --%>
                        <asp:Panel ID="pnlColors" runat="server" CssClass="color-block">
                            <span class="color-label">Màu sắc:</span>
                            <asp:RadioButtonList ID="rblColors" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal" CssClass="color-options" />
                        </asp:Panel>

                        <div class="detail-actions">
                            <div class="qty">
                                <asp:TextBox ID="txtQty" runat="server" TextMode="Number" Text="1" min="1" max="99" />
                            </div>
                            <asp:Button ID="btnAdd" runat="server" Text="Thêm vào giỏ" CssClass="btn" OnClick="btnAdd_Click" />
                        </div>
                    </asp:Panel>

                    <asp:Label ID="lblAddMsg" runat="server" EnableViewState="false" />
                    <asp:Panel ID="pnlAdded" runat="server" CssClass="add-msg" Visible="false">
                        Đã thêm vào giỏ hàng. <a href="Cart.aspx">Xem giỏ hàng</a>
                    </asp:Panel>
                </div>
            </div>

            <%-- ================= ĐÁNH GIÁ ================= --%>
            <section class="reviews" id="reviews">
                <h2 class="section-title">Đánh giá sản phẩm</h2>

                <div class="review-layout">
                    <div class="review-summary">
                        <div class="review-avg"><%: RatingCount > 0 ? AvgText : "–" %></div>
                        <div class="stars stars-lg"><%= Stars(AvgRating) %></div>
                        <div class="review-count"><%= RatingCount %> đánh giá</div>
                    </div>

                    <div class="review-form">
                        <asp:Label ID="lblReviewMsg" runat="server" EnableViewState="false" />

                        <asp:Panel ID="pnlReviewForm" runat="server" DefaultButton="btnReview">
                            <h3>Viết đánh giá của bạn</h3>

                            <div class="field">
                                <label>Chọn số sao</label>
                                <div class="star-input">
                                    <% for (int s = 5; s >= 1; s--) { %>
                                    <input type="radio" id="star<%= s %>" name="rating" value="<%= s %>" <%= SelectedRating == s ? "checked" : "" %> />
                                    <label for="star<%= s %>" title="<%= s %> sao">★</label>
                                    <% } %>
                                </div>
                            </div>

                            <div class="field">
                                <label for="<%= txtComment.ClientID %>">Bình luận</label>
                                <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="4" CssClass="input" placeholder="Chia sẻ cảm nhận của bạn về sản phẩm..." />
                            </div>

                            <asp:Button ID="btnReview" runat="server" Text="Gửi đánh giá" CssClass="btn" OnClick="btnReview_Click" />
                        </asp:Panel>

                        <asp:Panel ID="pnlLoginHint" runat="server" CssClass="review-login">
                            Vui lòng <a href="Login.aspx">đăng nhập</a> để viết đánh giá.
                        </asp:Panel>
                    </div>
                </div>

                <asp:Repeater ID="rptReviews" runat="server">
                    <HeaderTemplate><div class="review-list"></HeaderTemplate>
                    <ItemTemplate>
                        <div class="review-item">
                            <div class="review-head">
                                <strong><%#: Eval("AuthorName") %></strong>
                                <span class="stars"><%# Stars(Eval("Rating")) %></span>
                                <span class="review-date"><%# ((DateTime)Eval("CreatedAt")).ToString("dd/MM/yyyy HH:mm") %></span>
                            </div>
                            <p class="review-text"><%#: Eval("Comment") %></p>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate></div></FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoReviews" runat="server" CssClass="no-review" Visible="false">
                    Chưa có đánh giá nào. Hãy là người đầu tiên đánh giá sản phẩm này.
                </asp:Panel>
            </section>

            <h2 class="section-title">Sản phẩm liên quan</h2>
            <uc:ProductCards ID="gridRelated" runat="server" />
        </asp:Panel>

        <%-- ================= KHÔNG TÌM THẤY ================= --%>
        <asp:Panel ID="pnlNotFound" runat="server" CssClass="no-result" Visible="false">
            Không tìm thấy sản phẩm này. <a href="Products.aspx">Quay lại danh sách sản phẩm</a>
        </asp:Panel>

    </div>

</asp:Content>
