using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Author;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class AuthorService : BaseService<Author, AuthorDTO, AuthorCreateDTO, AuthorUpdateDTO>, IAuthorService
    {
        private readonly IMemoryCache _cache;
        private const string ALL_AUTHORS_KEY = "authors_all";

        public AuthorService(
            IAuthorRepository repo,
            IMapper mapper,
            IMemoryCache cache,
            IUnitOfWork unitOfWork) : base(repo, unitOfWork, mapper)
        {
            _cache = cache;
        }

        public override async Task<IEnumerable<AuthorDTO>> GetAllAsync()
        {
            return await _cache.GetOrCreateAsync(ALL_AUTHORS_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await base.GetAllAsync();
            });
        }

        public override async Task<AuthorDTO> CreateAsync(AuthorCreateDTO dto)
        {
            var result = await base.CreateAsync(dto); // Base gọi CommitAsync
            _cache.Remove(ALL_AUTHORS_KEY);
            return result;
        }

        public override async Task<AuthorDTO> UpdateAsync(int id, AuthorUpdateDTO dto)
        {
            var result = await base.UpdateAsync(id, dto);
            _cache.Remove(ALL_AUTHORS_KEY);
            return result;
        }

        public override async Task<bool> DeleteAsync(int id, bool hardDelete = false)
        {
            var result = await base.DeleteAsync(id, hardDelete);
            if (result) _cache.Remove(ALL_AUTHORS_KEY);
            return result;
        }
    }
}