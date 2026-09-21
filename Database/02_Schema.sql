USE HomeLibrary;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.xml_schema_collections
               WHERE name = N'TableOfContentsSchema'
                 AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE XML SCHEMA COLLECTION dbo.TableOfContentsSchema AS N'
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" elementFormDefault="qualified">
      <xs:element name="toc">
        <xs:complexType>
          <xs:sequence>
            <xs:element name="chapter" type="chapterType" minOccurs="0" maxOccurs="unbounded"/>
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:complexType name="chapterType">
        <xs:sequence>
          <xs:element name="chapter" type="chapterType" minOccurs="0" maxOccurs="unbounded"/>
        </xs:sequence>
        <xs:attribute name="title" type="xs:string" use="required"/>
        <xs:attribute name="page"  type="xs:int"    use="optional"/>
      </xs:complexType>
    </xs:schema>';
END
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books
    (
        Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Books PRIMARY KEY CLUSTERED,
        Title           NVARCHAR(300)  NOT NULL,
        Author          NVARCHAR(200)  NOT NULL,
        PublishYear     SMALLINT       NOT NULL,
        Description     NVARCHAR(MAX)  NULL,
        TableOfContents XML(DOCUMENT dbo.TableOfContentsSchema) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'PXML_Books_TableOfContents'
               AND object_id = OBJECT_ID(N'dbo.Books'))
    CREATE PRIMARY XML INDEX PXML_Books_TableOfContents ON dbo.Books (TableOfContents);
GO
