using Microsoft.EntityFrameworkCore;
using DemoVolunteer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace DemoVolunteer.Data
{
    // public class ApplicationDbContext : DbContext
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; } 
        public DbSet<Post> Posts { get; set; } 
        public DbSet<Join> Joins { get; set; } 
        public DbSet<Notification> Notifications { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships

            // ถ้า Post.Owner(ApplicationUser) ถูกลบ จะ ไม่ให้ลบ Post ที่อ้างถึง (Restrict)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // ป้องกันการลบ cascade ที่ SQLite

            // ถ้าลบ Category จะไม่ลบ Post แต่จะเซ็ต CategoryId = null
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);


            // ถ้าลบ User จะลบ Join ทั้งหมดของ user คนนั้น (Cascade)
            modelBuilder.Entity<Join>()
                .HasOne(p => p.User)
                .WithMany(u => u.Joins)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ถ้าลบ Post จะลบ Join ทั้งหมดของ Post นั้น (Cascade)
            modelBuilder.Entity<Join>()
                .HasOne(p => p.Post)
                .WithMany(p => p.Joins)
                .HasForeignKey(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            // ถ้าลบ User จะลบ Notification ทั้งหมดของ user คนนั้น (Cascade)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem → Product (1:N)
            // ถ้าสินค้าถูกลบ จะ ไม่ให้ลบ OrderItem ที่อ้างถึง (Restrict)
            // modelBuilder.Entity<OrderItem>()
            //     .HasOne(oi => oi.Product)
            //     .WithMany(p => p.OrderItems)
            //     .HasForeignKey(oi => oi.ProductId)
            //     .OnDelete(DeleteBehavior.Restrict);


            // // Configure decimal precision
            // modelBuilder.Entity<Product>()
            //     .Property(p => p.Price)
            //     .HasColumnType("decimal(18,2)");

            // modelBuilder.Entity<Order>()
            //     .Property(o => o.TotalAmount)
            //     .HasColumnType("decimal(18,2)");

            // modelBuilder.Entity<OrderItem>()
            //     .Property(oi => oi.UnitPrice)
            //     .HasColumnType("decimal(18,2)");


            // Seed data
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    CategoryId = 1,
                    Name = "ทำความสะอาด",
                    IsActive = true
                },
                new Category
                {
                    CategoryId = 2,
                    Name = "ทำบุญ",
                    IsActive = true
                },
                new Category
                {
                    CategoryId = 3,
                    Name = "บริจาคของ",
                    IsActive = true
                },
                new Category
                {
                    CategoryId = 4,
                    Name = "มิตรสิ่งแวดล้อม",
                    IsActive = true
                },
                new Category
                {
                    CategoryId = 5,
                    Name = "อื่นๆ",
                    IsActive = true
                }
            );
        }
    }
}