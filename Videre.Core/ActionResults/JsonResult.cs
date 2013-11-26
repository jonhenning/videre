using System.Collections.Generic;
using System.Web.Mvc;
using System;
using CodeEndeavors.Extensions;
using CodeEndeavors.Extensions.Serialization;
using Newtonsoft.Json.Converters;

namespace Videre.Core.ActionResults
{
    public class JsonResult<T> : JsonNetResult where T : new()
    {
        public JsonResult()
        {
            Data = new T();
            Context = new Dictionary<string, object>();
            Messages = new List<Models.Message>();
            IgnoreType = "client";
        }

        private List<Models.Message> Messages { get; set; }
        //private string _errorMessage;
        public new T Data { get; set; }
        public Dictionary<string, object> Context { get; set; }
        public bool HasError { get; set; }
        public bool Compressed { get; set; }
        public string IgnoreType { get; set; }
        public bool PreserveObjectReferences { get; set; }

        [SerializeIgnore(new string[] { "db", "client" })]
        public bool PrettyJson 
        {
            get
            {
                return base.Formatting == Newtonsoft.Json.Formatting.Indented;
            }
            set
            {
                base.Formatting = value ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
            }
        }


        public void AddError(string Text)
        {
            HasError = true;
            Messages.Add(new Models.Message(Text, Text, true));
        }

        public void AddError(Exception Ex)
        {
            HasError = true;
            var s = Ex.Message;
            if (Ex.InnerException != null)
                s += "\r\n" + Ex.InnerException.Message;
            Messages.Add(new Models.Message(s, s, true));
        }

        public void AddErrors(IEnumerable<string> Errors)
        {
            HasError = true;
            foreach (var sErr in Errors)
                Messages.Add(new Models.Message(sErr, sErr, true));
        }

        public void AddMessage(string Text)
        {
            AddMessage(Text, Text);
        }
        public void AddMessage(string Id, string Text)
        {
            Messages.Add(new Models.Message(Id, Text, false));
        }

        public override void ExecuteResult(ControllerContext context)
        {
            //get around using the MS serializer and newtonsoft one for having two ways to serialize (jsonIgnore and scriptignore)
            //by introducing the new SerializeIgnore attribute (from CodeEndeavors), which allows a string to be passed in on what type to 
            //ignore.  So when we send to client we can ignore properties marked "client" and when we persist to database we can ignore properties marked "db"
            //base.Formatting = Newtonsoft.Json.Formatting.None;
            base.SerializerSettings.ContractResolver = new CodeEndeavors.Extensions.Serialization.SerializeIgnoreContractResolver(IgnoreType);    //exclude properties marked client ignore
            base.SerializerSettings.Converters.Add(new IsoDateTimeConverter() { DateTimeStyles = System.Globalization.DateTimeStyles.AdjustToUniversal });

            //http://james.newtonking.com/projects/json/help/index.html?topic=html/PreserveObjectReferences.htm
            if (PreserveObjectReferences)
                base.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;

            base.Data = new { HasError = this.HasError, Messages = this.Messages, Data = this.Data };
            base.ExecuteResult(context);            
        }

    }
}