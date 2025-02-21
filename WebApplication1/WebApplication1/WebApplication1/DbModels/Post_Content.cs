namespace WebApplication1.DbModels
{
    public class Post_Content
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string ContentType{ get; set; }
        public byte[] Content { get; set; }
        public int Position {  get; set; }
    }
}
