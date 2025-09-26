using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace DemoVolunteer.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        
        public string? Name { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}