﻿using GrozaNews.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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

        public ActionResult IndexThumbNews()
        {
            //de verificat cum arata impartirea pe pagini (ulterior si cu stilizare din view-uri, care momentan sunt temporare si de vazut si cum e cu partial views)
            var thumbNews = db.ThumbnailedNews.Include("User").OrderByDescending(a => a.Date);

            var totalItems = thumbNews.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }
            
            var paginatedThumbNews = thumbNews.Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.perPage = this._perPage;
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.ThumbNews = paginatedThumbNews;

            return View();
        }

        public ActionResult IndexProposedNews()
        {
            var news = db.ProposedNews.Include("Category").Include("User").OrderByDescending(a => a.Date);


            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.News = news;

            if (TempData.ContainsKey("ProposedNewsToDelete") == true)
            {
                TempData.Remove("ProposedNewsToDelete");
            }

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

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult NewThumbnail()
        {
            ThumbnailedNews news = new ThumbnailedNews();

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

            if (TempData.ContainsKey("ProposedNewsToDelete") == true)
            {
                TempData.Remove("ProposedNewsToDelete");
            }

            return View(news);
        }

        [Authorize(Roles = "Editor, Administrator")]
        public ActionResult EditProposedNews(int id)
        {
            ProposedNews proposedNews = db.ProposedNews.Find(id);
            News news = new News();

            news.Title = proposedNews.Title;
            //// Protect content from XSS
            //// requestNews.Content = Sanitizer.GetSafeHtmlFragment(requestNews.Content);
            news.Content = proposedNews.Content;
            news.Date = proposedNews.Date;
            news.ImageData = proposedNews.ImageData;
            news.ImageMimeType = proposedNews.ImageMimeType;
            news.ImageThumbnail = proposedNews.ImageThumbnail;
            news.CategoryId = proposedNews.CategoryId ?? default(int);
            news.Category = proposedNews.Category;
            news.UserId = User.Identity.GetUserId();
            
            string imreBase64Data = Convert.ToBase64String(news.ImageData, 0, news.ImageMimeType);
            string imgDataURL = string.Format("data:image;base64,{0}", imreBase64Data);
            
            //Passing image data in viewbag to view  
            ViewBag.Img = imgDataURL;
            ViewBag.Categories = GetAllCategories();
            TempData["ProposedNewsToDelete"] = id;
            TempData["ImageData"] = proposedNews.ImageData;
            TempData["ImageMimeType"] = proposedNews.ImageMimeType;
            TempData["ImageThumbnail"] = proposedNews.ImageThumbnail;

            return View("New", news);
        }

        [Authorize(Roles = "Editor,Administrator")]
        [ValidateInput(false)] //dafuq?
        [HttpPost]
        public ActionResult New(News news, HttpPostedFileBase image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Protect content from XSS --> wtf treb citit in lab ce e
                    // news.Content = Sanitizer.GetSafeHtmlFragment(article.Content);
                    if (image != null)
                    {
                        //attach the uploaded image to the object before saving to Database
                        news.ImageMimeType = image.ContentLength;
                        news.ImageData = new byte[image.ContentLength];
                        image.InputStream.Read(news.ImageData, 0, image.ContentLength);

                        //Save image to file
                        var filename = image.FileName;
                        var filePathOriginal = Server.MapPath("/Content/Uploads/Originals");
                        var filePathThumbnail = Server.MapPath("/Content/Uploads/Thumbnails");
                        string savedFileName = Path.Combine(filePathOriginal, filename);
                        image.SaveAs(savedFileName);

                        //Read image back from file and create thumbnail from it
                        var imageFile = Path.Combine(Server.MapPath("~/Content/Uploads/Originals"), filename);
                        using (var srcImage = Image.FromFile(imageFile))
                        using (var newImage = new Bitmap(100, 100))
                        using (var graphics = Graphics.FromImage(newImage))
                        using (var stream = new MemoryStream())
                        {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(srcImage, new Rectangle(0, 0, 100, 100));
                            newImage.Save(stream, ImageFormat.Png);
                            var thumbNew = File(stream.ToArray(), "image/png");
                            news.ImageThumbnail = thumbNew.FileContents;
                        }
                    }

                    if (TempData.ContainsKey("ImageData"))
                    {
                        news.ImageMimeType = (int)TempData["ImageMimeType"];
                        news.ImageData = (byte[])TempData["ImageData"];

                        using (var ms = new MemoryStream(news.ImageData))
                        using (var srcImage = Image.FromStream(ms))
                        using (var newImage = new Bitmap(100, 100))
                        using (var graphics = Graphics.FromImage(newImage))
                        using (var stream = new MemoryStream())
                        {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(srcImage, new Rectangle(0, 0, 100, 100));
                            newImage.Save(stream, ImageFormat.Png);
                            var thumbNew = File(stream.ToArray(), "image/png");
                            news.ImageThumbnail = thumbNew.FileContents;
                        }

                        TempData.Remove("ImageData");
                        TempData.Remove("ImageMimeType");
                        TempData.Remove("ImageThumbnail");
                    }

                    db.News.Add(news);
                    
                    if (TempData.ContainsKey("ProposedNewsToDelete") == true)
                    {
                        db.ProposedNews.Remove(db.ProposedNews.Find(TempData["ProposedNewsToDelete"]));
                        db.SaveChanges();
                        TempData["message"] = "Stirea a fost adaugata!";
                        return RedirectToAction("IndexProposedNews");
                    }
                    else
                    {
                        db.SaveChanges();
                        TempData["message"] = "Stirea a fost adaugata!";
                        return RedirectToAction("Index");
                    }
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
        [ValidateInput(false)] //dafuq?
        [HttpPost]
        public ActionResult NewThumbnail(ThumbnailedNews news, HttpPostedFileBase image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (image != null)
                    {
                        //attach the uploaded image to the object before saving to Database
                        news.ImageMimeType = image.ContentLength;
                        news.ImageData = new byte[image.ContentLength];
                        image.InputStream.Read(news.ImageData, 0, image.ContentLength);

                        //Save image to file
                        var filename = image.FileName;
                        var filePathOriginal = Server.MapPath("/Content/Uploads/Originals");
                        var filePathThumbnail = Server.MapPath("/Content/Uploads/Thumbnails");
                        string savedFileName = Path.Combine(filePathOriginal, filename);
                        image.SaveAs(savedFileName);

                        //Read image back from file and create thumbnail from it
                        var imageFile = Path.Combine(Server.MapPath("~/Content/Uploads/Originals"), filename);
                        using (var srcImage = Image.FromFile(imageFile))
                        using (var newImage = new Bitmap(100, 100))
                        using (var graphics = Graphics.FromImage(newImage))
                        using (var stream = new MemoryStream())
                        {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(srcImage, new Rectangle(0, 0, 100, 100));
                            newImage.Save(stream, ImageFormat.Png);
                            var thumbNew = File(stream.ToArray(), "image/png");
                            news.ImageThumbnail = thumbNew.FileContents;
                        }
                    }

                    TempData["message"] = "Stirea a fost adaugata!";

                    //Save model object to database
                    db.ThumbnailedNews.Add(news);
                    db.SaveChanges();

                    return RedirectToAction("IndexThumbNews");
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

        public FileContentResult GetThumbnailImage(int NewsId)
        {
            ThumbnailedNews news = db.ThumbnailedNews.FirstOrDefault(p => p.NewsId == NewsId);
            if (news != null)
            {
                if (news.ImageThumbnail != null && news.ImageMimeType.ToString() != null)
                    return File(news.ImageThumbnail, news.ImageMimeType.ToString());
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public FileContentResult GetImage(int NewsId)
        {
            News news = db.News.FirstOrDefault(p => p.NewsId == NewsId);
            if (news != null)
            {
                if (news.ImageData != null && news.ImageMimeType.ToString() != null)
                    return File(news.ImageData, news.ImageMimeType.ToString());
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public FileContentResult GetProposedImage(int NewsId)
        {
            ProposedNews news = db.ProposedNews.FirstOrDefault(p => p.NewsId == NewsId);
            if (news != null)
            {
                if (news.ImageData != null && news.ImageMimeType.ToString() != null)
                    return File(news.ImageData, news.ImageMimeType.ToString());
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        [Authorize(Roles = "User, Editor,Administrator")]
        [ValidateInput(true)] //dafuq?
        [HttpPost]
        public ActionResult ProposeNews(ProposedNews news, HttpPostedFileBase image)
        {
            try
            {
                if (ModelState.IsValid && news.CategoryId != null)
                {
                    if (image != null)
                    {
                        //attach the uploaded image to the object before saving to Database
                        news.ImageMimeType = image.ContentLength;
                        news.ImageData = new byte[image.ContentLength];
                        image.InputStream.Read(news.ImageData, 0, image.ContentLength);

                        //Save image to file
                        var filename = image.FileName;
                        var filePathOriginal = Server.MapPath("/Content/Uploads/Originals");
                        var filePathThumbnail = Server.MapPath("/Content/Uploads/Thumbnails");
                        string savedFileName = Path.Combine(filePathOriginal, filename);
                        image.SaveAs(savedFileName);

                        //Read image back from file and create thumbnail from it
                        var imageFile = Path.Combine(Server.MapPath("~/Content/Uploads/Originals"), filename);
                        using (var srcImage = Image.FromFile(imageFile))
                        using (var newImage = new Bitmap(100, 100))
                        using (var graphics = Graphics.FromImage(newImage))
                        using (var stream = new MemoryStream())
                        {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(srcImage, new Rectangle(0, 0, 100, 100));
                            newImage.Save(stream, ImageFormat.Png);
                            var thumbNew = File(stream.ToArray(), "image/png");
                            news.ImageThumbnail = thumbNew.FileContents;
                        }
                    }

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
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei stiri care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult EditThumb(int id)
        {
            ThumbnailedNews news = db.ThumbnailedNews.Find(id);
            ViewBag.Categories = GetAllCategories();

            if (news.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                return View(news);
            }
            else
            {
                // recomand ca nici macar sa nu apara butonul de edit daca articolul nu e al tau // in view un check
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei stiri care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Editor,Administrator")]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(News requestNews, HttpPostedFileBase image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    News news = db.News.Find(requestNews.NewsId);
                    if (news.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(news))
                        {
                            if (image != null)
                            {
                                //attach the uploaded image to the object before saving to Database
                                news.ImageMimeType = image.ContentLength;
                                news.ImageData = new byte[image.ContentLength];
                                image.InputStream.Read(news.ImageData, 0, image.ContentLength);

                                //Save image to file
                                var filename = image.FileName;
                                var filePathOriginal = Server.MapPath("/Content/Uploads/Originals");
                                var filePathThumbnail = Server.MapPath("/Content/Uploads/Thumbnails");
                                string savedFileName = Path.Combine(filePathOriginal, filename);
                                image.SaveAs(savedFileName);

                                //Read image back from file and create thumbnail from it
                                var imageFile = Path.Combine(Server.MapPath("~/Content/Uploads/Originals"), filename);
                                using (var srcImage = Image.FromFile(imageFile))
                                using (var newImage = new Bitmap(100, 100))
                                using (var graphics = Graphics.FromImage(newImage))
                                using (var stream = new MemoryStream())
                                {
                                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    graphics.DrawImage(srcImage, new Rectangle(0, 0, 100, 100));
                                    newImage.Save(stream, ImageFormat.Png);
                                    var thumbNew = File(stream.ToArray(), "image/png");
                                    news.ImageThumbnail = thumbNew.FileContents;
                                }
                            }

                            news.Title = requestNews.Title;
                            // Protect content from XSS
                            // requestNews.Content = Sanitizer.GetSafeHtmlFragment(requestNews.Content);
                            // news.Content = requestNews.Content;
                            // news.Date = requestNews.Date;
                            // news.CategoryId = requestArticle.CategoryId;
                            news = requestNews;
                            db.SaveChanges();
                            TempData["message"] = "Stirea a fost modificata!";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei stiri care nu va apartine!";
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
        [HttpPost]
        public ActionResult EditThumb(ThumbnailedNews requestNews, HttpPostedFileBase image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ThumbnailedNews news = db.ThumbnailedNews.Find(requestNews.NewsId);
                    if (news.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(news))
                        {
                            if (image != null)
                            {
                                //attach the uploaded image to the object before saving to Database
                                news.ImageMimeType = image.ContentLength;
                                news.ImageData = new byte[image.ContentLength];
                                image.InputStream.Read(news.ImageData, 0, image.ContentLength);

                                //Save image to file
                                var filename = image.FileName;
                                var filePathOriginal = Server.MapPath("/Content/Uploads/Originals");
                                var filePathThumbnail = Server.MapPath("/Content/Uploads/Thumbnails");
                                string savedFileName = Path.Combine(filePathOriginal, filename);
                                image.SaveAs(savedFileName);

                                //Read image back from file and create thumbnail from it
                                var imageFile = Path.Combine(Server.MapPath("~/Content/Uploads/Originals"), filename);
                                using (var srcImage = Image.FromFile(imageFile))
                                using (var newImage = new Bitmap(100, 100))
                                using (var graphics = Graphics.FromImage(newImage))
                                using (var stream = new MemoryStream())
                                {
                                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                    graphics.DrawImage(srcImage, new Rectangle(0, 0, 100, 100));
                                    newImage.Save(stream, ImageFormat.Png);
                                    var thumbNew = File(stream.ToArray(), "image/png");
                                    news.ImageThumbnail = thumbNew.FileContents;
                                }
                            }

                            news.Title = requestNews.Title;
                            news.Headline = requestNews.Headline;
                            news.LinkStire = requestNews.LinkStire;
                            news.Date = requestNews.Date;
                            // Protect content from XSS
                            // requestNews.Content = Sanitizer.GetSafeHtmlFragment(requestNews.Content);
                            // news.Content = requestNews.Content;
                            // news.Date = requestNews.Date;
                            // news.CategoryId = requestArticle.CategoryId;
                            news = requestNews;
                            db.SaveChanges();
                            TempData["message"] = "Stirea a fost modificata!";
                        }
                        return RedirectToAction("IndexThumbNews");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei stiri care nu va apartine!";
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
                    else if (submitButton == "Edit")
                    {
                        return RedirectToAction("EditProposedNews", new { id });
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
                            news.Date = DateTime.Now.ToString();
                            news.ImageData = proposedNews.ImageData;
                            news.ImageMimeType = proposedNews.ImageMimeType;
                            news.ImageThumbnail = proposedNews.ImageThumbnail;
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
                TempData["message"] = "Stirea a fost stearsa!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o stire care nu va apartine!";
                return RedirectToAction("Index");
            }
        }
        
        [HttpDelete]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult DeleteThumb(int id)
        {
            ThumbnailedNews news = db.ThumbnailedNews.Find(id);
            if (news.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.ThumbnailedNews.Remove(news);
                db.SaveChanges();
                TempData["message"] = "Stirea a fost steara!";
                return RedirectToAction("IndexThumbNews");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o stire care nu va apartine!";
                return RedirectToAction("IndexThumbNews");
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

            TempData["message"] = "Rezultatele cautarii";
            return View("Index");
        }
    }
}