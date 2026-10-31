<%@ Page Title="Quản trị" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="Demo_btl_10_10.Admin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container">
        <h1 class="page-title">Quản trị</h1>

        <div class="admin-tabs">
            <asp:HyperLink ID="tabProducts" runat="server" NavigateUrl="~/Admin.aspx?tab=sp" CssClass="admin-tab active">Sản phẩm</asp:HyperLink>
            <asp:HyperLink ID="tabCustomers" runat="server" NavigateUrl="~/Admin.aspx?tab=kh" CssClass="admin-tab">Tài khoản</asp:HyperLink>
            <asp:HyperLink ID="tabOrders" runat="server" NavigateUrl="~/Admin.aspx?tab=dh" CssClass="admin-tab">Đơn hàng</asp:HyperLink>
        </div>

        <%-- ================= SẢN PHẨM ================= --%>
        <asp:Panel ID="pnlProducts" runat="server">

            <div class="admin-bar">
                <h2>Danh sách sản phẩm (<asp:Literal ID="litProductCount" runat="server" />)</h2>
                <asp:Button ID="btnNewProduct" runat="server" Text="+ Thêm sản phẩm" CssClass="btn" OnClick="btnNewProduct_Click" />
            </div>

            <asp:Label ID="lblProductMsg" runat="server" EnableViewState="false" />

            <asp:Panel ID="pnlProductForm" runat="server" Visible="false" CssClass="admin-form" DefaultButton="btnSaveProduct">
                <h3>
                    <asp:Literal ID="litProductFormTitle" runat="server" /></h3>
                <asp:HiddenField ID="hfProductId" runat="server" Value="0" />

                <div class="form-grid">
                    <div class="field full">
                        <label for="<%= txtPName.ClientID %>">Tên sản phẩm</label>
                        <asp:TextBox ID="txtPName" runat="server" CssClass="input" MaxLength="200" />
                    </div>
                    <div class="field">
                        <label for="<%= ddlPCat.ClientID %>">Loại sản phẩm</label>
                        <asp:DropDownList ID="ddlPCat" runat="server" CssClass="input" />
                    </div>
                    <div class="field">
                        <label for="<%= ddlPStatus.ClientID %>">Tình trạng</label>
                        <asp:DropDownList ID="ddlPStatus" runat="server" CssClass="input">
                            <asp:ListItem Text="Còn hàng" Value="Còn hàng" />
                            <asp:ListItem Text="Hết hàng" Value="Hết hàng" />
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <label for="<%= txtPPrice.ClientID %>">Giá bán (đ)</label>
                        <asp:TextBox ID="txtPPrice" runat="server" CssClass="input" TextMode="Number" />
                    </div>
                    <div class="field">
                        <label for="<%= txtPOld.ClientID %>">Giá cũ (đ)</label>
                        <asp:TextBox ID="txtPOld" runat="server" CssClass="input" TextMode="Number" />
                        <span class="hint">Để trống nếu không giảm giá.</span>
                    </div>
                    <div class="field full">
                        <label for="<%= txtPColors.ClientID %>">Màu sắc</label>
                        <asp:TextBox ID="txtPColors" runat="server" CssClass="input" MaxLength="200" />
                        <span class="hint">Các màu khách có thể chọn, cách nhau bằng dấu phẩy, ví dụ: Đen, Xám, Cam. Để trống nếu sản phẩm chỉ có một màu.</span>
                    </div>
                    <div class="field full">
                        <label for="<%= txtPImages.ClientID %>">Đường dẫn ảnh</label>
                        <asp:TextBox ID="txtPImages" runat="server" CssClass="input" TextMode="MultiLine" Rows="4" />
                        <span class="hint">Mỗi dòng một ảnh, ví dụ Images/sp1_1.jpg. Ảnh đầu tiên là ảnh đại diện.</span>
                    </div>
                    <div class="field full">
                        <label for="<%= txtPDesc.ClientID %>">Mô tả</label>
                        <asp:TextBox ID="txtPDesc" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="field">
                        <label for="<%= txtPMaterial.ClientID %>">Chất liệu</label>
                        <asp:TextBox ID="txtPMaterial" runat="server" CssClass="input" MaxLength="200" />
                    </div>
                    <div class="field">
                        <label for="<%= txtPSizes.ClientID %>">Kích cỡ</label>
                        <asp:TextBox ID="txtPSizes" runat="server" CssClass="input" MaxLength="200" />
                    </div>
                </div>

                <div class="form-actions">
                    <asp:Button ID="btnSaveProduct" runat="server" Text="Lưu sản phẩm" CssClass="btn" OnClick="btnSaveProduct_Click" />
                    <asp:Button ID="btnCancelProduct" runat="server" Text="Hủy" CssClass="btn btn-ghost" OnClick="btnCancelProduct_Click" />
                </div>
            </asp:Panel>

            <div class="table-scroll">
                <table class="admin-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Ảnh</th>
                            <th>Tên sản phẩm</th>
                            <th>Loại</th>
                            <th>Giá bán</th>
                            <th>Giá cũ</th>
                            <th>Tình trạng</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptProducts" runat="server" OnItemCommand="rptProducts_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("Id") %></td>
                                    <td>
                                        <img class="admin-thumb" src='<%#: Eval("Thumb") %>' alt="" /></td>
                                    <td><%#: Eval("Name") %></td>
                                    <td><%#: CatName(Eval("Cat")) %></td>
                                    <td><%# Money(Eval("Price")) %></td>
                                    <td><%# Money(Eval("OldPrice")) %></td>
                                    <td><%#: Eval("Status") %></td>
                                    <td class="admin-actions">
                                        <asp:LinkButton runat="server" CssClass="link-btn" CommandName="edit" CommandArgument='<%# Eval("Id") %>'>Sửa</asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="link-btn danger" CommandName="delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Xóa sản phẩm này?');">Xóa</asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </asp:Panel>

        <%-- ================= KHÁCH HÀNG ================= --%>
        <asp:Panel ID="pnlCustomers" runat="server">

            <div class="admin-bar">
                <h2>Danh sách tài khoản (<asp:Literal ID="litCustomerCount" runat="server" />)</h2>
            </div>

            <asp:Label ID="lblCustomerMsg" runat="server" EnableViewState="false" />

            <asp:Panel ID="pnlCustomerForm" runat="server" Visible="false" CssClass="admin-form" DefaultButton="btnSaveCustomer">
                <h3>
                    <asp:Literal ID="litCustomerFormTitle" runat="server" /></h3>
                <asp:HiddenField ID="hfCustomerId" runat="server" Value="0" />

                <div class="form-grid">
                    <div class="field">
                        <label for="<%= txtCName.ClientID %>">Họ và tên</label>
                        <asp:TextBox ID="txtCName" runat="server" CssClass="input" MaxLength="100" />
                    </div>
                    <div class="field">
                        <label for="<%= txtCEmail.ClientID %>">Email</label>
                        <asp:TextBox ID="txtCEmail" runat="server" CssClass="input" TextMode="Email" MaxLength="254" />
                    </div>
                    <div class="field">
                        <label for="<%= txtCUser.ClientID %>">Tên đăng nhập</label>
                        <asp:TextBox ID="txtCUser" runat="server" CssClass="input" MaxLength="50" />
                    </div>
                    <div class="field">
                        <label for="<%= ddlCRole.ClientID %>">Vai trò</label>
                        <asp:DropDownList ID="ddlCRole" runat="server" CssClass="input">
                            <asp:ListItem Text="Khách hàng" Value="customer" />
                            <asp:ListItem Text="Quản trị viên" Value="admin" />
                        </asp:DropDownList>
                    </div>
                    <div class="field full">
                        <label for="<%= txtCPass.ClientID %>">Mật khẩu</label>
                        <asp:TextBox ID="txtCPass" runat="server" CssClass="input" TextMode="Password" MaxLength="128" autocomplete="new-password" />
                        <span class="hint">Tối thiểu 8 ký tự. Để trống nếu không muốn đổi mật khẩu.</span>
                    </div>
                </div>

                <div class="form-actions">
                    <asp:Button ID="btnSaveCustomer" runat="server" Text="Lưu tài khoản" CssClass="btn" OnClick="btnSaveCustomer_Click" />
                    <asp:Button ID="btnCancelCustomer" runat="server" Text="Hủy" CssClass="btn btn-ghost" OnClick="btnCancelCustomer_Click" />
                </div>
            </asp:Panel>

            <div class="table-scroll">
                <table class="admin-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Họ và tên</th>
                            <th>Email</th>
                            <th>Tên đăng nhập</th>
                            <th>Vai trò</th>
                            <th>Ngày tạo</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCustomers" runat="server" OnItemCommand="rptCustomers_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("Id") %></td>
                                    <td><%#: Eval("FullName") %></td>
                                    <td><%#: Eval("Email") %></td>
                                    <td><%#: Eval("Username") %></td>
                                    <td><span class='<%# RoleCss(Eval("Role")) %>'><%# RoleName(Eval("Role")) %></span></td>
                                    <td><%# ((DateTime)Eval("CreatedAt")).ToString("dd/MM/yyyy HH:mm") %></td>
                                    <td class="admin-actions">
                                        <asp:LinkButton runat="server" CssClass="link-btn" CommandName="edit" CommandArgument='<%# Eval("Id") %>'>Sửa</asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="link-btn danger" CommandName="delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Xóa tài khoản này?');">Xóa</asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </asp:Panel>

        <%-- ================= ĐƠN HÀNG ================= --%>
        <asp:Panel ID="pnlOrders" runat="server">

            <div class="admin-bar">
                <h2>Danh sách đơn hàng (<asp:Literal ID="litOrderCount" runat="server" />)</h2>
            </div>

            <asp:Label ID="lblOrderMsg" runat="server" EnableViewState="false" />

            <%-- Chi tiết đơn đang xem (Admin.aspx?tab=dh&id=...) --%>
            <asp:Panel ID="pnlOrderDetail" runat="server" Visible="false" CssClass="order-detail">
                <h3>Đơn hàng #<%= CurOrder.Id %></h3>

                <div class="order-info">
                    <div><span>Ngày đặt:</span> <%: CurOrder.CreatedAt.ToString("dd/MM/yyyy HH:mm") %></div>
                    <div><span>Trạng thái:</span> <span class="<%= StatusCss(CurOrder.Status) %>"><%: CurOrder.Status %></span></div>
                    <div><span>Người nhận:</span> <%: CurOrder.ReceiverName %></div>
                    <div><span>Số điện thoại:</span> <%: CurOrder.Phone %></div>
                    <div><span>Địa chỉ:</span> <%: CurOrder.Address %></div>
                    <div><span>Tài khoản:</span> <%: CustomerLabel(CurOrder) %></div>
                    <% if (!string.IsNullOrEmpty(CurOrder.Note)) { %>
                    <div><span>Ghi chú:</span> <%: CurOrder.Note %></div>
                    <% } %>
                </div>

                <div class="table-scroll">
                    <table class="admin-table">
                        <thead>
                            <tr>
                                <th>Sản phẩm</th>
                                <th>Đơn giá</th>
                                <th>Số lượng</th>
                                <th>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptOrderItems" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <div class="order-item-cell">
                                                <img class="admin-thumb" src='<%#: Eval("Image") %>' alt="" />
                                                <div>
                                                    <%#: Eval("ProductName") %>
                                                    <%# ColorHtml(Eval("Color")) %>
                                                </div>
                                            </div>
                                        </td>
                                        <td><%#: OrderMoney(Eval("Price")) %></td>
                                        <td><%# Eval("Qty") %></td>
                                        <td><%#: OrderMoney(Eval("LineTotal")) %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>

                <div class="order-total">Tổng cộng: <%: OrderMoney(CurOrder.Total) %></div>

                <div class="order-status-form">
                    <label for="<%= ddlOrderStatus.ClientID %>"><strong>Đổi trạng thái:</strong></label>
                    <asp:DropDownList ID="ddlOrderStatus" runat="server" CssClass="input" />
                    <asp:Button ID="btnUpdateStatus" runat="server" Text="Cập nhật trạng thái" CssClass="btn" OnClick="btnUpdateStatus_Click" />
                    <a class="btn btn-ghost" href="Admin.aspx?tab=dh">Đóng</a>
                </div>
            </asp:Panel>

            <div class="table-scroll">
                <table class="admin-table">
                    <thead>
                        <tr>
                            <th>Mã đơn</th>
                            <th>Ngày đặt</th>
                            <th>Người nhận</th>
                            <th>Điện thoại</th>
                            <th>Sản phẩm</th>
                            <th>Tổng tiền</th>
                            <th>Trạng thái</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptOrders" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td>#<%# Eval("Id") %></td>
                                    <td><%# ((DateTime)Eval("CreatedAt")).ToString("dd/MM/yyyy HH:mm") %></td>
                                    <td><%#: Eval("ReceiverName") %></td>
                                    <td><%#: Eval("Phone") %></td>
                                    <td><%# Thumbs(Container.DataItem) %></td>
                                    <td><%#: OrderMoney(Eval("Total")) %></td>
                                    <td><span class='<%# StatusCss(Eval("Status")) %>'><%#: Eval("Status") %></span></td>
                                    <td class="admin-actions">
                                        <a class="link-btn" href="Admin.aspx?tab=dh&amp;id=<%# Eval("Id") %>">Xem</a>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
                <asp:Panel ID="pnlNoOrders" runat="server" CssClass="no-result" Visible="false">Chưa có đơn hàng nào.</asp:Panel>
            </div>
        </asp:Panel>
    </div>

</asp:Content>
