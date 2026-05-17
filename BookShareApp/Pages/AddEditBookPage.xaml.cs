using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BookShareApp.Pages
{
    public partial class AddEditBookPage : Page
    {
        private readonly int? _bookId;
        private List<Genre> _allGenres;

        public AddEditBookPage(int? bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadGenres();

            if (_bookId.HasValue)
            {
                HeaderText.Text = "Редактирование книги";
                LoadBook(_bookId.Value);
            }
            else
            {
                HeaderText.Text = "Добавить новую книгу";
            }
        }

        private void LoadGenres()
        {
            _allGenres = Core.Context.Genres.OrderBy(g => g.Name).ToList();
            GenresList.ItemsSource = _allGenres;
        }

        private void LoadBook(int id)
        {
            var b = Core.Context.Books
                .Include(x => x.Genres)
                .FirstOrDefault(x => x.Id == id);
            if (b == null) return;

            TitleBox.Text       = b.Title;
            DescriptionBox.Text = b.Description;
            CoverPathBox.Text   = b.CoverPath;
            ContentBox.Text     = b.Content;

            // Отметим уже выбранные жанры
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                var selectedIds = b.Genres.Select(g => g.Id).ToList();
                foreach (var cb in FindCheckBoxes(GenresList))
                {
                    if (cb.Tag is int gid && selectedIds.Contains(gid))
                        cb.IsChecked = true;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private IEnumerable<CheckBox> FindCheckBoxes(DependencyObject root)
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is CheckBox cb) yield return cb;
                foreach (var sub in FindCheckBoxes(child)) yield return sub;
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите обложку",
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
                CoverPathBox.Text = dlg.FileName;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var title = TitleBox.Text?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Укажите название книги.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var description = DescriptionBox.Text?.Trim() ?? "";
            var coverPath   = CoverPathBox.Text?.Trim() ?? "";
            var content     = ContentBox.Text ?? "";

            var selectedGenreIds = FindCheckBoxes(GenresList)
                .Where(cb => cb.IsChecked == true && cb.Tag is int)
                .Select(cb => (int)cb.Tag)
                .ToList();

            Book book;
            if (_bookId.HasValue)
            {
                // Редактирование
                book = Core.Context.Books
                    .Include(b => b.Genres)
                    .FirstOrDefault(b => b.Id == _bookId.Value);
                if (book == null) return;

                book.Title       = title;
                book.Description = description;
                book.CoverPath   = coverPath;
                book.Content     = content;

                // Обновляем жанры
                book.Genres.Clear();
            }
            else
            {
                // Добавление
                book = new Book
                {
                    Title       = title,
                    Description = description,
                    CoverPath   = coverPath,
                    Content     = content,
                    AuthorId    = Core.CurrentUser.Id,
                    IsFrozen    = false,
                    Genres      = new List<Genre>()
                };
                Core.Context.Books.Add(book);
            }

            // Привязываем выбранные жанры
            var genresToLink = Core.Context.Genres.Where(g => selectedGenreIds.Contains(g.Id)).ToList();
            foreach (var g in genresToLink)
                book.Genres.Add(g);

            Core.Context.SaveChanges();

            MessageBox.Show("Книга сохранена.", "Готово",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            Core.MainFrame.Navigate(new AuthorPage());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Core.MainFrame.Navigate(new AuthorPage());
        }
    }
}
