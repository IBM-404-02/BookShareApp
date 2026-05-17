using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BookShareApp.Pages
{
    public partial class CatalogPage : Page
    {
        private List<Book> _allBooks;

        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadBooks();
        }

        private void LoadGenres()
        {
            // Получаем список всех жанров из БД
            var genres = Core.Context.Genres.OrderBy(g => g.Name).ToList();
            genres.Insert(0, new Genre { Id = 0, Name = "Все жанры" });
            GenreFilter.ItemsSource = genres;
            GenreFilter.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            // Запрос с подгрузкой связанных таблиц (Автор, Жанры, Отзывы)
            _allBooks = Core.Context.Books
                .Include(b => b.Author)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .Where(b => !b.IsFrozen)
                .ToList();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allBooks == null) return;

            IEnumerable<Book> q = _allBooks;

            // Поиск по названию / автору
            var search = SearchBox?.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                q = q.Where(b =>
                    (b.Title ?? "").ToLower().Contains(search) ||
                    (b.AuthorName ?? "").ToLower().Contains(search));
            }

            // Фильтр по жанру
            var selectedGenre = GenreFilter?.SelectedItem as Genre;
            if (selectedGenre != null && selectedGenre.Id != 0)
                q = q.Where(b => b.Genres != null && b.Genres.Any(g => g.Id == selectedGenre.Id));

            // Сортировка
            if (SortBox?.SelectedIndex == 1)
                q = q.OrderByDescending(b => b.AverageRating).ThenBy(b => b.Title);
            else
                q = q.OrderBy(b => b.Title);

            BooksGrid.ItemsSource = q.ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void OpenBook_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            Core.MainFrame.Navigate(new BookPage(bookId));
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            int bookId = (int)((Button)sender).Tag;
            var menu = new ContextMenu();
            string[] types = { BookListTypes.Planned, BookListTypes.Reading,
                               BookListTypes.Read, BookListTypes.Abandoned };
            foreach (var t in types)
            {
                var item = new MenuItem { Header = BookListTypes.ToRussian(t), Tag = t };
                item.Click += (s, ev) =>
                {
                    AddOrMoveToList(bookId, (string)((MenuItem)s).Tag);
                    MessageBox.Show("Книга добавлена в список «" +
                                    BookListTypes.ToRussian((string)((MenuItem)s).Tag) + "»",
                                    "Готово");
                };
                menu.Items.Add(item);
            }
            menu.IsOpen = true;
        }

        private void AddOrMoveToList(int bookId, string listType)
        {
            // Удаляем существующую запись (если есть)
            var existing = Core.Context.UserBookLists
                .FirstOrDefault(x => x.UserId == Core.CurrentUser.Id && x.BookId == bookId);
            if (existing != null)
                Core.Context.UserBookLists.Remove(existing);

            // Добавляем новую запись со списком listType
            Core.Context.UserBookLists.Add(new UserBookList
            {
                UserId = Core.CurrentUser.Id,
                BookId = bookId,
                ListType = listType
            });
            Core.Context.SaveChanges();
        }
    }
}
