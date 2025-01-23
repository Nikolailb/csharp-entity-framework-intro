﻿using exercise.webapi.DTO;
using exercise.webapi.Exceptions;
using exercise.webapi.Models;
using exercise.webapi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace exercise.webapi.Endpoints
{
    public static class BookEndpoints
    {
        public static string Path { get; private set; } = "books";

        public static void ConfigureBooksEndpoints(this WebApplication app)
        {
            var group = app.MapGroup(Path);

            group.MapPost("/", CreateBook);
            group.MapGet("/", GetBooks);
            group.MapGet("/{id}", GetBook);
            group.MapPut("/{id}", UpdateBook);
            group.MapDelete("/{id}", DeleteBook);
            group.MapPost("/author/add/{id}", AddAuthor);
            group.MapPost("/author/remove/{id}", RemoveAuthor);
        }
        public static async Task<IResult> CreateBook(IRepository<Book> bookRepository, IRepository<Author> authorRepository, IRepository<Publisher> publisherRepository, BookPost entity)
        {
            try
            {
                Author author = await authorRepository.Get(entity.AuthorId);
                Publisher publisher = await publisherRepository.Get(entity.PublisherId);
                Book book = await bookRepository.Add(new Book
                {
                    Title = entity.Title,
                    PublisherId = entity.PublisherId,
                });
                book.Authors.Add(author);
                await bookRepository.Update(book);
                return TypedResults.Ok(new BookView(
                    book.Id,
                    book.Title,
                    [new AuthorInternal(
                        author.Id,
                        author.FirstName,
                        author.LastName,
                        author.Email
                    )],
                    new PublisherInternal(
                        publisher.Id,
                        publisher.Name
                    )
                ));
            }
            catch (IdNotFoundException ex)
            {
                return TypedResults.NotFound(new {ex.Message});
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }
        public static async Task<IResult> GetBooks(IRepository<Book> repository)
        {
            try
            {
                IEnumerable<Book> books = await repository.GetAll();
                return TypedResults.Ok(books.Select(b =>
                {
                    return new BookView(
                        b.Id,
                        b.Title,
                        b.Authors.Select(author => new AuthorInternal(
                            author.Id,
                            author.FirstName,
                            author.LastName,
                            author.Email
                        )),
                        new PublisherInternal(
                            b.Publisher.Id,
                            b.Publisher.Name
                        )
                    );
                }));
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }

        public static async Task<IResult> GetBook(IRepository<Book> repository, int id)
        {
            try
            {
                Book book = await repository.Get(id);
                return TypedResults.Ok(new BookView(
                    book.Id,
                    book.Title,
                    book.Authors.Select(author => new AuthorInternal(
                        author.Id,
                        author.FirstName,
                        author.LastName,
                        author.Email
                    )),
                    new PublisherInternal(
                        book.Publisher.Id,
                        book.Publisher.Name
                    )
                ));
            }
            catch (IdNotFoundException ex)
            {
                return TypedResults.NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }
        public static async Task<IResult> UpdateBook(IRepository<Book> repository, int id, BookPut entity)
        {
            try
            {
                Book book = await repository.Get(id);
                if (entity.Title != null) book.Title = entity.Title;

                book = await repository.Update(book);
                return TypedResults.Ok(new BookView(
                    book.Id,
                    book.Title,
                    book.Authors.Select(author => new AuthorInternal(
                        author.Id,
                        author.FirstName,
                        author.LastName,
                        author.Email
                    )),
                    new PublisherInternal(
                        book.Publisher.Id,
                        book.Publisher.Name
                    )
                ));
            }
            catch (IdNotFoundException ex)
            {
                return TypedResults.NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }
        public static async Task<IResult> DeleteBook(IRepository<Book> repository, int id)
        {
            try
            {
                Book book = await repository.Delete(id);
                return TypedResults.Ok(new { Message = $"Deleted Book with Title = {book.Title}" });
            }
            catch (IdNotFoundException ex)
            {
                return TypedResults.NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }
        public static async Task<IResult> AddAuthor(IRepository<Book> bookRepository, IRepository<Author> authorRepository, int id, int authorId)
        {
            try
            {
                Book book = await bookRepository.Get(id);
                if (book.Authors.Any(a => a.Id == authorId)) return TypedResults.BadRequest(new { Message = $"Author with ID = {authorId} is already registered to this book!" });
                Author author = await authorRepository.Get(authorId);
                book.Authors.Add(author);
                book = await bookRepository.Update(book);

                return TypedResults.Ok(new BookView(
                    book.Id,
                    book.Title,
                    book.Authors.Select(a => new AuthorInternal(
                        a.Id,
                        a.FirstName,
                        a.LastName,
                        a.Email
                    )),
                    new PublisherInternal(
                        book.Publisher.Id,
                        book.Publisher.Name
                    )
                ));
            }
            catch (IdNotFoundException ex)
            {
                return TypedResults.NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }
        public static async Task<IResult> RemoveAuthor(IRepository<Book> bookRepository, IRepository<Author> authorRepository, int id, int authorId)
        {
            try
            {
                Book book = await bookRepository.Get(id);
                if (book.Authors.Count <= 1) return TypedResults.BadRequest(new { Message = $"Book must have at least one author!" });
                Author author = await authorRepository.Get(authorId);
                book.Authors.Remove(author);
                book = await bookRepository.Update(book);

                return TypedResults.Ok(new BookView(
                    book.Id,
                    book.Title,
                    book.Authors.Select(a => new AuthorInternal(
                        a.Id,
                        a.FirstName,
                        a.LastName,
                        a.Email
                    )),
                    new PublisherInternal(
                        book.Publisher.Id,
                        book.Publisher.Name
                    )
                ));
            }
            catch (IdNotFoundException ex)
            {
                return TypedResults.NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return TypedResults.Problem(ex.Message);
            }
        }
    }
}
