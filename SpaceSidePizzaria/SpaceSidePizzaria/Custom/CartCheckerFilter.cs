using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpaceSidePizzaria.Models;

namespace SpaceSidePizzaria.Custom
{
    public class CartCheckerFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["Cart"] == null)
            {
                filterContext.HttpContext.Session["Cart"] = new List<PizzaPO>();
            }

            base.OnActionExecuting(filterContext);
        }
    }
}