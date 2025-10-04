using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DemoVolunteer.Models;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using DemoVolunteer.Data;
using MyMvcProject.Controllers;

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
        UpdateExpiredPosts(); // เรียกใช้ function อัพเดทสถานะโพสต์ที่หมดอายุ
        UpdateAppointmenComplete(); //เรียกใช้ function อัพเดทสถานะโพสต์ที่สิ้นสุดกิจกรรมและแจกคะแนนสมาชิก
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

        var posts = await query.Where(p => p.IsActive == true).ToListAsync();
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


    public void UpdateExpiredPosts()
    {
        // ดึงโพสต์ที่หมดอายุ
        var expiredPosts = _context.Posts
            .Where(p => p.AppointmentDateEnd <= DateTime.Now && p.IsActive == true)
            .ToList();

        if (expiredPosts.Any())
        {
            foreach (var post in expiredPosts)
            {
                if (post.AppointmentDateEnd != null || post.Status == "Expired")
                {
                    // เปลี่ยนสถานะโพสต์
                    post.IsActive = false;
                    post.Status = "Expired";
                }
            }
        }
        // SaveChanges ครั้งเดียว
        _context.SaveChanges();
    }
    

    public void UpdateAppointmenComplete()
    {
        // ดึงโพสต์ที่หมดอายุ
        var CompletePosts = _context.Posts
            .Where(p => p.Status == "Expired" && p.AppointmentDateEnd <= DateTime.Now && p.AppointmentDateArrive <= DateTime.Now && p.IsActive == true)
            .ToList();

        if (CompletePosts.Any())
        {
            foreach (var post in CompletePosts)
            {
                // เปลี่ยนสถานะโพสต์
                post.Status = "Complete";
                post.IsActive = false;

                // ดึง join ที่เกี่ยวข้องแล้วอัปเดตสถานะ
                var joins = _context.Joins
                    .Include(j => j.User) // include User เพื่อให้ EF track คะแนน
                    .Where(j => j.PostId == post.PostId)
                    .ToList();
                if (joins.Any())
                {
                    foreach (var join in joins)
                    {
                        join.Status = "Complete";
                        join.User.Score += post.Score;
                    }
                }
            }
            // SaveChanges ครั้งเดียว
            _context.SaveChanges();
        }
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
