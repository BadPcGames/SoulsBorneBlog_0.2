using WebApplication1.DbModels;

namespace WebApplication1.Models
{
    public class PostContentViewModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string ContentType { get; set; } 
        public string Content { get; set; }    
        public byte[] ContentData { get; set; } 
        public IFormFile FormFile { get; set; } 
        public int Position {  get; set; }
    }
}
