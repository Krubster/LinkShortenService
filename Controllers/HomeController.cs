using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LinkShortenService.Models;
using System.Security.Cryptography;
using System.Text;
using LinkShortenService.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace LinkShortenService.Controllers
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        private UrlContext db;
        public HomeController(UrlContext context)
        {
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.URLs.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UrlRedirect(string url)
        {
            string redirect = FindUrl(url);
            if (redirect != null)
            {
                await db.SaveChangesAsync();
                return Redirect(redirect);
            }
            else
                return Content("No such link!");
        }

        private string FindUrl(string url)
        {
            foreach(UrlModel model in db.URLs)
            {
                if (model.ShortURL.Equals(url))
                {
                    model.Counter += 1;
                    return model.OriginalURL;
                }
            }
            return null;
        }

        [HttpGet]
        public ActionResult EditUrl(int? id)
        {
            if (id == null)
            {
                return Content("No ID");
            }
            UrlModel url = db.URLs.Find(id);
            if (url != null)
            {
                return View(url);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EditUrl(UrlModel url)
        {
            db.Entry(url).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteUrl(int? id)
        {
            if (id == null)
            {
                return Content("Bad Request");
            }
            UrlModel book = await db.URLs.FindAsync(id);
            if (book == null)
            {
                return Content("Not found!");
            }
            return View(book);
        }

        [HttpPost, ActionName("DeleteUrl")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UrlModel book = await db.URLs.FindAsync(id);
            db.URLs.Remove(book);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UrlConverter(string url)
        {
            if(url == null)
                return Content("Указанный URL невалиден!");

            Regex regex = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
            if(!regex.IsMatch(url))
                return Content("Указанный URL невалиден!");

            if (!ContainsUrl(url))
            {
                string shorten = Crc32.Compute(Encoding.ASCII.GetBytes(url)).ToString("X");
                UrlModel model = new UrlModel()
                {
                    OriginalURL = url,
                    ShortURL = shorten,
                    Created = DateTime.Now,
                    Counter = 0
                };
                db.URLs.Add(model);
                await db.SaveChangesAsync();
            }
            else
            {
                return Content("Указанный URL уже существует в базе!");
            }
            return RedirectToAction("Index");
        }

        private bool ContainsUrl(string url)
        {
            foreach(UrlModel model in db.URLs)
            {
                if (model.OriginalURL.Equals(url))
                    return true;
            }
            return false;
        }
    }
}
