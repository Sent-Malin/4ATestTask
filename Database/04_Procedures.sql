USE HomeLibrary;
GO

--Включаем идентификацию кавычек для работы с xml
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.Book_Insert
    @Title           NVARCHAR(300),
    @Author          NVARCHAR(200),
    @PublishYear     SMALLINT,
    @Description     NVARCHAR(MAX) = NULL,
    @TableOfContents XML = NULL
AS
BEGIN
    --Оптимизация ответа
    SET NOCOUNT ON;

    INSERT INTO dbo.Books (Title, Author, PublishYear, Description, TableOfContents)
    VALUES (@Title, @Author, @PublishYear, @Description, @TableOfContents);
    --Возврат сгенерированного identity для записи о книге
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Book_Update
    @Id              INT,
    @Title           NVARCHAR(300),
    @Author          NVARCHAR(200),
    @PublishYear     SMALLINT,
    @Description     NVARCHAR(MAX) = NULL,
    @TableOfContents XML = NULL
AS
BEGIN
    UPDATE dbo.Books
    SET    Title           = @Title,
           Author          = @Author,
           PublishYear     = @PublishYear,
           Description     = @Description,
           TableOfContents = @TableOfContents
    WHERE  Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Book_Delete
    @Id INT
AS
BEGIN
    DELETE FROM dbo.Books WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Book_GetById
    @Id INT
AS
BEGIN
    --Оптимизация ответа
    SET NOCOUNT ON;

    SELECT Id, Title, Author, PublishYear, Description, TableOfContents
    FROM   dbo.Books
    WHERE  Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.Book_GetList
    @SearchText NVARCHAR(200) = NULL
AS
BEGIN
    --Оптимизация ответа
    SET NOCOUNT ON;

    DECLARE @lower NVARCHAR(200) = LOWER(@SearchText);

    SELECT Id, Title, Author, PublishYear
    FROM   dbo.Books
    WHERE  @SearchText IS NULL --Верхние условия вернут все книги, если строка поиска пустая
        OR @SearchText = N''
        OR CHARINDEX(@SearchText, Title)  > 0
        OR CHARINDEX(@SearchText, Author) > 0
        OR TableOfContents.exist(
               '/toc//chapter[contains(lower-case(@title), sql:variable("@lower"))]') = 1
    ORDER BY Title;
END
GO