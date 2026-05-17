using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BookShareApp.Windows;

namespace BookShareApp.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            RefreshAll();
        }

        private void RefreshAll()
        {
            // ЖАЛОБЫ
            var complaints = Core.Context.Complaints
                .Include(c => c.Reporter)
                .Where(c => c.Status == "Pending")
                .ToList();
            // подгружаем "название цели" вручную (не хранится в БД)
            foreach (var c in complaints)
            {
                c.TargetTitle = GetTargetTitle(c.TargetType, c.TargetId);
            }
            ComplaintsList.ItemsSource = complaints;

            // АПЕЛЛЯЦИИ
            var appeals = Core.Context.FreezeAppeals
                .Include(a => a.User)
                .Where(a => a.Status == "Pending")
                .ToList();
            foreach (var a in appeals)
            {
                a.TargetTitle = (a.TargetType == "Book")
                    ? (Core.Context.Books.Where(b => b.Id == a.TargetId).Select(b => b.Title).FirstOrDefault() ?? "")
                    : (Core.Context.Users.Where(u => u.Id == a.TargetId).Select(u => u.FullName).FirstOrDefault() ?? "");
            }
            AppealsList.ItemsSource = appeals;

            // ЗАЯВКИ НА АВТОРА
            var authorReqs = Core.Context.AuthorRequests
                .Include(r => r.User)
                .Where(r => r.Status == "Pending")
                .ToList();
            AuthorRequestsList.ItemsSource = authorReqs;

            // ЗАМОРОЖЕНО — пользователи
            FrozenUsersList.ItemsSource = Core.Context.Users
                .Include(u => u.Role)
                .Where(u => u.IsFrozen)
                .ToList();

            // ЗАМОРОЖЕНО — книги
            FrozenBooksList.ItemsSource = Core.Context.Books
                .Include(b => b.Author)
                .Where(b => b.IsFrozen)
                .ToList();

            // ВСЕ ПОЛЬЗОВАТЕЛИ
            UsersList.ItemsSource = Core.Context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToList();
        }

        private string GetTargetTitle(string type, int id)
        {
            switch (type)
            {
                case "Book":
                    return Core.Context.Books.Where(b => b.Id == id).Select(b => b.Title).FirstOrDefault() ?? "";
                case "Review":
                    return "Отзыв №" + id;
                case "Author":
                    return Core.Context.Users.Where(u => u.Id == id).Select(u => u.FullName).FirstOrDefault() ?? "";
                default:
                    return "";
            }
        }

        // ============== ЖАЛОБЫ ==============
        private void AcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var c = Core.Context.Complaints.FirstOrDefault(x => x.Id == id);
            if (c == null) return;

            var dlg = new SimpleInputDialog("Причина заморозки",
                "Укажите причину заморозки (увидит автор/пользователь):");
            if (dlg.ShowDialog() != true) return;
            var reason = string.IsNullOrWhiteSpace(dlg.InputText)
                ? "Нарушение правил" : dlg.InputText.Trim();

            // Замораживаем цель жалобы
            switch (c.TargetType)
            {
                case "Book":
                    var book = Core.Context.Books.FirstOrDefault(b => b.Id == c.TargetId);
                    if (book != null) { book.IsFrozen = true; book.FreezeReason = reason; }
                    break;
                case "Review":
                    var rev = Core.Context.Reviews.FirstOrDefault(r => r.Id == c.TargetId);
                    if (rev != null) { rev.IsFrozen = true; }
                    break;
                case "Author":
                    var usr = Core.Context.Users.FirstOrDefault(u => u.Id == c.TargetId);
                    if (usr != null) { usr.IsFrozen = true; usr.FreezeReason = reason; }
                    break;
            }
            c.Status = "Accepted";
            Core.Context.SaveChanges();
            RefreshAll();
        }

        private void RejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var c = Core.Context.Complaints.FirstOrDefault(x => x.Id == id);
            if (c == null) return;
            c.Status = "Rejected";
            Core.Context.SaveChanges();
            RefreshAll();
        }

        // ============== АПЕЛЛЯЦИИ ==============
        private void AcceptAppeal_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var a = Core.Context.FreezeAppeals.FirstOrDefault(x => x.Id == id);
            if (a == null) return;

            if (a.TargetType == "Book")
            {
                var book = Core.Context.Books.FirstOrDefault(b => b.Id == a.TargetId);
                if (book != null) { book.IsFrozen = false; book.FreezeReason = null; }
            }
            else
            {
                var usr = Core.Context.Users.FirstOrDefault(u => u.Id == a.TargetId);
                if (usr != null) { usr.IsFrozen = false; usr.FreezeReason = null; }
            }
            a.Status = "Accepted";
            Core.Context.SaveChanges();
            RefreshAll();
        }

        private void RejectAppeal_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var a = Core.Context.FreezeAppeals.FirstOrDefault(x => x.Id == id);
            if (a == null) return;
            a.Status = "Rejected";
            Core.Context.SaveChanges();
            RefreshAll();
        }

        // ============== ЗАЯВКИ НА АВТОРА ==============
        private void AcceptAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var r = Core.Context.AuthorRequests.FirstOrDefault(x => x.Id == id);
            if (r == null) return;

            var usr = Core.Context.Users.FirstOrDefault(u => u.Id == r.UserId);
            if (usr != null) usr.RoleId = 2; // Автор
            r.Status = "Accepted";
            Core.Context.SaveChanges();
            RefreshAll();
        }

        private void RejectAuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var r = Core.Context.AuthorRequests.FirstOrDefault(x => x.Id == id);
            if (r == null) return;
            r.Status = "Rejected";
            Core.Context.SaveChanges();
            RefreshAll();
        }

        // ============== ЗАМОРОЖЕНО ==============
        private void UnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var u = Core.Context.Users.FirstOrDefault(x => x.Id == id);
            if (u == null) return;
            u.IsFrozen = false;
            u.FreezeReason = null;
            Core.Context.SaveChanges();
            RefreshAll();
        }

        private void UnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var b = Core.Context.Books.FirstOrDefault(x => x.Id == id);
            if (b == null) return;
            b.IsFrozen = false;
            b.FreezeReason = null;
            Core.Context.SaveChanges();
            RefreshAll();
        }

        // ============== ПОЛЬЗОВАТЕЛИ ==============
        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            var menu = new ContextMenu();

            var roles = new[]
            {
                new { Id = 1, Name = "Пользователь" },
                new { Id = 2, Name = "Автор"        },
                new { Id = 3, Name = "Администратор"}
            };
            foreach (var role in roles)
            {
                var item = new MenuItem { Header = role.Name, Tag = role.Id };
                int capturedRoleId = role.Id;
                item.Click += (s, ev) =>
                {
                    var u = Core.Context.Users.FirstOrDefault(x => x.Id == userId);
                    if (u != null)
                    {
                        u.RoleId = capturedRoleId;
                        Core.Context.SaveChanges();
                    }
                    RefreshAll();
                };
                menu.Items.Add(item);
            }
            menu.IsOpen = true;
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;
            var dlg = new SimpleInputDialog("Сброс пароля",
                "Введите новый пароль для пользователя:");
            if (dlg.ShowDialog() == true)
            {
                var pwd = dlg.InputText?.Trim();
                if (string.IsNullOrEmpty(pwd))
                {
                    MessageBox.Show("Пароль не может быть пустым.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var u = Core.Context.Users.FirstOrDefault(x => x.Id == userId);
                if (u != null)
                {
                    u.Password = pwd;
                    Core.Context.SaveChanges();
                }
                MessageBox.Show("Пароль изменён.", "Готово",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
