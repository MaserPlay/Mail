using Microsoft.EntityFrameworkCore;

namespace Mail.Sqllite;

public class SqlContext : DbContext
{
    public SqlContext() : base(new DbContextOptionsBuilder<SqlContext>()
        .UseSqlite("Data Source=db.db")
        .Options)
    {
    }

    public SqlContext(DbContextOptions<SqlContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Folder> Folders { get; set; } = null!;
    public DbSet<Mail> Mails { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=db.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity => { entity.HasKey(e => e.Id); });
        modelBuilder.Entity<Folder>(entity => { entity.HasKey(e => e.Id); });
        modelBuilder.Entity<Mail>(entity => { entity.HasKey(e => e.Id); });
    }
}