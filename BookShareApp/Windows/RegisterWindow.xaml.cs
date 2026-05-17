using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace BookShareApp.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var fullName = FullNameBox.Text?.Trim() ?? "";
            var login    = LoginBox.Text?.Trim() ?? "";
            var email    = EmailBox.Text?.Trim() ?? "";
            var password = PasswordBox.Password;
            var password2 = PasswordBox2.Password;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(email)    || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (login.Length < 3)
            {
                MessageBox.Show("Логин должен содержать не менее 3 символов",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать не менее 4 символов",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != password2)
            {
                MessageBox.Show("Пароли не совпадают",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Некорректный e-mail",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем, что логин не занят
                if (Core.Context.Users.Any(u => u.Login == login))
                {
                    MessageBox.Show("Такой логин уже занят, выберите другой",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создаём нового пользователя с ролью 1 (Пользователь)
                var newUser = new User
                {
                    Login        = login,
                    Password     = password,
                    Email        = email,
                    FullName     = fullName,
                    RoleId       = 1,
                    IsFrozen     = false
                };

                Core.Context.Users.Add(newUser);   // добавление в таблицу
                Core.Context.SaveChanges();        // сохранение изменений в БД

                MessageBox.Show("Регистрация прошла успешно! Теперь войдите в систему.",
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
