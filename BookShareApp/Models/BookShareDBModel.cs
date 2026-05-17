using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace BookShareApp
{
    public class BookShareDBEntities : DbContext
    {
        public BookShareDBEntities() : base("name=BookShareDBEntities") { }

        public virtual DbSet<Role>          Roles          { get; set; }
        public virtual DbSet<User>          Users          { get; set; }
        public virtual DbSet<Genre>         Genres         { get; set; }
        public virtual DbSet<Book>          Books          { get; set; }
        public virtual DbSet<Review>        Reviews        { get; set; }
        public virtual DbSet<UserBookList>  UserBookLists  { get; set; }
        public virtual DbSet<Complaint>     Complaints     { get; set; }
        public virtual DbSet<AuthorRequest> AuthorRequests { get; set; }
        public virtual DbSet<FreezeAppeal>  FreezeAppeals  { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Связь Book - Author (User) по полю AuthorId
            modelBuilder.Entity<Book>()
                .HasRequired(b => b.Author)
                .WithMany(u => u.Books)
                .HasForeignKey(b => b.AuthorId)
                .WillCascadeOnDelete(false);

            // Связь Complaint - Reporter (User) по полю ReporterId
            modelBuilder.Entity<Complaint>()
                .HasRequired(c => c.Reporter)
                .WithMany()
                .HasForeignKey(c => c.ReporterId)
                .WillCascadeOnDelete(false);

            // Many-to-many: Book <-> Genre через таблицу BookGenres
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Genres)
                .WithMany(g => g.Books)
                .Map(m =>
                {
                    m.ToTable("BookGenres");
                    m.MapLeftKey("BookId");
                    m.MapRightKey("GenreId");
                });

            base.OnModelCreating(modelBuilder);
        }
    }


    [Table("Roles")]
    public class Role
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }

    [Table("Users")]
    public class User
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Login { get; set; }
        [Required, StringLength(255)]
        public string Password { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        [Required, StringLength(200)]
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public bool IsFrozen { get; set; }
        [StringLength(500)]
        public string FreezeReason { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<Book>   Books   { get; set; } 
        public virtual ICollection<Review> Reviews { get; set; }

        [NotMapped] public string RoleName => Role?.Name ?? "";
        [NotMapped] public bool IsAdmin  => RoleId == 3;
        [NotMapped] public bool IsAuthor => RoleId == 2 || RoleId == 3;
    }

    [Table("Genres")]
    public class Genre
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public virtual ICollection<Book> Books { get; set; }
    }

    [Table("Books")]
    public class Book
    {
        public int Id { get; set; }
        [Required, StringLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }
        [StringLength(500)]
        public string CoverPath { get; set; }
        public string Content { get; set; }
        public int AuthorId { get; set; }
        public bool IsFrozen { get; set; }
        [StringLength(500)]
        public string FreezeReason { get; set; }

        public virtual User Author { get; set; }
        public virtual ICollection<Genre>  Genres  { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        [NotMapped] public string AuthorName => Author?.FullName ?? "";

        [NotMapped]
        public string GenresText =>
            (Genres != null && Genres.Any())
                ? string.Join(", ", Genres.Select(g => g.Name))
                : "—";

        [NotMapped]
        public double AverageRating
        {
            get
            {
                if (Reviews == null) return 0;
                var rs = Reviews.Where(r => !r.IsFrozen).ToList();
                if (rs.Count == 0) return 0;
                return rs.Average(r => r.Rating);
            }
        }

        [NotMapped]
        public int ReviewsCount =>
            Reviews == null ? 0 : Reviews.Count(r => !r.IsFrozen);

        [NotMapped]
        public string RatingText =>
            ReviewsCount > 0
                ? $"Оценка: {AverageRating:F1} из 5 ({ReviewsCount})"
                : "Оценок пока нет";
    }

    [Table("Reviews")]
    public class Review
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public bool IsFrozen { get; set; }

        public virtual Book Book { get; set; }
        public virtual User User { get; set; }

        [NotMapped] public string UserName  => User?.FullName ?? "";
        [NotMapped] public string BookTitle => Book?.Title    ?? "";
        [NotMapped] public string Stars     => "Оценка: " + Rating + " из 5";
    }

    [Table("UserBookLists")]
    public class UserBookList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        [Required, StringLength(20)]
        public string ListType { get; set; }

        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }

    public static class BookListTypes
    {
        public const string Planned   = "Planned";
        public const string Reading   = "Reading";
        public const string Read      = "Read";
        public const string Abandoned = "Abandoned";

        public static string ToRussian(string s)
        {
            switch (s)
            {
                case Planned:   return "В планах";
                case Reading:   return "Читаю";
                case Read:      return "Прочитано";
                case Abandoned: return "Заброшено";
                default:        return s;
            }
        }
    }

    [Table("Complaints")]
    public class Complaint
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        [Required, StringLength(20)]
        public string TargetType { get; set; }
        public int TargetId { get; set; }
        [StringLength(1000)]
        public string Reason { get; set; }
        [Required, StringLength(20)]
        public string Status { get; set; }

        public virtual User Reporter { get; set; }

        [NotMapped] public string ReporterName => Reporter?.FullName ?? "";
        [NotMapped] public string TargetTitle { get; set; } 
        [NotMapped]
        public string TargetTypeRus
        {
            get
            {
                switch (TargetType)
                {
                    case "Book":   return "Книга";
                    case "Review": return "Отзыв";
                    case "Author": return "Автор";
                    default:       return TargetType;
                }
            }
        }
    }

    [Table("AuthorRequests")]
    public class AuthorRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required, StringLength(20)]
        public string Status { get; set; }

        public virtual User User { get; set; }

        [NotMapped] public string UserName  => User?.FullName ?? "";
        [NotMapped] public string UserEmail => User?.Email    ?? "";
    }

    [Table("FreezeAppeals")]
    public class FreezeAppeal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required, StringLength(20)]
        public string TargetType { get; set; }
        public int TargetId { get; set; }
        [StringLength(1000)]
        public string Reason { get; set; }
        [Required, StringLength(20)]
        public string Status { get; set; }

        public virtual User User { get; set; }

        [NotMapped] public string UserName    => User?.FullName ?? "";
        [NotMapped] public string TargetTitle { get; set; } 
        [NotMapped] public string TargetTypeRus => TargetType == "Book" ? "Книга" : "Пользователь";
    }
}
