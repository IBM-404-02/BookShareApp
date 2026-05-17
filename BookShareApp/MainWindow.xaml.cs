using System.Windows;
using BookShareApp.Pages;
using BookShareApp.Windows;

namespace BookShareApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Core.MainFrame = MainFrame;

            // Видимость кнопок по роли
            BtnAdmin.Visibility  = Core.CurrentUser.IsAdmin  ? Visibility.Visible : Visibility.Collapsed;
            BtnAuthor.Visibility = Core.CurrentUser.IsAuthor ? Visibility.Visible : Visibility.Collapsed;
            BtnFrozen.Visibility = Core.CurrentUser.IsFrozen ? Visibility.Visible : Visibility.Collapsed;

            // По умолчанию открываем каталог
            MainFrame.Navigate(new CatalogPage());
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e) =>
            MainFrame.Navigate(new CatalogPage());

        private void BtnLists_Click(object sender, RoutedEventArgs e) =>
            MainFrame.Navigate(new BookListsPage());

        private void BtnAdmin_Click(object sender, RoutedEventArgs e) =>
            MainFrame.Navigate(new AdminPage());

        private void BtnAuthor_Click(object sender, RoutedEventArgs e) =>
            MainFrame.Navigate(new AuthorPage());

        private void BtnFrozen_Click(object sender, RoutedEventArgs e) =>
            MainFrame.Navigate(new FrozenWarningPage());

        private void BtnProfile_Click(object sender, RoutedEventArgs e) =>
            MainFrame.Navigate(new ProfilePage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("Выйти из аккаунта?", "Подтверждение",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;

            Core.CurrentUser = null;
            new LoginWindow().Show();
            Close();
        }
    }
}
