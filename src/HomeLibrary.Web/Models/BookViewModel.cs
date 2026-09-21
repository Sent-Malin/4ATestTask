using HomeLibrary.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace HomeLibrary.Web.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите название")]
        [StringLength(300)]
        [Display(Name = "Название")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите автора")]
        [StringLength(200)]
        [Display(Name = "Автор")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите год издания")]
        [Range(1000, 2100, ErrorMessage = "Год должен быть от 1000 до 2100")]
        [Display(Name = "Год издания")]
        public short? PublishYear { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Оглавление")]
        public string? TocHtml { get; set; }

        public static BookViewModel FromBook(Book book) => new()
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            PublishYear = book.PublishYear,
            Description = book.Description
        };

        public Book ToBook() => new()
        {
            Id = Id,
            Title = Title,
            Author = Author,
            PublishYear = PublishYear!.Value,
            Description = Description
        };
    }
}
