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
    public class CommentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // ulterior si ceva cu show prev comments cand sunt prea multe

        // GET: Comment
        [HttpPost]
        [Authorize(Roles = "User, Editor, Administrator")]
        public ActionResult New(Comment comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    comment.UserId = User.Identity.GetUserId();
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    // TempData["message"] = "Articolul a fost adaugat!";
                    return Redirect($"/News/Show/{comment.NewsId}"); // aici ar trebui sa fac un partial view pt show news si apoi cand se adauga un comm in viewul comment/show sa se fol de partial view news/show si sa dea redirect la id-ul corespunzator news/show/id
                }
                else
                {
                    return View(comment);
                }
            }
            catch (Exception e)
            {
                return View(comment);
            }
        }

        // GET: Editarea unui comentariu.
        [Authorize(Roles = "User, Editor, Administrator")]
        public ActionResult Edit(int id)
        {
            Comment comment = db.Comments.Find(id);
            //ViewBag.Categories = GetAllCategories();

            if (comment.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                TempData["inEditMode"] = true;
                TempData["commentInEditMode"] = id;
                return Redirect($"/News/Show/{comment.NewsId}");
                //return View(comment);
            }
            else
            {
                // recomand ca nici macar sa nu apara butonul de edit daca articolul nu e al tau // in view un check
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra acestui comentariu care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User, Editor, Administrator")]
        [ValidateInput(false)]
        [HttpPut]
        public ActionResult Edit(int id, Comment requestComment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Comment comment = db.Comments.Find(id);
                    if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(comment))
                        {
                            comment.Content = requestComment.Content;
                            comment = requestComment;

                            db.SaveChanges();
                            TempData["message"] = "Comentariul a fost modificat!";
                            TempData.Remove("inEditMode");
                            TempData.Remove("commentInEditMode");
                        }
                        return Redirect($"/News/Show/{comment.NewsId}");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra acestui comentariu care nu va apartine!";
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    return View(requestComment);
                }
            }
            catch (Exception)
            {
                return View(requestComment);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User, Editor,Administrator")]
        public ActionResult Delete(int id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                // TempData["message"] = "Comentariul a fost sters!";
                return Redirect($"/News/Show/{comment.NewsId}");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un comentariu care nu va apartine!";
                return Redirect($"/News/Show/{comment.NewsId}");
            }
        }

    }
}