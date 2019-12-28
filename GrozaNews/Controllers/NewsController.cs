using GrozaNews.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GrozaNews.Controllers
{
    // [Authorize(Roles = "Administrator")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int _perPage = 5;

        // GET: News
        // [Authorize(Roles = "User,Editor,Administrator")] // de vazut cum faci sa vada orcine, nu doar daca esti deja logat
        public ActionResult Index()
        {
            //de verificat cum arata impartirea pe pagini (ulterior si cu stilizare din view-uri, care momentan sunt temporare si de vazut si cum e cu partial views)
            var news = db.News.Include("Comments").Include("Category").Include("User").OrderByDescending(a => a.Date);
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
            ViewBag.News = paginatedNews;

            return View();
        }

        public ActionResult IndexProposedNews()
        {
            var news = db.ProposedNews.Include("Category").Include("User").OrderByDescending(a => a.Date);
            var totalItems = news.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedNews = news.Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.perPage = this._perPage;
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.News = paginatedNews;

            return View();
        }

        // [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int id)
        {
            News news= db.News.Find(id);

            ViewBag.afisareButoane = false;
            if (User.IsInRole("Editor") || User.IsInRole("Administrator"))
            {
                ViewBag.afisareButoane = true;
            }

            ViewBag.esteAdmin = User.IsInRole("Administrator");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();

            return View(news);
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New()
        {
            News news = new News();

            // preluam lista de categorii din metoda GetAllCategories()
            ViewBag.Categories = GetAllCategories();

            // Preluam ID-ul utilizatorului curent
            news.UserId = User.Identity.GetUserId();


            return View(news);
        }

        [Authorize(Roles = "User, Editor, Administrator")]
        public ActionResult ProposeNews()
        {
            ProposedNews news = new ProposedNews();

            // preluam lista de categorii din metoda GetAllCategories()
            ViewBag.Categories = GetAllCategories();

            // Preluam ID-ul utilizatorului curent
            news.UserId = User.Identity.GetUserId();

            return View(news);
        }

        [Authorize(Roles = "Editor,Administrator")]
        [ValidateInput(false)] //dafuq?
        [HttpPost]
        public ActionResult New(News news)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Protect content from XSS --> wtf treb citit in lab ce e
                    // news.Content = Sanitizer.GetSafeHtmlFragment(article.Content);
                    db.News.Add(news);
                    db.SaveChanges();
                    TempData["message"] = "Articolul a fost adaugat!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(news);
                }
            }
            catch (Exception e)
            {
                return View(news);
            }
        }

        [Authorize(Roles = "User, Editor,Administrator")]
        [ValidateInput(true)] //dafuq?
        [HttpPost]
        public ActionResult ProposeNews(ProposedNews news)
        {
            try
            {
                if (ModelState.IsValid && news.CategoryId != null)
                {
                    // Protect content from XSS --> wtf treb citit in lab ce e
                    // news.Content = Sanitizer.GetSafeHtmlFragment(article.Content);
                    db.ProposedNews.Add(news);
                    db.SaveChanges();
                    TempData["message"] = "Stirea a fost propusa cu succes!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(news);
                }
            }
            catch (Exception e)
            {
                return View(news);
            }
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int id)
        {
            News news = db.News.Find(id);
            ViewBag.Categories = GetAllCategories();

            if (news.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                return View(news);
            }
            else
            {
                // recomand ca nici macar sa nu apara butonul de edit daca articolul nu e al tau // in view un check
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Editor,Administrator")]
        [ValidateInput(false)]
        [HttpPut]
        public ActionResult Edit(int id, News requestNews)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    News news = db.News.Find(id);
                    if (news.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(news))
                        {
                            news.Title = requestNews.Title;
                            // Protect content from XSS
                            // requestNews.Content = Sanitizer.GetSafeHtmlFragment(requestNews.Content);
                            // news.Content = requestNews.Content;
                            // news.Date = requestNews.Date;
                            // news.CategoryId = requestArticle.CategoryId;
                            news = requestNews;
                            db.SaveChanges();
                            TempData["message"] = "Articolul a fost modificat!";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine!";
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    return View(requestNews);
                }

            }
            catch (Exception e)
            {
                return View(requestNews);
            }
        }

        [Authorize(Roles = "Editor,Administrator")]
        [ValidateInput(false)]
        [HttpPut]
        public ActionResult SubmitProposedNews(int id, string submitButton)
        {
            // Am putea face aici un redirect catre formularul de News/New cu datele completate deja si acolo sa o adauge ca stire noua :-??
            // Ramane de discutat
            try
            {
                if (ModelState.IsValid)
                {
                    ProposedNews proposedNews = db.ProposedNews.Find(id);
                    if (submitButton == "Reject")
                    {
                        db.ProposedNews.Remove(proposedNews);
                        db.SaveChanges();
                        TempData["message"] = "Stirea a fost stearsa cu succes!";
                    }
                    else
                    {
                        News news = new News();
                        if (TryUpdateModel(proposedNews))
                        {
                            news.Title = proposedNews.Title;
                            //// Protect content from XSS
                            //// requestNews.Content = Sanitizer.GetSafeHtmlFragment(requestNews.Content);
                            news.Content = proposedNews.Content;
                            news.Date = proposedNews.Date;
                            news.CategoryId = proposedNews.CategoryId ?? default(int);
                            news.Category = proposedNews.Category;
                            news.UserId = User.Identity.GetUserId();
                            
                            db.ProposedNews.Remove(proposedNews);
                            db.News.Add(news);
                            db.SaveChanges();
                            TempData["message"] = "Stirea a fost acceptata cu succes!";
                        }
                    }
                    
                    return RedirectToAction("IndexProposedNews");
                }
                else
                {
                    return View("IndexProposedNews");
                }

            }
            catch (Exception e)
            {
                return View("IndexProposedNews");
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(int id)
        {
            News news = db.News.Find(id);
            if (news.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.News.Remove(news);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine!";
                return RedirectToAction("Index");
            }
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // Extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // Adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            // returnam lista de categorii
            return selectList;
        }

        public ActionResult SearchNews(string searching)
        {
            var listOfNews = new List<News>();

            var allNews = from article in db.News
                             select article;

            foreach (var news in allNews)
            {
                if (news.Title.Contains(searching))
                {
                    listOfNews.Add(news);
                }
            }

            ViewBag.News = listOfNews;
            return View("Index");
        }
    }
}