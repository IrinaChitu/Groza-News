using GrozaNews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrozaNews.Controllers
{
    public class CategoryController : Controller
    {
        // GET: Category
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Show()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(News news)
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }

        [HttpPut]
        public ActionResult Edit(News news)
        {
            return View();
        }

        [HttpDelete]
        public ActionResult Delete()
        {
            return View();
        }
    }
}