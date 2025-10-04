using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoVolunteer.Data;
using DemoVolunteer.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Text;
using System.Globalization;
                        

namespace MyMvcProject.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;  // ใช้ _context สำหรับ query
            _userManager = userManager;  // ใช้ _userManager ดึงข้อมูล user 
        }

        // ส่งค่า Notification Model ผ่านการรับค่าจาก paramitor
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

        // การสร้างหน้า Create Post
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.categories = categories;
            //ส่งข้อมูล Categories ไปที่ View ผ่าน ViewBag
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // รับข้อมูลจาก Form Create Post และ newPost ลงใน DB
        [HttpPost]
        public async Task<IActionResult> Create(Post model, IFormFile imageFile, IFormFile imageFile2)
        {
            var user = await _userManager.GetUserAsync(User);
            if (model == null || user == null)
            {
                return Content("Model is null");
            }

            // ถ้ามีการอัปภาพเข้ารวมกิจกรรม
            if (imageFile2 != null && imageFile2.Length > 0)
            {
                var fileName2 = $"appointImg-{Guid.NewGuid()}{Path.GetExtension(imageFile2.FileName)}";
                var path2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName2);
                using (var stream = new FileStream(path2, FileMode.Create))
                {
                    await imageFile2.CopyToAsync(stream);
                }
                model.AppointImg = "/img/" + fileName2; // สมมุติว่ามี field นี้ใน Model
            }
            else
            {
                model.AppointImg = null;
            }
            // ถ้ามีการอัปภาพกิจกรรม
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
            }
            else
            {
                //กรณีไม่ได้กำหนดรูปภาพ
                model.ImgURL = null;
            }
            // คำนวน จำนวนชั่วโมงกิจกรรม
            DateTime? date1 = model.AppointmentDate;
            DateTime? date2 = model.AppointmentDateArrive;

            DateTime? time1 = model.TimeStart;
            DateTime? time2 = model.TimeEnd;
            double totalDays = 0;
            double totalHours = 0;

            if (date1.HasValue && date2.HasValue)
            {
                TimeSpan duration = date2.Value - date1.Value;
                totalDays = duration.TotalDays + 1;
            }
            if (time1.HasValue && time2.HasValue)
            {
                TimeSpan duration = time2.Value - time1.Value;
                totalHours = duration.TotalHours;
            }
            double score = totalDays * totalHours;
            model.Score = Convert.ToInt32(score);
            // บันทึก model ลง DB หรือทำงานต่อ
            var newPost = new Post
            {
                Title = model.Title,
                MaxParticipants = model.MaxParticipants,
                Location = model.Location,
                CategoryId = model.CategoryId,
                OwnerId = user.Id,
                AppointmentDate = model.AppointmentDate,
                AppointmentDateArrive = model.AppointmentDateArrive,
                TimeStart = model.TimeStart,
                TimeEnd = model.TimeEnd,
                Description = model.Description,
                AppointImg = model.AppointImg,
                AppointmentDateEnd = model.AppointmentDateEnd,
                Score = model.Score,
                Status = "Open",
                ImgURL = model.ImgURL,
                CreatedAt = DateTime.Now
            };

            // var json = JsonSerializer.Serialize(newPost);
            // Console.WriteLine(json); // หรือ Debug.WriteLine(json);

            // เพิ่มลงใน DB
            _context.Posts.Add(newPost);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อนเจ้าของ post ว่าได้สร้าง post ใหม่
            var Newnotify = notificationModel(user.Id, $"คุณได้สร้างกิจกรรม '{newPost.Title}'");
            _context.Notifications.Add(Newnotify);
            await _context.SaveChangesAsync();

            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = "สร้างโพสสำเร็จ!";
            TempData["PopupType"] = "success"; // success, error, inf
            return Redirect("/Post/Manager");
        }

        // แสดงรายละเอียดของ post
        [HttpGet]
        public async Task<IActionResult> Detail(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.Owner)
                .Include(p => p.Category)
                .Include(p => p.Joins)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            // ตรวจสอบ post เต็มรึยัง?
            var joins = await _context.Joins
                .Where(p => p.PostId == postId)
                .ToListAsync();
            return View(post);
        }

        // หน้าแก้ไขข้อมูล post
        [HttpGet]
        public async Task<IActionResult> Edit(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            ViewBag.Categories = _context.Categories.ToList();
            return View(post);
        }

        // ดำเนินการแก้ไขข้อมูล post
        [HttpPost]
        public async Task<IActionResult> Edit(Post model, IFormFile? imageFile, IFormFile? imageFile2)
        {
            var user = await _userManager.GetUserAsync(User);
            var post = await _context.Posts.FindAsync(model.PostId); // ดึงรายการเดียวตาม PostId
            if (model == null || user == null)
            {
                return Content("Model is null");
            }

            // ถ้ามีการแก้ไขภาพเข้ารวมกิจกรรม
            if (imageFile2 != null && imageFile2.Length > 0)
            {
                var fileName2 = $"appointImg-{Guid.NewGuid()}{Path.GetExtension(imageFile2.FileName)}";
                var path2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName2);
                using (var stream = new FileStream(path2, FileMode.Create))
                {
                    await imageFile2.CopyToAsync(stream);
                }
                model.AppointImg = "/img/" + fileName2; // สมมุติว่ามี field นี้ใน Model
            }

            // ถ้ามีการแก้ไขภาพกิจกรรม
            if (imageFile != null && imageFile.Length > 0 && post.ImgURL != null)
            {
                // ตั้งชื่อไฟล์
                string[] arrayPath = post.ImgURL.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var FileName = arrayPath[1];
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                // กำหนด ImgURL ใน Model เป็น path สำหรับแสดงภาพ
                model.ImgURL = "/img/" + FileName;
                Console.WriteLine("เปลี่ยนภาพ");
            }
            else if (imageFile != null && imageFile.Length > 0 && post.ImgURL == null)
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
                Console.WriteLine("เปลี่ยนภาพ");
            }
            // คำนวน จำนวนชั่วโมงกิจกรรม
            DateTime? date1 = model.AppointmentDate;
            DateTime? date2 = model.AppointmentDateArrive;

            DateTime? time1 = model.TimeStart;
            DateTime? time2 = model.TimeEnd;
            double totalDays = 0;
            double totalHours = 0;

            if (date1.HasValue && date2.HasValue)
            {
                TimeSpan duration = date2.Value - date1.Value;
                totalDays = duration.TotalDays + 1;
            }
            if (time1.HasValue && time2.HasValue)
            {
                TimeSpan duration = time2.Value - time1.Value;
                totalHours = duration.TotalHours;
            }
            double score = totalDays * totalHours;
            int Score = Convert.ToInt32(score);


            // อัปเดตค่าจากฟอร์ม
            if (model.AppointmentDateEnd > DateTime.Now || model.AppointmentDateEnd == null)
            {
                post.Status = "Open";
                post.IsActive = true;
            }
            post.Title = model.Title;
            post.CategoryId = model.CategoryId;
            post.Location = model.Location;
            post.MaxParticipants = model.MaxParticipants;
            post.Description = model.Description;
            post.AppointmentDate = model.AppointmentDate;
            post.AppointmentDateArrive = model.AppointmentDateArrive;
            post.TimeStart = model.TimeStart;
            post.TimeEnd = model.TimeEnd;
            post.Score = Score;
            post.AppointmentDateEnd = model.AppointmentDateEnd;

            if (imageFile != null)
            {
                post.ImgURL = model.ImgURL;
            }
            if (imageFile2 != null)
            {
                post.AppointImg = model.AppointImg;
            }

            await _context.SaveChangesAsync();

            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = $"แก้ไขโพสตแล้ว!!";
            TempData["PopupType"] = "success"; // success, error, inf
            return Redirect("/Post/Manager");
        }

        // ลบโพส
        public async Task<IActionResult> Delete(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            var post = await _context.Posts.FindAsync(postId);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อนเจ้าของ post
            var notifyOwner = notificationModel(post.OwnerId, $"คุณได้ลบกิจกรรมของคุณในโพสต์ '{post.Title}'");
            _context.Notifications.Add(notifyOwner);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อน user ทุกคนที่เข้าร่วมกิจกรรม
            var joins = await _context.Joins
                .Where(p => p.PostId == postId)
                .ToListAsync();
            foreach (Join join in joins)
            {
                var notifyUser = notificationModel(join.UserId, $"กิจกรรมที่คุณได้เข้าร่วมในโพสต์ '{post.Title}' ได้ถูกลบไปแล้ว");
                _context.Notifications.Add(notifyUser);
                await _context.SaveChangesAsync();
            }
            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = $"ลบโพสตแล้ว!!";
            TempData["PopupType"] = "success"; // success, error, inf
            return Redirect("/Post/Manager");
        }

        // ขอเข้าร่วมกิจกรรม 
        [HttpPost]
        public async Task<IActionResult> Join(int postId)
        {
            // ตรวจสอบว่า user ได้ login เข้ามาไหม?
            if (!(User?.Identity != null && User.Identity.IsAuthenticated))
            {
                TempData["PopupMessage"] = "กรุณาเข้าสู่ระบบก่อนเข้าร่วมกิจกรรม!!";
                TempData["PopupType"] = "error";
                return RedirectToAction("Login", "User");
            }
            // user ได้ login และดึง user model
            var user = await _userManager.GetUserAsync(User);
            // ดึงรายการเดียวตาม PostId
            var post = await _context.Posts
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PostId == postId);
            var joins = await _context.Joins
                .Where(p => p.PostId == postId)
                .ToListAsync();
            if (post.Owner.Id == user.Id) // ตรวจสอบว่า userID Login ตรง กับ postOwnerID รึเปล่า?
            {
                TempData["PopupMessage"] = "เข้าร่วมกิจกรรมของตัวเองไม่ได้!!";
                TempData["PopupType"] = "error";
                return Redirect($"/Post/Detail?postId={postId}");
            }
            // ตรวจสอบว่ามีการกดเข้าร่วมกิจกรรมซ้ำรึป่าว?
            var ispost = await _context.Joins
                            .Where(p => p.UserId == user.Id)
                            .FirstOrDefaultAsync(p => p.PostId == postId);
            //ตรวจสอบการเข้าร่วมกิจรรมซ้ำ
            if (ispost != null)
            {
                TempData["PopupMessage"] = "เข้าร่วมกิจรรมซ้ำไม่ได้!!!";
                TempData["PopupType"] = "error";
                return Redirect($"/Post/Detail?postId={postId}");
            }
            // Post เต็ม
            if (joins.Count + 1 >= post.MaxParticipants)
            {
                post.Status = "Full";
                await _context.SaveChangesAsync();
                // return Redirect($"/Post/Detail?postId={postId}");
            }
            // สร้าง new Join model 
            var join = new Join
            {
                UserId = user.Id,
                PostId = postId,
                JoinDate = DateTime.Now,
                Status = "InProgress"
            };

            // นำ model ลงบน DB
            _context.Joins.Add(join);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อนเจ้าของ post
            var notifyOwner = notificationModel(post.OwnerId, $"{user.Email} เข้าร่วมกิจกรรมของคุณในโพสต์ '{post.Title}'");
            _context.Notifications.Add(notifyOwner);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อน user ที่เข้าร่วมกิจกรรม
            var notifyUser = notificationModel(user.Id, $"คุณได้เข้าร่วมกิจกรรมในโพสต์ '{post.Title}'");
            _context.Notifications.Add(notifyUser);
            await _context.SaveChangesAsync();

            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = "เข้าร่วมกิจกรรมสำเร็จ!";
            TempData["PopupType"] = "success"; // success, error, inf

            return Redirect($"/Post/Detail?postId={postId}");
        }

        //หน้าจัดการโพสของฉัน
        public async Task<IActionResult> Manager()
        {
            // ตรวจสอบว่า user ได้ login เข้ามาไหม?
            if (!(User?.Identity != null && User.Identity.IsAuthenticated))
            {
                //ไม่ได้ Login
                ModelState.AddModelError("", "Login First");
                return RedirectToAction("Login", "User");
            }
            var user = await _userManager.GetUserAsync(User);
            var joins = await _context.Joins.Where(p => p.UserId == user.Id).ToListAsync();
            var posts = await _context.Posts  //ดึง post พร้อมกับ join และมี OwnerId == userId
                .Include(p => p.Joins)
                .Where(p => p.Owner.Id == user.Id)
                .ToListAsync();

            var allPosts = await _context.Posts
                .Where(p => p.IsActive == true)
                .ToListAsync();

            ViewBag.allPostAmount = allPosts.Count; //จำนวนโพสทั้งหมดของสมาชิกอื่น
            ViewBag.postAmount = allPosts.Count + posts.Count; //จำนวนโพสทั้งหมด
            return View(posts);
        }

        // แสดงรายชื่อสมาชิกของ Post
        public async Task<IActionResult> PostJoinList(int postId)
        {
            var joins = await _context.Joins
                .Include(p => p.User)
                .Where(p => p.PostId == postId)
                .ToListAsync();
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            ViewBag.joinAmount = joins.Count;
            ViewBag.MaxParticipants = post.MaxParticipants;
            return View(joins);
        }

        // ลบสมาชิกออกจากกิจกรรม
        [HttpPost]
        public async Task<IActionResult> JoinDel(int joinId)
        {
            // ลบ Join
            var user = await _userManager.GetUserAsync(User);
            var join = await _context.Joins
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.JoinId == joinId);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == join.PostId);
            if (post.Status == "Full")
            {
                post.Status = "Open";
                await _context.SaveChangesAsync();
            }
            _context.Joins.Remove(join);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อนเจ้าของ post
            var notifyOwner = notificationModel(join.Post.OwnerId, $"{join.User.Email} ยกเลิกจากกิจกรรม {join.Post.Title}");
            _context.Notifications.Add(notifyOwner);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อน user ที่เข้าร่วมกิจกรรม
            var notifyUser = notificationModel(user.Id, $"คุณยกเลิกจากกิจกรรม {join.Post.Title}");
            _context.Notifications.Add(notifyUser);
            await _context.SaveChangesAsync();

            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = $"ยกเลิกจากกิจกรรมแล้ว!!";
            TempData["PopupType"] = "success"; // success, error, inf
            return Redirect("/Post/UserJoinList");
        }

        // แสดงรายชื่อการเข้าร่วมกิจกรรม ของ User
        public async Task<IActionResult> UserJoinList()
        {
            CultureInfo thaiCulture = new CultureInfo("th-TH");
            var user = await _userManager.GetUserAsync(User);
            var joins = await _context.Joins
                    .Include(p => p.Post)
                    .Where(p => p.UserId == user.Id)
                    .ToListAsync();
            ViewBag.thaiCulture = thaiCulture;
            ViewBag.Img = user.ImgURL;
            ViewBag.FullName = user.FullName;
            ViewBag.Phone = user.PhoneNumber;
            ViewBag.Email = user.Email;
            ViewBag.Score = user.Score;
            return View(joins);
        }

        public async Task<IActionResult> Close(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post.Status == "Open")
            {
                post.Status = "Close";
            }
            else
            {
                post.Status = "Open";
            }
            await _context.SaveChangesAsync();
            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            // TempData["PopupMessage"] = $"ปิดการรับสมาชิกแล้ว!!";
            // TempData["PopupType"] = "success"; // success, error, inf
            return Redirect("/Post/Manager");
        }

        public async Task<IActionResult> Members(int postId)
        {
            // ระบุ path ของไฟล์ HTML
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "html", "viewmember.html");

            // อ่านเนื้อหา HTML จากไฟล์
            var htmlTemplate = System.IO.File.ReadAllText(filePath);

            var joins = await _context.Joins
                .Include(p => p.User)
                .Where(p => p.PostId == postId)
                .ToListAsync();
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

            // สร้าง HTML สำหรับแต่ละ post
            var membertHtml = new StringBuilder();
            int i = 0;
            foreach (var join in joins)
            {
                membertHtml.AppendLine($"<tr>");
                membertHtml.AppendLine($"<td>{i + 1}</td>");
                membertHtml.AppendLine($"<td>{join.User.FullName}</td>");
                membertHtml.AppendLine($"<td>{join.User.PhoneNumber}</td>");
                if (join.User.Gender == "male")
                {
                    membertHtml.AppendLine($"<td><p class='gender' data-gender='male'>{join.User.Gender}</p></td>");
                }
                else
                {
                    membertHtml.AppendLine($"<td><p class='gender' data-gender='female'>{join.User.Gender}</p></td>");
                }
                membertHtml.AppendLine($"<td>");
                membertHtml.AppendLine($"<form action='/Post/MemberDel' method='post'>"); 
                membertHtml.AppendLine($"<input type='hidden' name='joinId' value='{join.JoinId}'>");
                membertHtml.AppendLine($"<button class='btn cancal' data-confirm='คุณต้องการลบสมาชิกที่คุณเลือก ใช่ไหม?' data-confirm-title='ยืนยันการลบสมาชิก' data-ok-text='ยืนยัน' data-cancel-text='ปิด' type='submit'>ลบสมาชิก</button>");
                membertHtml.AppendLine($"</form>");

                membertHtml.AppendLine($"</td>");
                membertHtml.AppendLine($"</tr>");
                i = i + 1;
            }

            // แทนที่ {{MenberList}} ด้วย HTML ที่สร้างจาก loop

            var finalHtml = htmlTemplate.Replace("{{joinCount}}", $"{joins.Count} / {post.MaxParticipants}");
            var finalHtmlV2 = finalHtml.Replace("{{MenberList}}", membertHtml.ToString());

            // ส่งกลับ HTML
            return Content(finalHtmlV2, "text/html");
        }

        [HttpPost]
        public async Task<IActionResult> MemberDel(int joinId) {
            // ลบ Join
            var user = await _userManager.GetUserAsync(User);
            var join = await _context.Joins
                .Include(p => p.Post)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.JoinId == joinId);

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == join.PostId);
            if (post.Status == "Full")
            {
                post.Status = "Open";
                await _context.SaveChangesAsync();
            }

            _context.Joins.Remove(join);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อนเจ้าของ post
            var notifyOwner = notificationModel(user.Id, $"คุณยกเลิกการเข้าร่วมกิจกรรมของ {join.User.FullName}");
            _context.Notifications.Add(notifyOwner);
            await _context.SaveChangesAsync();

            // แจ้งเตื่อน user ที่เข้าร่วมกิจกรรม
            var notifyUser = notificationModel(join.UserId, $"คุณถูกยกเลิกจากกิจกรรม {join.Post.Title}");
            _context.Notifications.Add(notifyUser);
            await _context.SaveChangesAsync();

            // แสดงในข้อความแจ้งเตือนใน pop up ที่ Redirect ไป
            TempData["PopupMessage"] = $"ลบสมาชิกออกจากกิจกรรมแล้ว!!";
            TempData["PopupType"] = "success"; // success, error, inf
            return Redirect("/Post/Manager");
        }


    }
}