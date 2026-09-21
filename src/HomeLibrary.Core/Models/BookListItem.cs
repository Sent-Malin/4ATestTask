using System;
using System.Collections.Generic;
using System.Text;

namespace HomeLibrary.Core.Models
{
    public class BookListItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public short PublishYear { get; set; }
    }
}
