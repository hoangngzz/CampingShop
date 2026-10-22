<%@ Page Title="Tin tức" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="News.aspx.cs" Inherits="Demo_btl_10_10.News" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container">

        <%-- ================= DANH SÁCH BÀI VIẾT ================= --%>
        <asp:Panel ID="pnlList" runat="server">
            <p class="breadcrumb"><a href="Default.aspx">Trang chủ</a> / <span>Tin tức</span></p>
            <h1 class="page-title">Tin tức &amp; kinh nghiệm</h1>

            <div class="news-grid">
                <asp:Repeater ID="rptNews" runat="server">
                    <ItemTemplate>
                        <article class="news-card">
                            <a class="news-img" href='News.aspx?id=<%# Eval("Id") %>'>
                                <img src='<%#: Eval("Image") %>' alt='<%#: Eval("Title") %>' />
                            </a>
                            <div class="news-body">
                                <span class="news-date"><%# ((DateTime)Eval("CreatedAt")).ToString("dd/MM/yyyy") %></span>
                                <h2><a href='News.aspx?id=<%# Eval("Id") %>'><%#: Eval("Title") %></a></h2>
                                <p><%#: Eval("Summary") %></p>
                                <a class="news-more" href='News.aspx?id=<%# Eval("Id") %>'>Đọc tiếp →</a>
                            </div>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <%-- ================= CHI TIẾT BÀI VIẾT ================= --%>
        <asp:Panel ID="pnlDetail" runat="server" Visible="false">
            <p class="breadcrumb"><a href="Default.aspx">Trang chủ</a> / <a href="News.aspx">Tin tức</a> / <span><%: Item.Title %></span></p>

            <div class="article-layout">
                <article class="article">
                    <h1><%: Item.Title %></h1>
                    <span class="news-date"><%: Item.CreatedAt.ToString("dd/MM/yyyy") %></span>
                    <img class="article-img" src="<%: Item.Image %>" alt="<%: Item.Title %>" />
                    <p class="article-lead"><%: Item.Summary %></p>
                    <asp:Literal ID="litContent" runat="server" />
                    <p class="article-back"><a href="News.aspx">← Quay lại tin tức</a></p>
                </article>

                <aside class="side-box">
                    <h2 class="side-title">Bài viết khác</h2>
                    <ul class="news-other">
                        <asp:Repeater ID="rptOther" runat="server">
                            <ItemTemplate>
                                <li>
                                    <a href='News.aspx?id=<%# Eval("Id") %>'>
                                        <img src='<%#: Eval("Image") %>' alt="" />
                                        <span><%#: Eval("Title") %></span>
                                    </a>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </aside>
            </div>
        </asp:Panel>

        <%-- ================= KHÔNG TÌM THẤY ================= --%>
        <asp:Panel ID="pnlNotFound" runat="server" CssClass="no-result" Visible="false">
            Không tìm thấy bài viết này. <a href="News.aspx">Quay lại tin tức</a>
        </asp:Panel>

    </div>

</asp:Content>
