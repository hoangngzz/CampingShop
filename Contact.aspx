<%@ Page Title="Liên hệ" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="Demo_btl_10_10.Contact" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container">
        <p class="breadcrumb"><a href="Default.aspx">Trang chủ</a> / <span>Liên hệ</span></p>
        <h1 class="page-title">Liên hệ với MyCamp</h1>
        <p class="page-lead">Bạn cần tư vấn chọn lều, đèn hay xe đạp cho chuyến đi sắp tới? Hãy để lại lời nhắn, chúng tôi sẽ phản hồi sớm nhất.</p>

        <div class="contact-layout">

            <div class="contact-info">
                <div class="contact-card">
                    <svg class="ico"><use href="#i-pin" /></svg>
                    <div><h3>Địa chỉ</h3><p>Hà Nội, Việt Nam</p></div>
                </div>
                <div class="contact-card">
                    <svg class="ico"><use href="#i-phone" /></svg>
                    <div><h3>Điện thoại</h3><p><a href="tel:0987654321">0987654321</a></p></div>
                </div>
                <div class="contact-card">
                    <svg class="ico"><use href="#i-mail" /></svg>
                    <div><h3>Email</h3><p><a href="mailto:hotro@Mycamp.vn">hotro@Mycamp.vn</a></p></div>
                </div>
                <div class="contact-card">
                    <svg class="ico"><use href="#i-clock" /></svg>
                    <div><h3>Giờ làm việc</h3><p>8:00 - 21:00, hằng ngày</p></div>
                </div>
            </div>

            <asp:Panel ID="pnlForm" runat="server" CssClass="contact-form" DefaultButton="btnSend">
                <h2>Gửi lời nhắn</h2>
                <asp:Label ID="lblMsg" runat="server" EnableViewState="false" />

                <div class="form-grid">
                    <div class="field">
                        <label for="<%= txtName.ClientID %>">Họ và tên *</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="input" MaxLength="100" />
                    </div>
                    <div class="field">
                        <label for="<%= txtEmail.ClientID %>">Email *</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="input" TextMode="Email" MaxLength="254" />
                    </div>
                    <div class="field">
                        <label for="<%= txtPhone.ClientID %>">Số điện thoại</label>
                        <asp:TextBox ID="txtPhone" runat="server" CssClass="input" TextMode="Phone" MaxLength="20" />
                    </div>
                    <div class="field">
                        <label for="<%= txtSubject.ClientID %>">Chủ đề</label>
                        <asp:TextBox ID="txtSubject" runat="server" CssClass="input" MaxLength="150" />
                    </div>
                    <div class="field full">
                        <label for="<%= txtMessage.ClientID %>">Nội dung *</label>
                        <asp:TextBox ID="txtMessage" runat="server" CssClass="input" TextMode="MultiLine" Rows="6" />
                        <span class="hint">Từ 10 đến 1000 ký tự.</span>
                    </div>
                </div>

                <div class="form-actions">
                    <asp:Button ID="btnSend" runat="server" Text="Gửi lời nhắn" CssClass="btn" OnClick="btnSend_Click" />
                </div>
            </asp:Panel>

        </div>
    </div>

</asp:Content>
