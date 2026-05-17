using System.Windows;

namespace BookShareApp.Windows
{
    public partial class SimpleInputDialog : Window
    {
        public string InputText { get; private set; }

        public SimpleInputDialog(string title, string message, string defaultValue = "")
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
            InputBox.Text = defaultValue ?? "";
            InputBox.Focus();
            InputBox.SelectAll();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputBox.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
