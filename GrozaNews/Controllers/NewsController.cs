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
        private ApplicationDbContext db = new ApplicationDbContext();
        private int _perPage = 5;

        // GET: News
        [Authorize(Roles = "User,Editor,Administrator")] // de vazut cum faci sa vada orcine, nu doar daca esti deja logat
        public ActionResult Index()
        {
            var news = db.News.Include("Comment").Include("Category").Include("User").OrderBy(a => a.Date);
            var totalItems = news.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedNews= news.Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.perPage = this._perPage;
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Articles = paginatedNews;

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