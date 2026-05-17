-- =========================================================
-- БД для приложения "Читай, Пиши и не спиши"
-- УП.01.01 + УП.02 (используется в обеих практиках)
-- =========================================================

-- Раскомментируйте две строки ниже, если БД ещё не создана:
-- CREATE DATABASE BookShareDB;
-- GO

USE BookShareDB;
GO

-- Если таблицы уже есть — удаляем (в правильном порядке)
IF OBJECT_ID('FreezeAppeals', 'U') IS NOT NULL DROP TABLE FreezeAppeals;
IF OBJECT_ID('AuthorRequests', 'U') IS NOT NULL DROP TABLE AuthorRequests;
IF OBJECT_ID('Complaints', 'U') IS NOT NULL DROP TABLE Complaints;
IF OBJECT_ID('UserBookLists', 'U') IS NOT NULL DROP TABLE UserBookLists;
IF OBJECT_ID('Reviews', 'U') IS NOT NULL DROP TABLE Reviews;
IF OBJECT_ID('BookGenres', 'U') IS NOT NULL DROP TABLE BookGenres;
IF OBJECT_ID('Books', 'U') IS NOT NULL DROP TABLE Books;
IF OBJECT_ID('Genres', 'U') IS NOT NULL DROP TABLE Genres;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;
GO

-- Роли пользователей
CREATE TABLE Roles (
    Id    INT PRIMARY KEY IDENTITY(1,1),
    Name  NVARCHAR(50) NOT NULL
);

INSERT INTO Roles (Name) VALUES (N'Пользователь'), (N'Автор'), (N'Администратор');
GO

-- Пользователи
CREATE TABLE Users (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    Login        NVARCHAR(50)  NOT NULL UNIQUE,
    Password     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(100) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    RoleId       INT NOT NULL FOREIGN KEY REFERENCES Roles(Id),
    IsFrozen     BIT NOT NULL DEFAULT 0,
    FreezeReason NVARCHAR(500) NULL
);

-- Стартовые пользователи (пароли в открытом виде для простоты учебной работы)
INSERT INTO Users (Login, Password, Email, FullName, RoleId)
VALUES
 (N'admin',  N'admin',  N'admin@test.ru',  N'Администратор Системы', 3),
 (N'author', N'author', N'author@test.ru', N'Иван Иванов',           2),
 (N'user',   N'user',   N'user@test.ru',   N'Пётр Петров',           1);
GO

-- Жанры
CREATE TABLE Genres (
    Id   INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO Genres (Name) VALUES
 (N'Фантастика'), (N'Детектив'), (N'Роман'),
 (N'Поэзия'),     (N'Фэнтези'),  (N'Приключения');
GO

-- Книги
CREATE TABLE Books (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    Title        NVARCHAR(200) NOT NULL,
    Description  NVARCHAR(MAX) NULL,
    CoverPath    NVARCHAR(500) NULL,
    Content      NVARCHAR(MAX) NULL,
    AuthorId     INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    IsFrozen     BIT NOT NULL DEFAULT 0,
    FreezeReason NVARCHAR(500) NULL
);

INSERT INTO Books (Title, Description, Content, AuthorId) VALUES
 (N'Тестовая книга 1', N'Описание первой тестовой книги. Очень интересная история.',
  N'Это содержимое первой главы тестовой книги. Здесь много текста для чтения...', 2),
 (N'Тестовая книга 2', N'Захватывающее произведение для всех возрастов.',
  N'Глава 1. Начало истории. Жил-был на свете...', 2);
GO

-- Связь книг и жанров (многие ко многим)
CREATE TABLE BookGenres (
    BookId  INT NOT NULL FOREIGN KEY REFERENCES Books(Id),
    GenreId INT NOT NULL FOREIGN KEY REFERENCES Genres(Id),
    PRIMARY KEY (BookId, GenreId)
);

INSERT INTO BookGenres (BookId, GenreId) VALUES (1, 1), (1, 5), (2, 3);
GO

-- Отзывы на книги
CREATE TABLE Reviews (
    Id       INT PRIMARY KEY IDENTITY(1,1),
    BookId   INT NOT NULL FOREIGN KEY REFERENCES Books(Id),
    UserId   INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Rating   INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Text     NVARCHAR(MAX) NULL,
    IsFrozen BIT NOT NULL DEFAULT 0
);

INSERT INTO Reviews (BookId, UserId, Rating, Text) VALUES
 (1, 3, 5, N'Очень понравилась книга, читал на одном дыхании!'),
 (2, 3, 4, N'Хорошая история, но местами затянуто.');
GO

-- Списки книг пользователей (Заброшено / В планах / Читаю / Прочитано)
CREATE TABLE UserBookLists (
    Id       INT PRIMARY KEY IDENTITY(1,1),
    UserId   INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    BookId   INT NOT NULL FOREIGN KEY REFERENCES Books(Id),
    ListType NVARCHAR(20) NOT NULL -- 'Abandoned' | 'Planned' | 'Reading' | 'Read'
);
GO

-- Жалобы (на книгу / отзыв / автора)
CREATE TABLE Complaints (
    Id         INT PRIMARY KEY IDENTITY(1,1),
    ReporterId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    TargetType NVARCHAR(20) NOT NULL, -- 'Book' | 'Review' | 'Author'
    TargetId   INT NOT NULL,
    Reason     NVARCHAR(500) NULL,
    Status     NVARCHAR(20) NOT NULL DEFAULT 'Pending' -- 'Pending' | 'Accepted' | 'Rejected'
);
GO

-- Заявки на роль автора
CREATE TABLE AuthorRequests (
    Id     INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending'
);
GO

-- Заявки на снятие заморозки (для пользователя или книги)
CREATE TABLE FreezeAppeals (
    Id         INT PRIMARY KEY IDENTITY(1,1),
    UserId     INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    TargetType NVARCHAR(20) NOT NULL, -- 'User' | 'Book'
    TargetId   INT NOT NULL,
    Reason     NVARCHAR(500) NULL,
    Status     NVARCHAR(20) NOT NULL DEFAULT 'Pending'
);
GO

PRINT N'База данных создана успешно!';
PRINT N'Тестовые учётные записи:';
PRINT N'  admin  / admin  — Администратор';
PRINT N'  author / author — Автор';
PRINT N'  user   / user   — Пользователь';
