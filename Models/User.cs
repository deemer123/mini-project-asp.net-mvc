using System.ComponentModel.DataAnnotations;

namespace DemoVolunteer.Models
{
    public class User
    {
        public int Id { get; set; }                             // id primery key

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;   // ชื่อ
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;    // นามสกุล
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;       // อีเมล
        
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty; // เบอร์โทร
        
        public DateTime CreatedDate { get; set; } = DateTime.Now; // เวลาที่สร้าง user

        public int Score { get; set; } = 0;                           // คะแนนจิตอาสา

        public bool IsActive { get; set; } = true;

        // Computed property
        public string FullName => $"{FirstName} {LastName}";

    }
}