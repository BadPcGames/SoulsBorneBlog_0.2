namespace WebApplication1.DbModels
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Theme { get; set; }
        public int AuthorId { get; set; }
    }
}
