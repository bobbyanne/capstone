using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpaceSidePizzaria.Custom
{
    public class SessionRoleFilter : ActionFilterAttribute
    {
        private readonly string _key = String.Empty;
        private readonly int[] _roles;

        public SessionRoleFilter(string key, params int[] roles)
        {
            _key = key;
            _roles = roles ?? new int[0];
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //TODO: Add try catch in this method. Also refactor

            HttpSessionStateBase session = filterContext.HttpContext.Session;

            if (session[_key] == null || !int.TryParse(session[_key].ToString(), out int role) || !_roles.Contains(role))
            {
                filterContext.Result = new RedirectResult("/Account/Login");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}