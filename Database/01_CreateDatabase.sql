IF DB_ID(N'HomeLibrary') IS NULL
BEGIN
    CREATE DATABASE HomeLibrary COLLATE Cyrillic_General_CI_AS;
END
GO