using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;

namespace Videre.Core.Models
{
    public class SearchDocument //: CoreModels.ISearchResult
    {
        private Document _document = null;

        public SearchDocument()
        {
        }

        public SearchDocument(string id, string type, string name, string summary, Dictionary<string, string> analyzedValues)
        {
            _document = new Document();
            _document.Add(new Field("id", id, Field.Store.YES, Field.Index.NOT_ANALYZED));
            _document.Add(new Field("type", type, Field.Store.YES, Field.Index.ANALYZED));
            _document.Add(new Field("_name", name, Field.Store.YES, Field.Index.NOT_ANALYZED));
            _document.Add(new Field("_summary", summary, Field.Store.YES, Field.Index.NOT_ANALYZED));
            var sb = new System.Text.StringBuilder();
            foreach (var key in analyzedValues.Keys)
            {
                sb.Append(analyzedValues[key] + " ");
                _document.Add(new Field(key, analyzedValues[key], Field.Store.YES, Field.Index.ANALYZED));
            }
            _document.Add(new Field("text", sb.ToString(), Field.Store.YES, Field.Index.ANALYZED));
        }

        public SearchDocument(Document document)
        {
            this._document = document;
        }

        public Document Document
        {
            get { return _document; }
        }


        public string Id
        {
            get
            {
                return _document.GetField("id").StringValue();
            }
            set
            {
                _document.GetField("id").SetValue(value);
            }
        }

        public string Type
        {
            get
            {
                return _document.GetField("type").StringValue();
            }
            set
            {
                _document.GetField("type").SetValue(value);
            }
        }

        public string Name
        {
            get
            {
                return _document.GetField("_name").StringValue();
            }
            set
            {
                _document.GetField("_name").SetValue(value);
            }
        }

        public string Summary
        {
            get
            {
                return _document.GetField("_summary").StringValue();
            }
            set
            {
                _document.GetField("_summary").SetValue(value);
            }
        }

        public string Url {get;set;}

        public string GetField(string name, string defaultValue)
        {
            var field = _document.GetField(name);
            if (field != null)
                return field.StringValue();
            return defaultValue;
        }

    }
}
