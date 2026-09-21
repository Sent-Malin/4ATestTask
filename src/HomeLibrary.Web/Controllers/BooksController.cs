using HomeLibrary.Core.Abstractions;
using HomeLibrary.Web.Models;
using HomeLibrary.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace HomeLibrary.Web.Controllers
{
    public class BooksController : Controller
    {
        private const long MaxImportSize = 1024 * 1024;
        private readonly IBookRepository _books;
        private readonly ITocConverter _toc;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookRepository books, ITocConverter toc, ILogger<BooksController> logger)
        {
            _books = books;
            _toc = toc;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Search"] = search;
            return View(await _books.GetListAsync(search));
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _books.GetByIdAsync(id);
            if (book is null)
                return NotFound();

            ViewData["TocHtml"] = _toc.XmlToHtml(book.TableOfContents);
            return View(book);
        }

        public IActionResult Create() => View(new BookViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var id = await _books.CreateAsync(BuildBook(model));
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (FormatException ex)
            {
                ModelState.AddModelError(nameof(model.TocHtml), ex.Message);
                return View(model);
            }
            catch (SqlException ex)
            {
                return SaveFailed(model, ex);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var book = await _books.GetByIdAsync(id);
            if (book is null)
                return NotFound();

            var model = BookViewModel.FromBook(book);
            model.TocHtml = _toc.XmlToHtml(book.TableOfContents);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel model)
        {
            model.Id = id;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var updated = await _books.UpdateAsync(BuildBook(model));
                return updated
                    ? RedirectToAction(nameof(Details), new { id })
                    : NotFound();
            }
            catch (FormatException ex)
            {
                ModelState.AddModelError(nameof(model.TocHtml), ex.Message);
                return View(model);
            }
            catch (SqlException ex)
            {
                return SaveFailed(model, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _books.DeleteAsync(id);
            return deleted ? RedirectToAction(nameof(Index)) : NotFound();
        }

        public async Task<IActionResult> ExportToc(int id)
        {
            var book = await _books.GetByIdAsync(id);
            if (book?.TableOfContents is null)
                return NotFound();

            var xml = XDocument.Parse(book.TableOfContents).ToString();
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", $"toc-{id}.xml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportToc(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return BadRequest("Выберите XML-файл.");

            if (file.Length > MaxImportSize)
                return BadRequest("Файл слишком большой (максимум 1 МБ).");

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            var xml = await reader.ReadToEndAsync();

            try
            {
                return Json(new { html = _toc.XmlToHtml(xml) });
            }
            catch (Exception ex) when (ex is XmlException or FormatException)
            {
                return BadRequest("Файл не является корректным оглавлением: " + ex.Message);
            }
        }

        private Core.Models.Book BuildBook(BookViewModel model)
        {
            var book = model.ToBook();
            book.TableOfContents = _toc.HtmlToXml(model.TocHtml);
            return book;
        }

        private IActionResult SaveFailed(BookViewModel model, SqlException ex)
        {
            _logger.LogError(ex, "Не удалось сохранить книгу {BookId}", model.Id);
            ModelState.AddModelError(string.Empty, "Не удалось сохранить книгу. Попробуйте ещё раз.");
            return View(model);
        }
    }
}
