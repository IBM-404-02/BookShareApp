using System.Windows.Controls;

namespace BookShareApp
{
    /// <summary>
    /// Глобальный контекст приложения. Хранит:
    ///   Core.Context     — единый экземпляр БД (EF6 DbContext)
    ///   Core.CurrentUser — текущий вошедший пользователь
    ///   Core.MainFrame   — Frame главного окна для навигации
    ///   List&lt;User&gt; users = Core.Context.Users.ToList();
    ///   Core.Context.Users.Add(newUser);
    ///   Core.Context.SaveChanges();
    /// </summary>
    public static class Core
    {
        public static BookShareDBEntities Context = new BookShareDBEntities();
        public static User CurrentUser { get; set; }
        public static Frame MainFrame { get; set; }
    }
}
