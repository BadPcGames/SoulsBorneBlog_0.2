using WebApplication1.DbModels;

namespace WebApplication1.Models
{
    public class StatsViewModel
    {
        public string Name { get; set; }
        public List<GameStatsViewModel> GameStats { get; set; }
        public List<ThemeStatsViewModel> ThemesStats { get; set; }
        public int PostCount { get; set; }
        public int VerifyPostCount { get; set; }
        public int NotVerifyPostCount { get; set; }
        public List<PostShortStatsViewModel> PostShortStats { get; set; }
    }

    public class GameStatsViewModel
    {
        public string Name { get; set; }
        public int PostsCount { get; set; }
    }

    public class ThemeStatsViewModel
    {
        public string Name { get; set; }
        public int PostsCount { get; set; }
    }

    public class PostShortStatsViewModel 
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public int LikeCount { get; set;}
        public int NotLikeCount { get; set;}
        public  int ComentsCount { get; set; }
        public bool Verify {  get; set; }
        public string BlogName { get; set; }
    }


}
