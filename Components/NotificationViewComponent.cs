using Microsoft.AspNetCore.Mvc;
using DemoVolunteer.Data;
using Microsoft.EntityFrameworkCore;
using DemoVolunteer.Models;
using Microsoft.AspNetCore.Identity;


public class NotificationViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return View(new List<Notification>());
        }
        var notifications = await _context.Notifications
            .Where(n => n.UserId == user.Id)
            // .Where(n => n.IsRead == false)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
            // .Take(5)
        return View(notifications);
    }
}
