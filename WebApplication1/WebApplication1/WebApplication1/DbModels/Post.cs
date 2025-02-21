using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DbModels
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Game { get; set; }
        public int BlogId { get; set; }
        public bool Verify { get; set; }
    }
}
