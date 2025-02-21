using WebApplication1.DbModels;

namespace WebApplication1.Models
{
    public class GameViewModel
    {
        public string GameName { get; set; }
        public string Description { get; set; }
        public byte[] GameCharacter { get; set; }
        public string Color { get; set; }
    }
}
