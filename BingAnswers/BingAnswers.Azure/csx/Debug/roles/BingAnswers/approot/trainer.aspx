<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="trainer.aspx.cs" Inherits="BingAnswers.trainer" %>
<asp:Repeater runat="server" ID="trainerRep">
<ItemTemplate>
<%# DataBinder.Eval(Container.DataItem, "Question") %>$$$$$<%# DataBinder.Eval(Container.DataItem, "Url") %>$$$$$<%# DataBinder.Eval(Container.DataItem, "Source")%>$$$$$<%# DataBinder.Eval(Container.DataItem, "Features.featureString")%>$$$$$<%# DataBinder.Eval(Container.DataItem, "Score")%>$$$$$<%# DataBinder.Eval(Container.DataItem, "Type")%><br />
</ItemTemplate>
</asp:Repeater>