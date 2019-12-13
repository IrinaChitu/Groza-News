using GrozaNews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrozaNews.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class NewsController : Controller
    {
        // GET: News
        [Authorize(Roles = "User,Editor,Administrator")] // de vazut cum faci sa vada orcine, nu doar daca esti deja logat
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(string Id)
        {
            return View();
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New()
        {
            return View();
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New(News news)
        {
            return View();
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(string Id)
        {
            return View();
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(News news)
        {
            return View();
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(string Id)
        {
            return View();
        }
    }
}