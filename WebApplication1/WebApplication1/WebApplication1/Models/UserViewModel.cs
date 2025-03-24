namespace WebApplication1.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public byte[] Avatar { get; set; }
        public DateTime? BanTime { get; set; }
        public int Warnings {  get; set; }
    }
}
