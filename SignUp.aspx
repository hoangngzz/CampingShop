<%@ Page Title="Đăng ký tài khoản" Language="C#" AutoEventWireup="true"
    CodeBehind="Signup.aspx.cs" Inherits="Demo_btl_10_10.Signup" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }

        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            color: #333;
            background: #eef5ea;
        }

        .signup-page {
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 40px 16px;
        }

        .signup-container {
            width: 100%;
            max-width: 820px;
            padding: 35px;
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0,0,0,.08);
        }

        .signup-title { text-align: center; font-size: 28px; margin-bottom: 8px; color: #222; }
        .signup-subtitle { text-align: center; color: #777; margin-bottom: 28px; }

        /* Chia đôi form: 2 cột nằm cùng 1 hàng */
        .signup-columns {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 0 32px;
            align-items: start;
        }

        .signup-field { margin-bottom: 18px; }
        .signup-field label { display: block; margin-bottom: 7px; font-weight: 600; }

        .signup-input {
            width: 100%;
            padding: 12px 14px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 15px;
        }
        .signup-input:focus { outline: none; border-color: #346c43; }

        .signup-button {
            width: 100%;
            margin-top: 6px;
            padding: 13px;
            background: #346c43;
            color: #fff;
            border: none;
            border-radius: 6px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
        }
        .signup-button:hover { background: #2a5736; }

        .signup-login { text-align: center; margin-top: 20px; }
        .signup-login a { color: #346c43; text-decoration: none; font-weight: 600; }
        .signup-login a:hover { text-decoration: underline; }
        .signup-login .back { display: block; margin-top: 10px; font-weight: 400; color: #777; }

        .signup-message { display: block; margin-top: 15px; }

        @media (max-width: 700px) {
            .signup-container { padding: 25px 20px; }
            .signup-columns { grid-template-columns: 1fr; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="signup-page">
            <div class="signup-container">
                <h1 class="signup-title">Tạo tài khoản</h1>
                <p class="signup-subtitle">Đăng ký để trải nghiệm mua sắm tại MyCamp</p>

                <div class="signup-columns">

                    <!-- Cột trái: thông tin cá nhân -->
                    <div>
                        <div class="signup-field">
                            <label for="<%= txtFullName.ClientID %>">Họ và tên</label>
                            <asp:TextBox ID="txtFullName" runat="server"
                                CssClass="signup-input" MaxLength="100"
                                placeholder="Nhập họ và tên" />
                            <asp:RequiredFieldValidator ID="valFullName" runat="server"
                                ControlToValidate="txtFullName"
                                ErrorMessage="Vui lòng nhập họ và tên."
                                ValidationGroup="Signup" ForeColor="Red" Display="Dynamic" />
                        </div>

                        <div class="signup-field">
                            <label for="<%= txtEmail.ClientID %>">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server"
                                CssClass="signup-input" TextMode="Email" MaxLength="254"
                                placeholder="example@email.com" />
                            <asp:RequiredFieldValidator ID="valEmailRequired" runat="server"
                                ControlToValidate="txtEmail"
                                ErrorMessage="Vui lòng nhập email."
                                ValidationGroup="Signup" ForeColor="Red" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="valEmailFormat" runat="server"
                                ControlToValidate="txtEmail"
                                ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                                ErrorMessage="Email không hợp lệ."
                                ValidationGroup="Signup" ForeColor="Red" Display="Dynamic" />
                        </div>

                        <div class="signup-field">
                            <label for="<%= txtUsername.ClientID %>">Tên đăng nhập</label>
                            <asp:TextBox ID="txtUsername" runat="server"
                                CssClass="signup-input" MaxLength="50"
                                placeholder="Nhập tên đăng nhập" />
                            <asp:RequiredFieldValidator ID="valUsername" runat="server"
                                ControlToValidate="txtUsername"
                                ErrorMessage="Vui lòng nhập tên đăng nhập."
                                ValidationGroup="Signup" ForeColor="Red" Display="Dynamic" />
                        </div>
                    </div>

                    <!-- Cột phải: mật khẩu -->
                    <div>
                        <div class="signup-field">
                            <label for="<%= txtPassword.ClientID %>">Mật khẩu</label>
                            <asp:TextBox ID="txtPassword" runat="server"
                                CssClass="signup-input" TextMode="Password" MaxLength="128"
                                placeholder="Ít nhất 8 ký tự" />
                            <asp:RequiredFieldValidator ID="valPassword" runat="server"
                                ControlToValidate="txtPassword"
                                ErrorMessage="Vui lòng nhập mật khẩu."
                                ValidationGroup="Signup" ForeColor="Red" Display="Dynamic" />
                            <asp:RegularExpressionValidator ID="valPasswordLength" runat="server"
                                ControlToValidate="txtPassword"
                                ValidationExpression="^[\s\S]{8,128}$"
                                ErrorMessage="Mật khẩu phải có từ 8 đến 128 ký tự."
                                ValidationGroup="Signup" ForeColor="Red" Display="Dynamic" />
                        </div>

                        <div class="signup-field">
                            <label for="<%= txtConfirmPassword.ClientID %>">Xác nhận mật khẩu</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server"
                                CssClass="signup-input" TextMode="Password" MaxLength="128"
                                placeholder="Nhập lại mật khẩu" />
                            <asp:CompareValidator ID="valConfirmPassword" runat="server"
                                ControlToValidate="txtConfirmPassword"
                                ControlToCompare="txtPassword"
                                ErrorMessage="Mật khẩu xác nhận không khớp."
                                ValidationGroup="Signup" ForeColor="Red" Display="Dynamic" />
                        </div>
                    </div>
                </div>

                <asp:Button ID="btnSignup" runat="server"
                    Text="Đăng ký tài khoản"
                    CssClass="signup-button"
                    ValidationGroup="Signup"
                    OnClick="btnSignup_Click" />

                <asp:Label ID="lblMessage" runat="server"
                    CssClass="signup-message" EnableViewState="false" />

                <div class="signup-login">
                    Đã có tài khoản? <a href="Login.aspx">Đăng nhập</a>
                    <a class="back" href="Default.aspx">← Về trang chủ</a>
                </div>
            </div>
        </div>
    </form>
</body>
</html>