using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Principal;
using Tipstaff.Models;
using TPLibrary.Logger;

namespace Tipstaff
{
    public enum AccessLevel
    {
        Denied = -1,
        Deactivated = 0,
        User = 25,
        Admin = 75,
        SystemAdmin = 100
    }

    public class CPrincipal : IPrincipal
    {
        private DateTime lastCheck;
        private User systemUser;

        public int UserID { get; private set; }
        private readonly TimeSpan refreshInterval = TimeSpan.FromMinutes(10);

        public IIdentity Identity { get; private set; }
        private TipstaffDB Db { get; }

        private readonly Guid instanceId = Guid.NewGuid();
        private static readonly ICloudWatchLogger logger = new CloudWatchLogger();

        // --- Constructors ---------------------------------------------------
        public CPrincipal(TipstaffDB repository)
        {
            Db = repository ?? throw new ArgumentNullException(nameof(repository));
            lastCheck = DateTime.MinValue;
        }

        public CPrincipal(IIdentity identity) : this(identity, new TipstaffDB())
        {
        }

        public CPrincipal(IIdentity identity, TipstaffDB repository)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            Db = repository ?? throw new ArgumentNullException(nameof(repository));
            lastCheck = DateTime.MinValue;
            LoadUserIfNeeded();
        }

        // --- Public Properties ---------------------------------------------------
        public int UserId
        {
            get
            {
                systemUser = LoadUserIfNeeded();
                return systemUser?.UserID ?? 0;
            }
        }

        public User User
        {
            get
            {
                systemUser = LoadUserIfNeeded();
                return systemUser;
            }
        }

        public AccessLevel AccessLevel
        {
            get
            {
                systemUser = LoadUserIfNeeded();
                return (AccessLevel)(systemUser?.Role.strength ?? 0);
            }
        }

        public string DisplayName
        {
            get
            {
                systemUser = LoadUserIfNeeded();
                return systemUser?.DisplayName ?? string.Empty;
            }
        }

        public bool IsInRole(string role)
        {
            return (this.User.Role.Detail == role);
        }

        // --- Internal refresh logic ---------------------------------------------

        private User LoadUserIfNeeded()
        {
            User _user = null;
            var now = DateTime.Now;
            var shouldReload =
                systemUser == null ||
                (now - lastCheck) > refreshInterval;

            if (!shouldReload) {
                return systemUser;
            }

            string username = Identity?.Name?.Split('\\').Last();

            if (string.IsNullOrWhiteSpace(username))
                return null;


            try {
                _user = Db.GetUserByLoginName(username);
            } catch (Exception ex) {
                logger.LogError(ex, $"Failed to load user {username}");
                return null;
            }

            lastCheck = now;
            return _user;
        }

        private static void Log(string message)
        {
            logger.LogInfo($"{message}");
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRedirect : AuthorizeAttribute
    {
        private bool _isAuthorized = false;
        private AccessLevel UserAccessLevel;
        public string RedirectPrivateUrl = "~/Private";
        public string RedirectUnAuthUrl = "~/Error/Unauthorised";
        public string RedirectWrongTeam = "~/Error/WrongTeam";
        public AccessLevel MinimumRequiredAccessLevel { get; set; }
        private static readonly ICloudWatchLogger logger = new CloudWatchLogger();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (!_isAuthorized && filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated && UserAccessLevel == AccessLevel.Denied)
            {
                filterContext.RequestContext.HttpContext.Response.Redirect(RedirectPrivateUrl);
            }
            else if (!_isAuthorized && filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated && (UserAccessLevel < MinimumRequiredAccessLevel))
            {
                filterContext.RequestContext.HttpContext.Response.Redirect(RedirectUnAuthUrl);
            }
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var identity = httpContext.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                return false;
            }


            using (TipstaffDB db = new TipstaffDB())
            {
                var userAccessLevel =
                    (AccessLevel)db.UserAccessLevel(httpContext.User);

                _isAuthorized = userAccessLevel >= MinimumRequiredAccessLevel;
            }
            return _isAuthorized;
        }

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AllowAnonymousAttribute : Attribute { }

    public sealed class LogonAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
            || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

                base.OnAuthorization(filterContext);
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            try
            {
                IIdentity user = httpContext.User.Identity;
                CPrincipal cPrincipal = new CPrincipal(user);
                httpContext.User = cPrincipal;
                return true; // always true as anonymous allowed
            }
            catch
            {
                return false;
            }
        }
    }
}