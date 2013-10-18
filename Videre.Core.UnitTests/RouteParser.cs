using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CoreServices = Videre.Core.Services;

namespace Videre.Core.UnitTests
{
    [TestClass]
    public class RouteParser
    {
        [TestMethod]
        public void BasicBestMatch()
        {
            var urls = new List<string>()
            {
                "contact",
                "contact/detail",
                "account/edit/{id:int}"
            };

            Assert.IsTrue(CoreServices.RouteParser.GetBestMatchedUrl("contact", urls) == "contact");
            Assert.IsTrue(CoreServices.RouteParser.GetBestMatchedUrl("contact/someunknownportion/foobar", urls) == "contact");
            Assert.IsNull(CoreServices.RouteParser.GetBestMatchedUrl("account/edit/a", urls));
            Assert.IsTrue(CoreServices.RouteParser.GetBestMatchedUrl("account/edit/1", urls) == "account/edit/{id:int}");
        }

        [TestMethod]
        public void MatchedGroups()
        {
            var urls = new List<string>()
            {
                "contact",
                "contact/detail",
                "account/edit",
                "account/edit/{id:int}"
            };

            var url = "account/edit/1";
            var bestMatch = CoreServices.RouteParser.GetBestMatchedUrl(url, urls);
            var groups = CoreServices.RouteParser.Parse(bestMatch, url);
            Assert.IsTrue(groups.Count == 2);
            Assert.IsTrue(groups["0"] == url);
            Assert.IsTrue(groups["id"] == "1");
        }

    }
}
