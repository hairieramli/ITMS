using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ITMS.Helpers
{
    public static class MenuItem
    {
        public static string setClass(this HtmlHelper helper, string action)
        {
            string classList = "nav-item";
            string currentAction = helper.ViewContext.RouteData.Values["controller"].ToString();

            if(currentAction.Equals(action, StringComparison.CurrentCultureIgnoreCase))
            {
                classList += " active";
            }
            return classList;
        }

    }
}