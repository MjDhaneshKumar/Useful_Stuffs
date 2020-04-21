using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace Umbraco_External_Auth.ViewModel
{
    public class FacebookViewModel : ContentModel
    {
        public FacebookViewModel(IPublishedContent content) : base(content)
        {
        }
       public FaceBookResponseModel FaceBookResponseModel { get; set; }
    }
    public class FaceBookResponseModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        public string Email { get; set; }
        public object Picture { get; set; }
        public string Gender { get; set; }
        public bool Verified
        {
            get;
            set;
        }
    }
}