using Dapper;
using HomeLibrary.Core.Abstractions;
using HomeLibrary.Core.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HomeLibrary.Data
{
    public class BookRepository : IBookRepository
    {
        private readonly string _connectionString;

        public BookRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IReadOnlyList<BookListItem>> GetListAsync(string? searchText)
        {
            await using var connection = new SqlConnection(_connectionString);

            var items = await connection.QueryAsync<BookListItem>(
                "dbo.Book_GetList",
                new { SearchText = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim() },
                commandType: CommandType.StoredProcedure);

            return items.AsList();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Book>(
                "dbo.Book_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(Book book)
        {
            await using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                "dbo.Book_Insert",
                ToParameters(book, includeId: false),
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAsync(Book book)
        {
            await using var connection = new SqlConnection(_connectionString);

            var affected = await connection.ExecuteAsync(
                "dbo.Book_Update",
                ToParameters(book, includeId: true),
                commandType: CommandType.StoredProcedure);

            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString);

            var affected = await connection.ExecuteAsync(
                "dbo.Book_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure);

            return affected > 0;
        }

        private static DynamicParameters ToParameters(Book book, bool includeId)
        {
            var parameters = new DynamicParameters();

            if (includeId)
                parameters.Add("@Id", book.Id);

            parameters.Add("@Title", book.Title);
            parameters.Add("@Author", book.Author);
            parameters.Add("@PublishYear", book.PublishYear);
            parameters.Add("@Description", book.Description);
            parameters.Add("@TableOfContents", book.TableOfContents, DbType.Xml);

            return parameters;
        }
    }
}
