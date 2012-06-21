<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Answers.aspx.cs" Inherits="BingAnswers.Answers" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <script type="text/javascript"">
        jQuery.ajaxSetup({async:true});
        var query = "<%= HttpUtility.UrlEncode(HttpContext.Current.Request.Params["q"])%>";
        function Expand(index) {
            obj = $('#expandLink' + index);
            if (obj.text() == "More") {
                $('#answerDiv'+index).css('height', "100%");
                $('#expandLink' + index).html('Less<img style="vertical-align:bottom" width="15px" height="15px" src="/Images/arrow-up.png" />');
            }
            else
            { 
                $('#answerDiv'+index).css('height', "100px");
                $('#expandLink' + index).html('More<img style="vertical-align:bottom" width="15px" height="15px" src="/Images/arrow-down.png" />');
            }
            
        }
        function Like(obj,index,link) {
            jQuery.ajaxSetup({async:true});
            obj.innerText = 'You liked this';
            $.get("Logger.aspx",{q:query,scenario:"like",k:index,link:link});
            obj.src = "#";
        }
        function Share() {
        jQuery.ajaxSetup({async:true});
        $.get("Logger.aspx",{q:query,scenario:"share"});
        //jQuery.get("Logger.aspx?q="+query+"&scenario=share");
        }
        function Click(index,link) {
        jQuery.ajaxSetup({async:true});
        $.get("Logger.aspx",{q:query,scenario:"click",k:index,link:link});
        }
    </script>
    <img ID="logo-small" class="header" Width="200px" Height = "40px" src="/Images/bing-logo.png" />
    <asp:TextBox ID="Answer_SearchBox" class="header AnswerSearchBox" AutoPostBack="false" Width = "400px" Height = "20px"  runat="server" ToolTip="Enter your question" x-webkit-speech>            
    </asp:TextBox>    
    <asp:Button class="header AnswerButton" ID="Answer_SUBMIT" runat="server" Text="Go" Width="50px" />   
    <div class="header share">
        <p>
            <!--<a title="Share on Office Talk" onclick="Share()" href="http://officetalk/default.aspx?post_text=<%= HttpUtility.UrlEncode(Request.Url.ToString()) %>%20%23BingAnswers" target="_blank">share</a>-->
            <a title="Share on Facebook" onclick="javascript:window.open('https://www.facebook.com/sharer.php?u=<%=HttpUtility.UrlEncode(Request.Url.ToString())%>&t=Bing%20Web%20Answers','FbShare','status=yes')"><img alt="Share" src="Images/FbShare.jpg" /></a>
        </p>
    </div>
    <div class="header feedback">
        <p>
            <a style="color:Orange; cursor:pointer" onclick="$('#RatingOverlay').toggle('slow')" >Feedback</a>
        </p>
    </div>    
    <div style="position:absolute; margin-left:10%; margin-top:50px">
    <asp:Repeater runat="server" ID="answerRep">
        <ItemTemplate>
        <tr>
        <td>            
            <table width="800px">
            <tr>
                <td>
                <h3>
                    <asp:LinkButton runat="server" OnClick="Entity_Click" ID="AnswerClick" CommandArgument='<%# Container.ItemIndex %>'>
                    <!--<a onclick="Click(<%# Container.ItemIndex %>, '<%# HttpUtility.UrlEncode(DataBinder.Eval(Container.DataItem, "Url").ToString())%>')" href="<%# DataBinder.Eval(Container.DataItem, "Url") %>" target="_blank">-->
                    <%# DataBinder.Eval(Container.DataItem, "Question") %>
                    <!--</a>-->
                    </asp:LinkButton>
                </h3>
                </td>
            </tr>
            <tr>
                <td>                    
                    <div id="answerDiv<%# Container.ItemIndex %>" class="break-word answerDiv" "word- <%# DataBinder.Eval(Container.DataItem, "TopAnswer").ToString().Length > 450 ? "style=\"height:100px; overflow-y:hidden; overflow-x:\"" : ""%> ><%# GetAnswerTemplate(Container.DataItem, Container.ItemIndex) %></div>                    
                </td>
                <td align="right">
                    <img height="100px" width="150px" src="/Images/<%# DataBinder.Eval(Container.DataItem, "Source") %>.png" />
                </td>            
            </tr>
            
            <tr>
            <td style="text-align:left; color:Blue">
                <asp:LinkButton runat="server" OnClick="Entity_Like" ID="AnswerLikeClick" CommandArgument='<%# Container.ItemIndex %>'>                
                <iframe allowtransparency="true" height="20px" frameborder="0" scrolling="no" src='http://www.facebook.com/plugins/like.php?href=<%# HttpUtility.UrlEncode(DataBinder.Eval(Container.DataItem, "Url").ToString())%>&send=false&layout=button_count&action=like&show_faces=false&colorscheme=light'></iframe>
              <!--<a onclick="Like(this,<%# Container.ItemIndex %>, '<%# HttpUtility.UrlEncode(DataBinder.Eval(Container.DataItem, "Url").ToString())%>')">like</a> -->
              </asp:LinkButton>
              </td>                        
            <%# DataBinder.Eval(Container.DataItem, "TopAnswer").ToString().Length > 450 ? "<td style=\"text-align:left\"><a id=\"expandLink"+Container.ItemIndex+"\" onclick=\"Expand("+Container.ItemIndex+")\">More<img style=\"vertical-align:bottom\" width=\"15px\" height=\"15px\" src=\"/Images/arrow-down.png\" /></a></td>" : "" %> 
            </tr>                        
            </table>            
         </td>         
        </tr>
        </ItemTemplate>
    </asp:Repeater>
    </div>   
</asp:Content>

<asp:Content ID="FeedbackOverlayControls" ContentPlaceHolderID="FeedbackOverlayControls" runat="server">
    <asp:LinkButton ID="FeedbackSubmit" runat="server" Text="Submit" OnClick="RatingSubmit_Click">Submit</asp:LinkButton>    
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    <asp:LinkButton ID="FeedbackCancel" runat="server" Text="Cancel" OnClientClick="javascript:$('#RatingOverlay').toggle('slow'); return false">Cancel</asp:LinkButton>
</asp:Content>