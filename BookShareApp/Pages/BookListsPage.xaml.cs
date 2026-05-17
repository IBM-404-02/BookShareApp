using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BookShareApp.Pages
{
    public partial class BookListsPage : Page
    {
        private List<Book> _allBooks;
        private string _currentList = BookListTypes.Reading;

        public BookListsPage()
        {
            InitializeComponent();
            LoadGenres();
        }

        private void LoadGenres()
        {
            var genres = Core.Context.Genres.OrderBy(g => g.Name).ToList();
            genres.Insert(0, new Genre { Id = 0, Name = "Все жанры" });
            GenreFilter.ItemsSource = genres;
            GenreFilter.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            if (Core.CurrentUser == null) return;

            // Запрашиваем книги, попавшие в выбранный список текущего пользователя
            _allBooks = Core.Context.UserBookLists
                .Include(ubl => ubl.Book.Author)
                .Include(ubl => ubl.Book.Genres)
                .Include(ubl => ubl.Book.Reviews)
                .Where(ubl => ubl.UserId == Core.CurrentUser.Id && ubl.ListType == _currentList)
                .Select(ubl => ubl.Book)
                .ToList();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allBooks == null) return;

            IEnumerable<Book> q = _allBooks;

            var search = SearchBox?.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                q = q.Where(b =>
                    (b.Title ?? "").ToLower().Contains(search) ||
                    (b.AuthorName ?? "").ToLower().Contains(search));
            }

            var selectedGenre = GenreFilter?.SelectedItem as Genre;
            if (selectedGenre != null && selectedGenre.Id != 0)
                q = q.Where(b => b.Genres != null && b.Genres.Any(g => g.Id == selectedGenre.Id));

            if (SortBox?.SelectedIndex == 1)
                q = q.OrderByDescending(b => b.AverageRating).ThenBy(b => b.Title);
            else
                q = q.OrderBy(b => b.Title);

            BooksGrid.ItemsSource = q.ToList();
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb == null) return;
            _currentList = rb.Tag as string ?? BookListTypes.Reading;
            LoadBooks();
        }

        private void Filter_Changed(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void ComboFilter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void OpenBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            Core.MainFrame.Navigate(new BookPage(bookId));
        }

        private void MoveBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            var menu = new ContextMenu();
            string[] types = { BookListTypes.Planned, BookListTypes.Reading,
                               BookListTypes.Read, BookListTypes.Abandoned };
            foreach (var t in types)
            {
                if (t == _currentList) continue;
                var item = new MenuItem { Header = BookListTypes.ToRussian(t), Tag = t };
                item.Click += (s, ev) =>
                {
                    var entry = Core.Context.UserBookLists
                        .FirstOrDefault(x => x.UserId == Core.CurrentUser.Id && x.BookId == bookId);
                    if (entry != null)
                    {
                        entry.ListType = (string)((MenuItem)s).Tag;
                        Core.Context.SaveChanges();
                    }
                    LoadBooks();
                };
                menu.Items.Add(item);
            }
            menu.IsOpen = true;
        }

        private void RemoveBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            if (MessageBox.Show("Убрать книгу из списка?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var entry = Core.Context.UserBookLists
                .FirstOrDefault(x => x.UserId == Core.CurrentUser.Id && x.BookId == bookId);
            if (entry != null)
            {
                Core.Context.UserBookLists.Remove(entry);
                Core.Context.SaveChanges();
            }
            LoadBooks();
        }
    }
}
