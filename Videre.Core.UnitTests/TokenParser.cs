using System;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CoreServices = Videre.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using Microsoft.QualityTools.Testing.Fakes;

namespace Videre.Core.UnitTests
{
    [TestClass]
    public class TokenParser
    {
        private static string _baseUrl = "http://www.codeendeavors.com/";
        [TestMethod]
        public void BasicUrl()
        {
            
            using (ShimsContext.Create())
            {
                System.Web.Fakes.ShimVirtualPathUtility.ToAbsoluteString = (string s) => 
                {
                    if (s.StartsWith("~/"))
                        return s.Replace("~/", _baseUrl);
                    return s; 
                };

                var text = "This is an image <img src=\"http://www.codeendeavors.com/pic.jpg\" />";
                var content = CoreServices.TokenParser.ReplaceContentWithTokens(text);
                Assert.IsTrue(content == text.Replace(_baseUrl, CoreServices.TokenParser.GetRule("BASEURL").TokenText));
                Assert.IsTrue(text == CoreServices.TokenParser.ReplaceTokensWithContent(content));

            }
        }


    }
}
