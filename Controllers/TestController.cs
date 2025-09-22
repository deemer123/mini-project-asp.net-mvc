using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoVolunteer.Data;
using DemoVolunteer.Models;
using Microsoft.AspNetCore.Identity;

namespace MyMvcProject.Controllers
{
    //Controller ไว้ทดสอบ
    // [ApiController]
    // [Route("api/Test")]
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Notification notificationModel(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false
            };
            return notification;
        }

        private string GetProfileImage()
        {
            var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            var images = Directory.GetFiles(imgPath, "*.png").ToList();

            if (images.Count == 0)
                return null;

            var random = new Random();
            var randomImage = images[random.Next(images.Count)];

            return Path.GetFileName(randomImage);
        }


        //ตัวอย่างการเรียกรายการจาก Table Posts ทั้งหมด
        // var posts = await _context.Posts.ToListAsync(); 

        //ดึงเฉพาะ Post ที่ IsActive = true
        // var activePosts = await _context.Posts 
        //     .Where(p => p.IsActive)
        //     .ToListAsync();

        //ดึงพร้อม Owner (ApplicationUser)
        // var postsWithOwner = await _context.Posts 
        //     .Include(p => p.Owner)
        //     .ToListAsync()

        //ดึงพร้อม Category และ Joins
        // var postsFull = await _context.Posts  
        //     .Include(p => p.Category)
        //     .Include(p => p.Owner)
        //     .Include(p => p.Joins)
        //     .ToListAsync();

        // ดึงรายการเดียวตาม PostId
        // var post = await _context.Posts  
        //     .Include(p => p.Owner)
        //     .Include(p => p.Category)
        //     .FirstOrDefaultAsync(p => p.PostId == id);


        // เพิ่มข้อมูล
        // var group = new Group { Name = "My Group", OwnerId = userId };
        // _context.Groups.Add(group);
        // await _context.SaveChangesAsync();

        // ดึงข้อมูล
        // var groups = await _context.Groups
        //     .Include(g => g.Members)
        //     .ThenInclude(m => m.User)
        //     .ToListAsync();

        // แก้ไข
        // var group = await _context.Groups.FindAsync(groupId);
        // group.Name = "New Name";
        // await _context.SaveChangesAsync();

        // ลบ
        // var group = await _context.Groups.FindAsync(groupId);
        // _context.Groups.Remove(group);
        // await _context.SaveChangesAsync();

        // _context = ApplicationDbContext
        // ต้อง await _context.SaveChangesAsync() ทุกครั้งที่แก้ไขข้อมูล

        // Try Test
        public async Task<IActionResult> AddNofi(){
            var user = await _userManager.GetUserAsync(User);
            var notification = new Notification
            {
                UserId = user.Id,
                Message = "แจ้งเตือนจ้า!!!",
                IsRead = false
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return Content($"Add notification To {user.FullName}");
        }

        // CreatePost View
        [HttpGet]
        public IActionResult CreatePost() => View();
        [HttpPost]
        public async Task<IActionResult> CreatePost(Post model)
        {
            var user = await _userManager.GetUserAsync(User);
            //เพิ่มข้อมูล post
            var post = new Post
            {
                Title = model.Title,
                OwnerId = user.Id,
                CategoryId = model.CategoryId,
                Description = model.Description,
                Location = model.Location,
                MaxParticipants = model.MaxParticipants,
                AppointmentDate = model.AppointmentDate,
                TimeStart = model.TimeStart,
                TimeEnd = model.TimeEnd,
                Score = model.Score,
                Status = "Open",
                ImgURL = model.ImgURL
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Content($"add {post.Title} sucess");
        }

        public async Task<IActionResult> Read()
        {
            // ดึงพร้อม Owner (ApplicationUser)
            var postsWithOwner = await _context.Posts
                .Include(p => p.Owner)
                .ToListAsync();
            return View(postsWithOwner);
        }

        public async Task<IActionResult> Update()
        {
            return Content($"");
        }

        public async Task<IActionResult> Delete()
        {
            return Content($"");
        }

        [HttpGet]
        public async Task<IActionResult> PostUpload()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.categories = categories;
            //ส่งข้อมูล Categories ไปที่ View ผ่าน ViewBag
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostUpload(Post model, IFormFile imageFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (model == null || imageFile == null || user == null)
            {
                return BadRequest("Model is null");
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                // ตั้งชื่อไฟล์เอง เช่น ใช้ชื่อจาก Model หรือเวลาปัจจุบัน
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var customFileName = $"{user.Id}_" + timestamp + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", customFileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                // กำหนด ImgURL ใน Model เป็น path สำหรับแสดงภาพ
                model.ImgURL = "/img/" + customFileName;

                // อัปเดต ImgURL แล้วบันทึกอีกครั้ง
                // model.ImgURL = "/img/" + customFileName;
                // _context.Update(model);
                // await _context.SaveChangesAsync();
            }
            // บันทึก model ลง DB หรือทำงานต่อ
            var post = new Post
            {
                Title = model.Title,
                OwnerId = user.Id,
                CategoryId = model.CategoryId,
                Description = model.Description,
                Location = model.Location,
                MaxParticipants = model.MaxParticipants,
                AppointmentDate = model.AppointmentDate,
                TimeStart = model.TimeStart,
                TimeEnd = model.TimeEnd,
                Score = model.Score,
                Status = "Open",
                ImgURL = model.ImgURL
            };
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Upload success", imageUrl = model.ImgURL });
            // return RedirectToAction("Index");
        }
    }
}