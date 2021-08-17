﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using net5backendwithidentityandADandGroups.Models;
using Microsoft.AspNetCore.Http;
using net5backendwithidentityandADandGroups.Services;

namespace net5backendwithidentityandADandGroups.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            ViewData["User"] = HttpContext.User;

            // Calls method GetSessionGroupList to get groups from session.
            var groups = GraphHelper.GetUserGroupsFromSession(HttpContext.Session);
            if (groups?.Count > 0)
            {
                ViewData.Add("groupClaims", groups);
            }
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}