//----------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Controllers
{
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OpenIdConnect;

    /// <summary>
    /// Account Controller
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Sign In
        /// </summary>
        public void SignIn()
        {
            // Send an OpenID Connect sign-in request.
            if (!this.Request.IsAuthenticated)
            {
                this.HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        /// <summary>
        /// Sign Out
        /// </summary>
        public void SignOut()
        {
            string callbackUrl = this.Url.Action("SignOutCallback", "Account", routeValues: null, protocol: this.Request.Url.Scheme);

            this.HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
        }

        /// <summary>
        /// Sign Out Callback
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult SignOutCallback()
        {
            if (this.Request.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return this.RedirectToAction("Index", "Stickers");
            }

            return this.View();
        }

        /// <summary>
        /// Invalid User
        /// </summary>
        public void InvalidUser()
        {
            string callbackUrl = this.Url.Action("InvalidUserCallback", "Account", routeValues: null, protocol: this.Request.Url.Scheme);

            this.HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
        }

        /// <summary>
        /// Invalid User Callback
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult InvalidUserCallback()
        {
            return this.View();
        }
    }
}
