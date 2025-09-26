using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DemoVolunteer.Models;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using DemoVolunteer.Data;

namespace DemoVolunteer.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;       // ใช้ _context สำหรับ query/add/remove database
        _userManager = userManager; // ใช้ _userManager ดึงข้อมูล user        
    }

    //หน้าแรกแสดงโพสทั้งหมด
    public async Task<IActionResult> Index(int? categorieId) //เพิ่มพารามิเตอร์
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.FullName = user.FullName;
            ViewBag.Gender = user.Gender;
            ViewBag.PhoneNumber = user.PhoneNumber;
            ViewBag.Email = user.Email;
        }

        //ส่งข้อมูล Categories ไปที่ View ผ่าน ViewBag //แก้ไขการ filter หมวดหมู่
        ViewBag.Categories = _context.Categories.ToList();
        IQueryable<Post> query = _context.Posts.Include(p => p.Owner).Where(p => p.IsActive == true);

        if (categorieId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categorieId.Value);
            ViewBag.SelectedCategoryId = categorieId.Value;
        }

        var posts = await query.ToListAsync();
        return View(posts);
    }

    //ค้นหา post ตามชื่อ title
    public async Task<IActionResult> Search(string keyword)
    {
        ViewBag.Categories = _context.Categories.ToList();
        var posts = await _context.Posts
                .Include(p => p.Owner)
                .Where(p => p.Title.Contains(keyword) == true)
                .Where(p => p.IsActive == true)
                .ToListAsync();
        return View(posts);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
