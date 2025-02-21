using WebApplication1.DbModels;

namespace WebApplication1.Models
{
    public class PostViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreateAt{ get; set; }
        public string Game{ get; set; }
        public string Color {  get; set; }
        public int BlogId { get; set; }
        public string BlogName { get; set;}
        public string AuthorName { get; set;}
        public bool Verify { get; set; }
        public List<Post_Content> Contents { get; set; }
    }
}
