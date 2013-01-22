using System;
using System.Linq;
using System.IO;
using CodeEndeavors.Extensions;
using PaniciSoftware.FastTemplate;
using PaniciSoftware.FastTemplate.Common;
using System.Net.Mail;

namespace Videre.Core.Services
{
    public class Mail
    {
        public static bool Send(string from, string recipients, string subject, string body, bool isBodyHtml = true)
        {
            var client = new SmtpClient();  //read from configuration

            if (!string.IsNullOrEmpty(client.PickupDirectoryLocation) && client.PickupDirectoryLocation.StartsWith("~/"))
            {
                client.PickupDirectoryLocation = Portal.ResolvePath(client.PickupDirectoryLocation);
                if (!Directory.Exists(client.PickupDirectoryLocation))
                    Directory.CreateDirectory(client.PickupDirectoryLocation);
            }
            var msg = new MailMessage()
            {
                From = new MailAddress(from),
                Subject = subject,
                IsBodyHtml = isBodyHtml,
                Body = body
            };
            msg.To.Add(recipients);

            client.Send(msg);
            return true;
        }

        public static bool Send(string from, string recipients, string templateName, string subjectTemplateText, string bodyTemplateText, TemplateDictionary tokens, bool isBodyHtml = true)
        {
            return Send(from, recipients, ParseTemplate(templateName + "_subject", subjectTemplateText, tokens), ParseTemplate(templateName + "_subject", bodyTemplateText, tokens), isBodyHtml);
        }

        public static bool Send(string from, string recipients, string subjectTemplateFileName, string bodyTemplateFileName, TemplateDictionary tokens, bool isBodyHtml = true)
        {
            return Send(from, recipients, ParseTemplate(subjectTemplateFileName, tokens), ParseTemplate(bodyTemplateFileName, tokens), isBodyHtml);
        }

        //todo:  move out of Mail namespace?  if we had Template namespace may confuse with PageTemplate and LayoutTemplate
        public static string ParseTemplate(string name, string templateText, TemplateDictionary tokens)
        {
            var result = Template.CompileAndRun(name, templateText, tokens);
            if (result.Errors.Count > 0)
                throw new Exception("ParseTemplate error: " + result.Errors.ToJson());
                return result.Output;
        }

        public static string ParseTemplate(string templateFileName, TemplateDictionary tokens)
        {
            var fileName = Portal.ResolvePath(templateFileName);
            if (!System.IO.File.Exists(fileName))
                throw new Exception("ParseTemplate - File Not Found: " + fileName);
            return ParseTemplate(fileName, fileName.GetFileContents(), tokens);
        }

    }
}
