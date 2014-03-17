using CodeEndeavors.Extensions.Serialization;
using System;

namespace Videre.Core.Models
{
    public enum LocalizationType
    {
        Portal,
        Widget,
        WidgetEditor,
        ClientControl,
        WidgetContent,
        Exception
    }

    public class Localization 
    {
        public string Id { get; set; }      //Resource.Id
        public string Key { get; set; }     //Resource.Key
        public string Text { get; set; }    //Resource.Data

        [SerializeIgnore(new string[] { "db" })]
        public string DisplayText
        {
            get
            {
                return Services.TokenParser.ReplaceTokensWithContent(Text, false);
            }
            set
            {
                Text = Services.TokenParser.ReplaceContentWithTokens(value);
            }
        }

        [SerializeIgnore(new string[] { "db" })]
        public string EditText
        {
            get
            {
                return Services.TokenParser.ReplaceTokensWithContent(Text, true);
            }
            set
            {
                Text = Services.TokenParser.ReplaceContentWithTokens(value);
            }
        }

        public LocalizationType Type { get; set; }  

        public string PortalId { get; set; }       //Scope[PortalId]
        public string Locale { get; set; }      //Scope[Locale]
        public string Namespace { get; set; }   //Scope[Namespace]

        //public DateTime? EffectiveDate { get; set; }
        //public DateTime? ExpirationDate { get; set; }

    }

}
