using System;
using System.Activities;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace MeetupWfIntro.MeetupActivityLibrary.MeetupAPI
{
    /// <summary>
    /// Custom Activity that retrieves the Member Names and Count for a specified Meetup.Com Group
    /// </summary>
    public class GetGroupMembers : CodeActivity
    {
        #region Arguments

        /// <summary>
        /// Meetup.Com API Key
        /// </summary>
        [RequiredArgument]
        public InArgument<string> ApiKey { get; set; }
        
        /// <summary>
        /// URL Name of the Meetup Group
        /// </summary>
        [RequiredArgument]
        public InArgument<string> GroupUrlName { get; set; }

        /// <summary>
        /// Total Number of Members of the specified Meetup Group
        /// </summary>
        public OutArgument<int> MembersCount { get; set; }
        
        /// <summary>
        /// Member Names of the specified Meetup Group
        /// </summary>
        public OutArgument<Collection<string>> MembersNames { get; set; }

        /// <summary>
        /// Raw response from the Meetup REST API
        /// </summary>
        public OutArgument<string> RawResponse { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public GetGroupMembers():base()
        {
            GroupUrlName = "Timisoara-NET-Meetup";
        }

        /// <summary>
        /// Execution Logic
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            MembersCount.Set(context,0);
            MembersNames.Set(context, new Collection<string>() { });

            string host = "https://api.meetup.com/2/members?order=name&sign=true&group_urlname={0}&key={1}";
            string url = string.Format(host, GroupUrlName.Get(context), ApiKey.Get(context));
            string response = string.Empty;

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            if (null == request)
            {
                throw new ApplicationException("HttpWebRequest failed");
            }
            using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                response = sr.ReadToEnd();
            }

            RawResponse.Set(context, response);

            Collection<string> names = new Collection<string>() { };

            JObject o = JObject.Parse(response);
            JArray a = (JArray)o["results"];

            foreach(JToken item in a)
            {
                names.Add(item["name"].ToString());
            }

            MembersCount.Set(context, a.Count);
            MembersNames.Set(context, names);
        }
    }
}
