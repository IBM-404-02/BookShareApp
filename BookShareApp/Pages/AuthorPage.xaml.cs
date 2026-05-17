using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BookShareApp.Windows;

namespace BookShareApp.Pages
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            var u = Core.CurrentUser;
            if (u == null) return;

            var all = Core.Context.Books
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .Where(b => b.AuthorId == u.Id)
                .ToList();

            var active = all.Where(b => !b.IsFrozen).ToList();
            var frozen = all.Where(b =>  b.IsFrozen).ToList();

            ActiveBooksList.ItemsSource = active;
            FrozenBooksList.ItemsSource = frozen;
            FrozenHeader.Visibility = frozen.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            Core.MainFrame.Navigate(new AddEditBookPage(null));
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            Core.MainFrame.Navigate(new AddEditBookPage(bookId));
        }

        private void AppealBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            var dlg = new SimpleInputDialog("Апелляция по книге",
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
                    TargetType = "Book",
                    TargetId   = bookId,
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
