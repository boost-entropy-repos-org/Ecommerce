using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using MrCMS.Web.Apps.NewsletterBuilder.Entities;
using MrCMS.Web.Apps.NewsletterBuilder.Entities.ContentItems;
using MrCMS.Web.Apps.NewsletterBuilder.Services.Parsing;
using MrCMS.Website;

namespace MrCMS.Web.Apps.NewsletterBuilder.ActionResults
{
    public class NewsletterActionResult : ActionResult
    {
        private static readonly Regex BodyRegex = new Regex(@"\[BodyContent\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly MethodInfo ParseMethod = typeof(NewsletterActionResult)
            .GetMethod("ParseItem", BindingFlags.NonPublic | BindingFlags.Static);

        private readonly Newsletter _newsletter;
        private readonly NewsletterTemplate _template;

        public NewsletterActionResult(Newsletter newsletter, NewsletterTemplate templateOverride = null)
        {
            _newsletter = newsletter;
            _template = templateOverride ?? newsletter.NewsletterTemplate;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Write(ParseTemplate());
        }

        private string ParseTemplate()
        {
            string bodyContent = string.Empty;

            IList<ContentItem> contentItems = _newsletter.ContentItems.OrderBy(x => x.DisplayOrder).ToList();
            if (contentItems.Any())
            {
                bodyContent += ParseItem(contentItems.First());

                foreach (ContentItem item in contentItems.Skip(1))
                {
                    bodyContent += _template.Divider;
                    bodyContent += ParseItem(item);
                }
            }

            //Find image src with out HTTP
            var linksDealtWith = new List<string>();
            var url = "http://" + CurrentRequestData.CurrentSite.BaseUrl;
            foreach (Match m in Regex.Matches(bodyContent, @"<\s*?img\s+[^>]*?\s*src\s*=\s*([""'])((\\?.)*?)\1[^>]*?>"))
            {
                var link = m.Groups[2].Value;
                if (!link.ToLower().Contains("http") && !linksDealtWith.Contains(link) && !string.IsNullOrWhiteSpace(link))
                {
                    bodyContent = bodyContent.Replace(link, link.Insert(0, url.TrimEnd('/')));
                    linksDealtWith.Add(link);
                }
            }

            return BodyRegex.Replace(_template.BaseTemplate ?? string.Empty, bodyContent);
        }

        private string ParseItem(ContentItem item)
        {
            return (string)ParseMethod.MakeGenericMethod(item.GetType()).Invoke(this, new object[] { _template, item });
        }

        // ReSharper disable UnusedMember.Local - Called via reflection
        private static string ParseItem<T>(NewsletterTemplate template, T item)
        {
            try
            {
                var parser = MrCMSApplication.Get<INewsletterItemParser<T>>();
                return parser != null ? parser.Parse(template, item) : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // ReSharper restore UnusedMember.Local
    }
}