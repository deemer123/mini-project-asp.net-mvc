using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DemoVolunteer.Models;
using DemoVolunteer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace DemoVolunteer.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;


    //DB Manager
    public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }


    // Register
    [HttpGet]
    public IActionResult Register() => View();
    [HttpPost]
    public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string confirmPassword, string gender, string phoneNumber)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match");
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Gender = gender,
            Email = email,
            PhoneNumber = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            ImgURL = "/img/default-profile.png"
        };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = "สมัครสมาชิกสำเร็จ!";
            TempData["PopupType"] = "success"; // success, error, inf
            return RedirectToAction("Login", "User");
        }
        foreach (var error in result.Errors)
        {
            // แสดง error ที่ view
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View();
    }

    // Login
    [HttpGet]
    public IActionResult Login() => View();
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
        if (result.Succeeded)
        {
            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = "เข้าสู่ระบบสำเร็จ!";
            TempData["PopupType"] = "success"; // success, error, inf
            return RedirectToAction("Index", "Home");
        }
        // กรณี login ไม่สำเร็จ
        ModelState.AddModelError("", "Invalid login attempt");
        return View();
    }


    // Logout
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
        TempData["PopupMessage"] = "ออกจากระบบเรียบร้อย!";
        TempData["PopupType"] = "success"; // success, error, inf
        return RedirectToAction("Index", "Home");
    }


    // GET Login User: Edit Profile
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        // ดึงข้อมูลของ user ที่ login
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "User");
        }
        //สร้าง instance ของ model และส่งไปยัง view
        var model = new UserViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Gender = user.Gender,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ImgURL = user.ImgURL
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserViewModel model, string? imageFile)
    {
        Console.WriteLine($"File : {imageFile}");
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // debug ดู errors 
            return Json(errors);
        }
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return RedirectToAction("Login", "User");

        if (!string.IsNullOrEmpty(imageFile) && imageFile.Contains(","))
        {
            var base64Data = imageFile.Split(',')[1];
            var bytes = Convert.FromBase64String(base64Data);
            var fileName = $"avatar_{user.Id}.png";
            var savePath = Path.Combine("wwwroot/img", fileName);
            System.IO.File.WriteAllBytes(savePath, bytes);
            model.ImgURL = "/img/" + fileName;
        }
        else
        {
            model.ImgURL = user.ImgURL;
            Console.WriteLine("ไม่มีรูปใหม่ ถูกข้ามการอัปโหลด");
        }

        // อัปเดตค่าจากฟอร์ม
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Gender = model.Gender;
        user.PhoneNumber = model.PhoneNumber;
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.ImgURL = model.ImgURL;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = "แก้ไขข้อมูลสำเร็จ!";
            TempData["PopupType"] = "success"; // success, error, inf
            return RedirectToAction("Edit", "User");
        }
        // ถ้ามี error
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == user.Id && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications)
        {
            n.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "All notifications marked as read" });
    }

    [HttpPost]
    public async Task<IActionResult> isNewNofic()
    {
        bool result = false;
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return View(new List<Notification>());
        }
        var notifications = await _context.Notifications
            .Where(n => n.UserId == user.Id)
            .Where(n => n.IsRead == false)
            .ToListAsync();
        if (notifications.Count > 0)
        {
            result = true;
        }
        return Json(new { result = result });
    }
}