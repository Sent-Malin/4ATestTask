USE HomeLibrary;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Books)
BEGIN
    INSERT INTO dbo.Books (Title, Author, PublishYear, Description, TableOfContents)
    VALUES
    (N'Clean Code', N'Robert C. Martin', 2008,
     N'Руководство по написанию чистого кода.',
     N'<toc>
         <chapter title="Clean Code" page="1"/>
         <chapter title="Meaningful Names" page="17"/>
         <chapter title="Functions" page="31"/>
       </toc>'),

    (N'Война и мир', N'Лев Толстой', 1869,
     N'Роман-эпопея.',
     N'<toc>
         <chapter title="Том первый">
           <chapter title="Часть первая" page="5"/>
           <chapter title="Часть вторая" page="120"/>
         </chapter>
         <chapter title="Том второй">
           <chapter title="Часть первая" page="10"/>
         </chapter>
       </toc>'),

    (N'Преступление и наказание', N'Фёдор Достоевский', 1866, NULL, NULL);
END
GO