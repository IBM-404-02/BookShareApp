using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace BookShareApp.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginBox.Text?.Trim() ?? "";
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем пользователя из БД через Entity Framework
                var user = Core.Context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Core.CurrentUser = user;
                new MainWindow().Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к базе данных:\n" + ex.Message,
                                "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow().ShowDialog();
        }
    }
}
