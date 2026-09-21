using HtmlAgilityPack;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace HomeLibrary.Web.Services;

public interface ITocConverter
{
    /// <summary>HTML из редактора (вложенные списки) → XML оглавления. Пусто → null.</summary>
    string? HtmlToXml(string? html);

    /// <summary>XML оглавления → HTML для редактора и просмотра.</summary>
    string XmlToHtml(string? xml);
}

public partial class TocConverter : ITocConverter
{
    // "Название ..... 17"
    [GeneratedRegex(@"^(?<title>.+?)\s*\.{2,}\s*(?<page>\d{1,5})$")]
    private static partial Regex TitleWithPage();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    public string? HtmlToXml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var hasTextOutsideList = document.DocumentNode.ChildNodes
            .Where(n => !IsList(n))
            .Any(n => WebUtility.HtmlDecode(n.InnerText).Trim().Length > 0);

        if (hasTextOutsideList)
            throw new FormatException(
                "Оглавление должно быть списком: каждая глава - отдельный пункт списка.");

        var toc = new XElement("toc");
        foreach (var list in document.DocumentNode.ChildNodes.Where(IsList))
            toc.Add(ReadList(list));

        return toc.HasElements ? toc.ToString(SaveOptions.DisableFormatting) : null;
    }

    public string XmlToHtml(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return string.Empty;

        // DTD запрещён: файл приходит от пользователя
        using var reader = XmlReader.Create(
            new StringReader(xml),
            new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

        var root = XDocument.Load(reader).Root;
        if (root is null || root.Name.LocalName != "toc")
            throw new FormatException("Корневым элементом должен быть toc.");

        var html = new StringBuilder();
        WriteList(root, html);
        return html.ToString();
    }

    private static bool IsList(HtmlNode node) => node.Name is "ul" or "ol";

    private static IEnumerable<XElement> ReadList(HtmlNode list)
    {
        foreach (var item in list.ChildNodes.Where(n => n.Name == "li"))
        {
            var (title, page) = ParseTitle(item);
            var children = item.ChildNodes.Where(IsList).SelectMany(ReadList).ToList();

            // Пустой пункт пропускаем, а вложенные главы поднимаем на его уровень
            if (title.Length == 0)
            {
                foreach (var child in children)
                    yield return child;
                continue;
            }

            var chapter = new XElement("chapter", new XAttribute("title", title));
            if (page is not null)
                chapter.Add(new XAttribute("page", page.Value));
            chapter.Add(children);

            yield return chapter;
        }
    }

    private static (string Title, int? Page) ParseTitle(HtmlNode item)
    {
        // Собственный текст пункта, без вложенных списков
        var raw = string.Concat(item.ChildNodes.Where(n => !IsList(n)).Select(n => n.InnerText));
        var text = Whitespace().Replace(WebUtility.HtmlDecode(raw), " ").Trim();

        var match = TitleWithPage().Match(text);
        return match.Success
            ? (match.Groups["title"].Value, int.Parse(match.Groups["page"].Value))
            : (text, null);
    }

    private static void WriteList(XElement parent, StringBuilder html)
    {
        var chapters = parent.Elements("chapter").ToList();
        if (chapters.Count == 0)
            return;

        html.Append("<ul>");
        foreach (var chapter in chapters)
        {
            var title = (string?)chapter.Attribute("title")
                ?? throw new FormatException("У одной из глав нет атрибута title.");

            html.Append("<li>").Append(WebUtility.HtmlEncode(title));

            var page = (string?)chapter.Attribute("page");
            if (page is not null)
            {
                if (!int.TryParse(page, out _))
                    throw new FormatException($"Некорректный номер страницы: {page}.");
                html.Append(" ..... ").Append(page);
            }

            WriteList(chapter, html);
            html.Append("</li>");
        }
        html.Append("</ul>");
    }
}