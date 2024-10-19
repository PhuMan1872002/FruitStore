using DepartMentStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DepartMentStore.Controllers
{
    public class CategoryController : Controller
    {
        // GET: Category
        DepartmentStoreEntities da = new DepartmentStoreEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ListCate()
        {
            IEnumerable<Category> ds = da.Categories.OrderByDescending(s => s.Cate_id);
            
            return View(ds);
        }
        // GET: Category/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }
        // POST: Category/Create
        [HttpPost]
        public ActionResult Create(Category cate,FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here
                Category c = new Category();
                c=cate;
                da.Categories.Add(c);
                da.SaveChanges();
                return RedirectToAction("ListCate");
            }
            catch
            {
                return View();
            }
        }

        // GET: Category/Edit/5
        public ActionResult Edit(int id)
        {
            Category c = da.Categories.FirstOrDefault(s => s.Cate_id == id);
            return View(c);
        }

        // POST: Category/Edit/5
        [HttpPost]
        public ActionResult Edit(int id,Category category, FormCollection collection)
        {
            try
            {
                Category c = da.Categories.FirstOrDefault(s => s.Cate_id == id);
                // TODO: Add update logic here
                c.Name = category.Name;
                da.SaveChanges();
                return RedirectToAction("ListCate");
            }
            catch
            {
                return View();
            }
        }

        // GET: Category/Delete/5
        public ActionResult Delete(int id)
        {
            Category c = da.Categories.FirstOrDefault(s => s.Cate_id == id);
            return View(c);
        }

        // POST: Category/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                Category c = da.Categories.FirstOrDefault(s => s.Cate_id == id);
                da.Categories.Remove(c);
                da.SaveChanges();
                return RedirectToAction("ListCate");
            }
            catch
            {
                return View();
            }
        }
    }
}
