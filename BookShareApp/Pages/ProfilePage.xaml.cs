using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BookShareApp.Windows;

namespace BookShareApp.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            var u = Core.CurrentUser;
            if (u == null) return;

            // Перезагружаем пользователя из БД (на случай свежей заморозки и т.п.)
            var fresh = Core.Context.Users
                .Include(x => x.Role)
                .FirstOrDefault(x => x.Id == u.Id);
            if (fresh != null) { Core.CurrentUser = fresh; u = fresh; }

            FullNameText.Text = u.FullName;
            LoginText.Text    = "Логин: " + u.Login;
            EmailText.Text    = "Email: " + (string.IsNullOrEmpty(u.Email) ? "—" : u.Email);
            RoleText.Text     = "Роль: " + u.RoleName;

            if (u.IsFrozen)
            {
                FrozenPanel.Visibility = Visibility.Visible;
                FrozenReasonText.Text = "Причина: " +
                    (string.IsNullOrEmpty(u.FreezeReason) ? "не указана" : u.FreezeReason);
            }
            else
            {
                FrozenPanel.Visibility = Visibility.Collapsed;
            }

            if (u.RoleId == 1 && !u.IsFrozen)
            {
                AuthorRequestPanel.Visibility = Visibility.Visible;
                bool hasPending = Core.Context.AuthorRequests
                    .Any(r => r.UserId == u.Id && r.Status == "Pending");
                if (hasPending)
                {
                    AuthorRequestButton.Content = "Заявка уже отправлена";
                    AuthorRequestButton.IsEnabled = false;
                }
                else
                {
                    AuthorRequestButton.Content = "Подать заявку";
                    AuthorRequestButton.IsEnabled = true;
                }
            }
            else
            {
                AuthorRequestPanel.Visibility = Visibility.Collapsed;
            }

            // Список отзывов пользователя
            var reviews = Core.Context.Reviews
                .Include(r => r.Book)
                .Where(r => r.UserId == u.Id)
                .ToList();
            ReviewsList.ItemsSource = reviews;
            NoReviewsText.Visibility = (reviews.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AuthorRequest_Click(object sender, RoutedEventArgs e)
        {
            Core.Context.AuthorRequests.Add(new AuthorRequest
            {
                UserId = Core.CurrentUser.Id,
                Status = "Pending"
            });
            Core.Context.SaveChanges();

            MessageBox.Show("Заявка отправлена. Дождитесь рассмотрения администратором.",
                            "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            Refresh();
        }

        private void Appeal_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SimpleInputDialog("Апелляция",
                "Опишите, почему вы считаете заморозку несправедливой:");
            if (dlg.ShowDialog() == true)
            {
                var text = dlg.InputText?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    MessageBox.Show("Текст апелляции не может быть пустым.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Core.Context.FreezeAppeals.Add(new FreezeAppeal
                {
                    UserId     = Core.CurrentUser.Id,
                    TargetType = "User",
                    TargetId   = Core.CurrentUser.Id,
                    Reason     = text,
                    Status     = "Pending"
                });
                Core.Context.SaveChanges();
                MessageBox.Show("Апелляция отправлена.", "Готово",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
