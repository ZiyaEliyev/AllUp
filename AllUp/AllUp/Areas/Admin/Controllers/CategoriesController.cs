using AllUp.DAL;
using AllUp.Helper;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public CategoriesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _db.Categories.OrderByDescending(x=>x.IsMain).Include(x => x.Children).Include(x => x.Parent).ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, int? mainCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            if (category.IsMain)
            {
                bool isExist = await _db.Categories.AnyAsync(x => x.Name == category.Name);
                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category already is exist");
                    return View();
                }

                #region Save Image
                if (category.Photo == null)
                {
                    ModelState.AddModelError("Photo", "Image can not be null !");
                    return View();
                }
                if (!category.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select image type file !");
                    return View();
                }
                if (category.Photo.IsOlder1Mb())
                {
                    ModelState.AddModelError("Photo", "File size can be max 1Mb !");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                category.Image = await category.Photo.SaveFileAsync(folder);
                #endregion

            }


            else
            {
                category.ParentId= mainCatId;
            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
