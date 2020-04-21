using Facebook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco_External_Auth.ViewModel;

namespace Umbraco_External_Auth.Controller
{
    //redirect url need to match
    //reference : https://our.umbraco.com/forum/developers/api-questions/23530-Facebook-and-Umbraco-membership-provider-integration
    public class FacebookLoginController:RenderMvcController
    {
        private Uri RedirectedUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = uriBuilder.Path; //Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }
        [HttpGet]
        public override ActionResult Index(ContentModel model) {
            var code = Request?.QueryString["code"]?.ToString();
            if (!string.IsNullOrEmpty(code))
            {
                return FacebookCallback(code);
            }

           return base.Index(new FacebookViewModel(CurrentPage));
            
        }

        [HttpPost]
        public  ActionResult Index()
        {
            var code = Request?.QueryString["code"]?.ToString();
            if (string.IsNullOrEmpty(code))
            {
                return Facebook();
            }
            else
            {
                return FacebookCallback(code);
            }

            
        }
       
        public ActionResult Facebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                // client_id = "118812552167300",
                //client_secret = "6022616e1b176fe221dd20d591173444",
                client_id = "937102470058147",
                client_secret = "be604e66a5b81ad14e010c823be80038",
                redirect_uri = RedirectedUri.AbsoluteUri,
                response_type = "code",
                scope = "email"
            });

            return Redirect(loginUrl.AbsoluteUri);
        }

        public ActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = "937102470058147",
                client_secret = "be604e66a5b81ad14e010c823be80038",
                redirect_uri = RedirectedUri.AbsoluteUri,
                code = code
            });

            var accessToken = result.access_token;
            Session["AccessToken"] = accessToken;
            fb.AccessToken = accessToken;
            dynamic me = fb.Get("me?fields=link,first_name,currency,last_name,email,gender,locale,timezone,verified,picture,age_range");
            string email = me.email;
            string lastname = me.last_name;
            string picture = me.picture.data.url;
           
            var json = JsonConvert.SerializeObject(me);
            var jsonDes = JsonConvert.DeserializeObject<FaceBookResponseModel>(json);
            var model = new FacebookViewModel(CurrentPage);
            model.FaceBookResponseModel = jsonDes;
            var fbResponseModel = model.FaceBookResponseModel;
            IMemberService memberService = Services.MemberService;
            if (Umbraco.MembershipHelper.GetByEmail(fbResponseModel.Email)!=null)
            {


              
                //umbraco member handling
              
                IMember newMember = memberService.CreateMemberWithIdentity(fbResponseModel.Email, fbResponseModel.Email, $"{fbResponseModel.FirstName} {fbResponseModel.LastName}", "Member");
                newMember.Id = Convert.ToInt32(fbResponseModel.Id);
                memberService.AssignRole(newMember.Id, "jango");



                memberService.Save(newMember,raiseEvents:true);
                //this will logged in
                FormsAuthentication.SetAuthCookie(newMember.Username, false);
            }

            return View("/Views/FacebookLogin.cshtml", model);
        }
    }
}