using System.Windows;
using System.Windows.Controls;

namespace BookShareApp.Pages
{
    public partial class FrozenWarningPage : Page
    {
        public FrozenWarningPage()
        {
            InitializeComponent();
            var u = Core.CurrentUser;
            if (u != null)
            {
                ReasonText.Text = string.IsNullOrEmpty(u.FreezeReason)
                    ? "Причина не указана."
                    : u.FreezeReason;
            }
        }

        private void Appeal_Click(object sender, RoutedEventArgs e)
        {
            var text = AppealBox.Text?.Trim();
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

            MessageBox.Show("Апелляция отправлена. Дождитесь ответа администратора.",
                            "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            AppealBox.Text = "";
        }
    }
}
