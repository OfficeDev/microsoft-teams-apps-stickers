//----------------------------------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration
{
    using System.Web.Optimization;

    /// <summary>
    /// Bundle Config class
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// Register Bundles
        /// For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        /// </summary>
        /// <param name="bundles">Bundles</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/lib/bootstrap/dist/js/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/lib/bootstrap/dist/css/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
