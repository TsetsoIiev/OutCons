using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace OutCons.Web.Models
{
    public partial class OutConsContext : DbContext
    {
        private readonly IConfiguration _configuration;
        

        public OutConsContext(DbContextOptions<OutConsContext> options, IConfiguration configuration)
            : base(options)
        { 
            _configuration = configuration;
        }

        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<TimeLog> TimeLogs { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Project");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TimeLog>(entity =>
            {
                entity.ToTable("TimeLog");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.TimeLogs)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeLog_Project");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TimeLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TimeLog_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
