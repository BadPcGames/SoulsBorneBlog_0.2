using WebApplication1.DbModels;

namespace WebApplication1.Models
{
    public class ComentViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int PostId { get; set; }
        public bool CanChange { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public byte[] AuthorAvatar { get; set; }
    }
}
