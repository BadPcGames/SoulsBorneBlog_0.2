using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using WebApplication1.DbModels;

namespace WebApplication1;
public class AppDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Theme> Themes { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Post_Content> Post_Contents { get; set; }
    public DbSet<Coments> Coments { get; set; }
    public DbSet<Reactions> Reactions { get; set; }
    public DbSet<Baner> Baners { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>().ToTable("blogs");
        modelBuilder.Entity<Post>().ToTable("posts");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Baner>().ToTable("baners");
    }
}