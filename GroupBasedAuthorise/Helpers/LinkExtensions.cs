using GroupBasedAuthorise.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Security;

namespace GroupBasedAuthorise.Helpers
{
    public static class LinkExtensions
    {
        public static MvcHtmlString ActionLinkAllowedForPermission(this HtmlHelper htmlHelper,
            string linkText,
            string action,
            string controller,
            string permissionName, 
            object routeAttributes = null, 
            object htmlAttributes = null)
        {
            if (EntityManager.HasCurrentUserPermission(permissionName))
                return htmlHelper.ActionLink(linkText, action, controller, routeAttributes, htmlAttributes);
            return MvcHtmlString.Empty;
        }

        public static MvcHtmlString IconLinkAllowedForPermission(this HtmlHelper htmlHelper, 
            string action, 
            string controller,
            string permission,
            object routeAttributes,
            string htmlClasses)
        {
            if (!EntityManager.HasCurrentUserPermission(permission))
                return MvcHtmlString.Empty;

            var iconLink = new TagBuilder("a");

            var href = new UrlHelper(htmlHelper.ViewContext.RequestContext).Action(action, controller, routeAttributes);

            iconLink.Attributes["href"] = href;
            iconLink.AddCssClass(htmlClasses);

            return MvcHtmlString.Create(iconLink.ToString());
        }
    }
}