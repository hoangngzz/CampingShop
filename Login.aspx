<%@ Page Title="Đăng nhập" Language="C#" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs" Inherits="Demo_btl_10_10.Login" %>

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

        .login-page {
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 40px 16px;
        }

        .login-card {
            width: 100%;
            max-width: 420px;
            padding: 32px;
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 6px 24px rgba(0, 0, 0, 0.08);
        }

        .login-card h2 { margin-bottom: 8px; text-align: center; color: #222; }
        .login-subtitle { margin-bottom: 28px; text-align: center; color: #777; }

        .login-group { margin-bottom: 18px; }
        .login-group label { display: block; margin-bottom: 8px; font-weight: 600; }

        .login-input {
            width: 100%;
            padding: 12px;
            border: 1px solid #d5d9df;
            border-radius: 6px;
        }
        .login-input:focus { outline: none; border-color: #346c43; }

        .login-btn {
            width: 100%;
            padding: 13px;
            border: none;
            border-radius: 6px;
            background: #346c43;
            color: #fff;
            font-size: 16px;
            font-weight: bold;
            cursor: pointer;
        }
        .login-btn:hover { background: #2a5736; }

        .login-message { display: block; margin-top: 16px; text-align: center; }

        .login-links { margin-top: 20px; text-align: center; }
        .login-links a { color: #346c43; text-decoration: none; font-weight: 600; }
        .login-links a:hover { text-decoration: underline; }
        .login-links .back { display: block; margin-top: 10px; font-weight: 400; color: #777; }

        .login-error { color: #dc2626; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-page">
            <div class="login-card">
                <h2>Đăng nhập</h2>
                <p class="login-subtitle">Chào mừng bạn đến với MyCamp</p>

                <div class="login-group">
                    <label for="<%= txtUsername.ClientID %>">Tên đăng nhập</label>
                    <asp:TextBox ID="txtUsername" runat="server"
                        CssClass="login-input"
                        placeholder="Nhập tên đăng nhập" />
                    <asp:RequiredFieldValidator ID="valUsername" runat="server"
                        ControlToValidate="txtUsername"
                        ErrorMessage="Vui lòng nhập tên đăng nhập."
                        CssClass="login-error"
                        Display="Dynamic"
                        ValidationGroup="Login" />
                </div>

                <div class="login-group">
                    <label for="<%= txtPassword.ClientID %>">Mật khẩu</label>
                    <asp:TextBox ID="txtPassword" runat="server"
                        TextMode="Password"
                        CssClass="login-input"
                        placeholder="Nhập mật khẩu" />
                    <asp:RequiredFieldValidator ID="valPassword" runat="server"
                        ControlToValidate="txtPassword"
                        ErrorMessage="Vui lòng nhập mật khẩu."
                        CssClass="login-error"
                        Display="Dynamic"
                        ValidationGroup="Login" />
                </div>

                <asp:Button ID="btnLogin" runat="server"
                    Text="Đăng nhập"
                    CssClass="login-btn"
                    ValidationGroup="Login"
                    OnClick="btnLogin_Click" />

                <asp:Label ID="lblMessage" runat="server" CssClass="login-message" />

                <div class="login-links">
                    Chưa có tài khoản? <a href="Signup.aspx">Đăng ký ngay</a>
                    <a class="back" href="Default.aspx">← Về trang chủ</a>
                </div>
            </div>
        </div>
    </form>
</body>
</html>