namespace WebApplication1.DbModels
{
    public class Game
    {
        public int Id { get; set; }
        public string GameName{ get; set; }
        public string Description { get; set; }
        public byte[] GameCharacter { get; set; }
        public string Color { get; set; }

    }
}
