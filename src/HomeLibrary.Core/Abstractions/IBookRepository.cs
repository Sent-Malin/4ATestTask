using HomeLibrary.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeLibrary.Core.Abstractions
{
    public interface IBookRepository
    {
        Task<IReadOnlyList<BookListItem>> GetListAsync(string? searchText);
        Task<Book?> GetByIdAsync(int id);
        Task<int> CreateAsync(Book book);
        Task<bool> UpdateAsync(Book book);
        Task<bool> DeleteAsync(int id);
    }
}
