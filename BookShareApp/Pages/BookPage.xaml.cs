using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BookShareApp.Windows;

namespace BookShareApp.Pages
{
    public partial class BookPage : Page
    {
        private Book _book;
        private bool _contentExpanded = false;

        public Visibility AdminVisibility =>
            (Core.CurrentUser != null && Core.CurrentUser.IsAdmin)
                ? Visibility.Visible : Visibility.Collapsed;

        public BookPage(int bookId)
        {
            InitializeComponent();
            DataContext = this;
            LoadBook(bookId);
        }

        private void LoadBook(int bookId)
        {
            _book = Core.Context.Books
                .Include(b => b.Author)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .FirstOrDefault(b => b.Id == bookId);

            if (_book == null)
            {
                MessageBox.Show("Книга не найдена", "Ошибка");
                Core.MainFrame.Navigate(new CatalogPage());
                return;
            }

            TitleText.Text       = _book.Title;
            AuthorText.Text      = "Автор: " + _book.AuthorName;
            GenresText.Text      = "Жанры: " + _book.GenresText;
            RatingText.Text      = _book.RatingText;
            DescriptionText.Text = string.IsNullOrEmpty(_book.Description) ? "—" : _book.Description;
            ContentText.Text     = string.IsNullOrEmpty(_book.Content) ? "(Содержимое отсутствует)" : _book.Content;

            // Панель заморозки
            if (_book.IsFrozen)
            {
                FrozenPanel.Visibility = Visibility.Visible;
                FrozenReasonText.Text = "Причина: " +
                    (string.IsNullOrEmpty(_book.FreezeReason) ? "не указана" : _book.FreezeReason);
            }
            else
            {
                FrozenPanel.Visibility = Visibility.Collapsed;
            }

            // Кнопка заморозки видна только админу
            FreezeBookBtn.Visibility = AdminVisibility;
            if (_book.IsFrozen) FreezeBookBtn.Content = "Разморозить книгу";

            LoadReviews();
        }

        private void LoadReviews()
        {
            bool showFrozen = Core.CurrentUser != null && Core.CurrentUser.IsAdmin;

            var reviews = Core.Context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == _book.Id && (showFrozen || !r.IsFrozen))
                .ToList();

            ReviewsList.ItemsSource = reviews;
        }

        // ------------- Действия -------------
        private void Back_Click(object sender, RoutedEventArgs e) =>
            Core.MainFrame.Navigate(new CatalogPage());

        private void ToggleContent_Click(object sender, RoutedEventArgs e)
        {
            _contentExpanded = !_contentExpanded;
            ContentScroll.MaxHeight = _contentExpanded ? double.PositiveInfinity : 300;
        }

        private void AddReview_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser.IsFrozen)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Публикация отзывов невозможна.",
                                "Ошибка");
                return;
            }

            var text = ReviewBox.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Введите текст отзыва.", "Ошибка");
                return;
            }

            int rating = (RatingBox.SelectedIndex >= 0) ? RatingBox.SelectedIndex + 1 : 5;

            var review = new Review
            {
                BookId   = _book.Id,
                UserId   = Core.CurrentUser.Id,
                Rating   = rating,
                Text     = text,
                IsFrozen = false
            };
            Core.Context.Reviews.Add(review);
            Core.Context.SaveChanges();

            ReviewBox.Text = "";
            LoadBook(_book.Id);   // обновить страницу
        }

        private void ComplainBook_Click(object sender, RoutedEventArgs e) =>
            AddComplaint("Book", _book.Id, "книгу");

        private void ComplainAuthor_Click(object sender, RoutedEventArgs e) =>
            AddComplaint("Author", _book.AuthorId, "автора");

        private void ComplainReview_Click(object sender, RoutedEventArgs e)
        {
            int reviewId = (int)((Button)sender).Tag;
            AddComplaint("Review", reviewId, "отзыв");
        }

        private void AddComplaint(string targetType, int targetId, string what)
        {
            var dlg = new SimpleInputDialog("Жалоба",
                "Опишите причину жалобы на " + what + ":");
            if (dlg.ShowDialog() != true) return;
            var reason = dlg.InputText?.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Введите причину.", "Ошибка");
                return;
            }

            Core.Context.Complaints.Add(new Complaint
            {
                ReporterId = Core.CurrentUser.Id,
                TargetType = targetType,
                TargetId   = targetId,
                Reason     = reason,
                Status     = "Pending"
            });
            Core.Context.SaveChanges();

            MessageBox.Show("Жалоба отправлена. Дождитесь рассмотрения администратором.",
                            "Готово");
        }

        // Только для админа
        private void FreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (_book.IsFrozen)
            {
                _book.IsFrozen = false;
                _book.FreezeReason = null;
                Core.Context.SaveChanges();
                LoadBook(_book.Id);
                return;
            }

            var dlg = new SimpleInputDialog("Заморозка книги",
                "Укажите причину заморозки:");
            if (dlg.ShowDialog() != true) return;
            var reason = dlg.InputText?.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("Причина не может быть пустой.", "Ошибка");
                return;
            }
            _book.IsFrozen = true;
            _book.FreezeReason = reason;
            Core.Context.SaveChanges();
            LoadBook(_book.Id);
        }

        private void FreezeReview_Click(object sender, RoutedEventArgs e)
        {
            int reviewId = (int)((Button)sender).Tag;
            var rev = Core.Context.Reviews.FirstOrDefault(r => r.Id == reviewId);
            if (rev == null) return;
            rev.IsFrozen = !rev.IsFrozen;
            Core.Context.SaveChanges();
            LoadReviews();
        }
    }
}
