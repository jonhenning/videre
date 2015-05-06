using System.IO;
using System.Net.Mail;
using System.Collections.Generic;

namespace Videre.Core.Services
{
    public class Mail
    {
        public static bool Send(string from, string recipients, string subject, string body, bool isBodyHtml = true, List<LinkedResource> linkedResources = null, List<Attachment> attachments = null)
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

            if (linkedResources != null && linkedResources.Count > 0)
            {
                msg.Body = null;
                var view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                foreach (var resource in linkedResources)
                    view.LinkedResources.Add(resource);
                msg.AlternateViews.Add(view);
            }

            if (attachments != null)
            {
                foreach (var a in attachments)
                    msg.Attachments.Add(a);
            }

            client.Send(msg);
            return true;
        }

        public static bool Send(string from, string recipients, string subject, string body, bool isBodyHtml = true, List<LinkedResource> linkedResources = null)
        {
            return Send(from, recipients, subject, body, isBodyHtml, linkedResources, null);
        }


        public static bool Send(string from, string recipients, string templateName, string subjectTemplateText, string bodyTemplateText, Dictionary<string, object> tokens, bool isBodyHtml = true, List<LinkedResource> linkedResources = null)
        {
            return Send(from, recipients, templateName, subjectTemplateText, bodyTemplateText, tokens, isBodyHtml, linkedResources, null);
        }

        public static bool Send(string from, string recipients, string templateName, string subjectTemplateText, string bodyTemplateText, Dictionary<string, object> tokens, bool isBodyHtml, List<LinkedResource> linkedResources, List<Attachment> attachments)
        {
            var expandedSubject = DynamicContent.ExpandTemplate("FastTemplate", templateName + "_subject", subjectTemplateText, tokens);
            var expandedBody = DynamicContent.ExpandTemplate("FastTemplate", templateName + "_body", bodyTemplateText, tokens);
            return Send(from, recipients, expandedSubject, expandedBody, isBodyHtml, linkedResources, attachments);
        }

        public static bool Send(string from, string recipients, string subjectTemplateFileName, string bodyTemplateFileName, Dictionary<string, object> tokens, bool isBodyHtml = true, List<LinkedResource> linkedResources = null)
        {
            return Send(from, recipients, subjectTemplateFileName, bodyTemplateFileName, tokens, isBodyHtml, linkedResources, null);
        }
        public static bool Send(string from, string recipients, string subjectTemplateFileName, string bodyTemplateFileName, Dictionary<string, object> tokens, bool isBodyHtml, List<LinkedResource> linkedResources, List<Attachment> attachments)
        {
            var expandedSubject = DynamicContent.ExpandTemplateFile("FastTemplate", subjectTemplateFileName + "_subject", subjectTemplateFileName, tokens);
            var expandedBody = DynamicContent.ExpandTemplateFile("FastTemplate", bodyTemplateFileName + "_body", bodyTemplateFileName, tokens);
            return Send(from, recipients, expandedSubject, expandedBody, isBodyHtml, linkedResources, attachments);
        }
    }
}
