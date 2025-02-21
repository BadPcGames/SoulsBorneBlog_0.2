namespace WebApplication1.DbModels
{
    public class Reactions
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int AuthorId { get; set; }
        public int PostId { get; set; }
    }
}
