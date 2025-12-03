using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Category;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class CategoryService : BaseService<Category, CategoryDTO, CategoryCreateDTO, CategoryUpdateDTO>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo; 
        private readonly IMemoryCache _cache;
        private const string ALL_CATEGORIES_KEY = "categories_all";

        public CategoryService(
            ICategoryRepository repo,
            IMapper mapper,
            IMemoryCache cache) : base(repo, mapper)
        {
            _categoryRepo = repo;
            _cache = cache;
        }



        public override async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            ///<summary>
            /// Cache danh sách thể loại trong 30 phút (vì Menu gọi cái này rất nhiều)
             ///</summary>    
            return await _cache.GetOrCreateAsync(ALL_CATEGORIES_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await base.GetAllAsync();
            });
        }

        public async Task<CategoryDTO?> GetBySlugAsync(string slug)
        {
            string key = $"category_slug_{slug}";

            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var category = await _categoryRepo.GetBySlugAsync(slug);
                if (category == null) return null;

                return _mapper.Map<CategoryDTO>(category);
            });
        }

        public override async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            string key = $"category_id_{id}";
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await base.GetByIdAsync(id);
            });
        }
        ///<summary
        /// --- 2. WRITE (Create - Update - Delete & Xóa Cache) ---
        ///</summary>
        public override async Task<CategoryDTO> CreateAsync(CategoryCreateDTO dto)
        {
            string slug = SlugHelper.GenerateSlug(dto.Name);

            
            string originalSlug = slug;
            int count = 0;
            while ((await _categoryRepo.GetBySlugAsync(slug)) != null)
            {
                count++;
                slug = $"{originalSlug}-{count}";
            }

            var entity = _mapper.Map<Category>(dto);
            entity.Slug = slug;

           
            await _categoryRepo.AddAsync(entity);

           
            _cache.Remove(ALL_CATEGORIES_KEY);

            return _mapper.Map<CategoryDTO>(entity);
        }

        public override async Task<CategoryDTO> UpdateAsync(int id, CategoryUpdateDTO dto)
        {
            var entity = await _categoryRepo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Category not found");

            string oldSlug = entity.Slug;

  
            _mapper.Map(dto, entity);

            await _categoryRepo.UpdateAsync(entity);

            _cache.Remove(ALL_CATEGORIES_KEY);      
            _cache.Remove($"category_id_{id}");     
            _cache.Remove($"category_slug_{oldSlug}"); 

            return _mapper.Map<CategoryDTO>(entity);
        }

        public override async Task<bool> DeleteAsync(int id, bool hardDelete = false)
        {
          
            var entity = await _categoryRepo.GetByIdAsync(id);

            var result = await base.DeleteAsync(id, hardDelete);

            if (result && entity != null)
            {
                _cache.Remove(ALL_CATEGORIES_KEY);
                _cache.Remove($"category_id_{id}");
                if (!string.IsNullOrEmpty(entity.Slug))
                {
                    _cache.Remove($"category_slug_{entity.Slug}");
                }
            }
            return result;
        }
    }
}