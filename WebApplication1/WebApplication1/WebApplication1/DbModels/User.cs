namespace WebApplication1.DbModels
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public byte[] Avatar { get; set; }
        public DateTime? BanTime { get; set; }
        public int Warnings { get; set; }
    }
}
