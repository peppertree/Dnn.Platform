<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Authenticate.ascx.cs" Inherits="DotNetNuke.Modules.Authentication.AuthenticationForm" %>



<asp:Label ID="lblIdentifier" runat="server"></asp:Label>
<asp:TextBox ID="txtIdentifier" runat="server"></asp:TextBox>

<asp:Label ID="lblPassword" runat="server"></asp:Label>
<asp:TextBox ID="txtPassword" runat="server"></asp:TextBox>

<ul class="dnnActions">
    <li id="liLogin" runat="server">
        <asp:LinkButton ID="cmdLogin" runat="server" CssClass="dnnPrimaryAction"></asp:LinkButton>
    </li>
    <li id="liCancel" runat="server">
        <asp:LinkButton ID="cmdCancel" runat="server" CssClass="dnnSecondaryAction"></asp:LinkButton>
    </li>
    <li id="liForgotPassword" runat="server">
        <asp:LinkButton ID="cmdForgotPassword" runat="server" CssClass="dnnSecondaryAction"></asp:LinkButton>
    </li>
</ul>