# Домашняя библиотека

Веб-приложение для учёта книг: список, карточка, создание, редактирование, удаление и поиск по названию, автору и оглавлению. Оглавление хранится в XML-поле MS SQL и редактируется в HTML-редакторе

## Стек

- ASP.NET Core MVC, .NET 10
- MS SQL Server 2022 (Docker), доступ к данным только через хранимые процедуры (Dapper)
- Оглавление: типизированный XML (`XML SCHEMA COLLECTION`), поиск через XQuery
- TinyMCE 7 (лежит в `wwwroot/lib/tinymce`)

## Запуск

Нужны Docker Desktop и .NET 10 SDK.

```powershell
docker compose up -d
docker compose logs db-init      # должно быть "Database is ready"
dotnet run --project src/HomeLibrary.Web --launch-profile https
```

Пароль SA по умолчанию `974915GTRy`. Для изменения можно создать `.env` файл с переменной при этом его нужно сменить также и в `ConnectionStrings:Library` в `appsettings.json`.

### Если свой SQL Server и нет Docker

Выполните скрипты из `Database/` по порядку:

```powershell
Get-ChildItem Database\*.sql | Sort-Object Name | ForEach-Object {
    sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -b -i $_.FullName
}
```

Затем задайте строку подключения без правки файлов:

```powershell
$env:ConnectionStrings__Library = "Server=(localdb)\MSSQLLocalDB;Database=HomeLibrary;Trusted_Connection=True;TrustServerCertificate=True"
```

## Описание решений

- **Хранимые процедуры.** Вся работа с данными идёт через них. `Book_GetList` с необязательным параметром поиска обслуживает и список, и поиск.
- **Оглавление.** Структурированный XML: `<toc><chapter title="..." page="..."/></toc>`, главы вкладываются друг в друга. Схема проверяется самой БД.
- **Редактор.** Оглавление редактируется как вложенный список, страница указывается так: `Название ..... 17`. На сервере `TocConverter` превращает HTML в XML и обратно. HTML в БД не попадает.
- **Поиск.** Название и автор через `CHARINDEX`, оглавление через XQuery `exist()` с `lower-case()` (без учёта регистра).
- **XML-файл.** В карточке можно скачать оглавление как XML, в форме редактирования загрузить его из файла.
- **DI.** Контроллер зависит от `IBookRepository` и `ITocConverter`, реализации регистрируются в `Program.cs`.
