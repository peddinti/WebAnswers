<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="BingAnswers._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <img ID="logo" class="centered" Width="500px" Height = "100px" src="/Images/bing-logo.png" />    

    <asp:TextBox class="centered SearchBox" ID="SearchBox" Height="25px" Width="500px" runat="server" ToolTip="Enter your question" placeholder="eg. What is the meaning of life" x-webkit-speech>
    </asp:TextBox>    
    
    <asp:Button Width="50px" class="Button centered" ID="SUBMIT" runat="server" Text="Go" />
    <div class="footer"><p style="text-align:center">Created by VMK</p></div>

    <!--
    <div style="overflow:auto">
    <asp:Repeater runat="server" ID="answerRep">
        <ItemTemplate>
        <tr>
        <td>            
            <table width="600px" style="border:2px solid black">
            <tr>
                <td>
                <h3>
                    <a href="<%# DataBinder.Eval(Container.DataItem, "Url") %>">
                    <%# DataBinder.Eval(Container.DataItem, "Question") %>
                    </a>
                </h3>
                </td>
            </tr>
            <tr>
                <td><%# DataBinder.Eval(Container.DataItem, "TopAnswer") %></td>                
            </tr>
            <tr>            
                <td>Source=<%# DataBinder.Eval(Container.DataItem, "Source") %></td>            
            </tr>            
            <tr>            
                <td>Score=<%# DataBinder.Eval(Container.DataItem, "Score") %></td>            
            </tr>
            </table>            
         </td>         
        </tr>
        </ItemTemplate>
    </asp:Repeater>
</div>
-->
</asp:Content>
