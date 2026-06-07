using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Repositories.Interfaces;
using ECommerce.DAL.Entities;

namespace ECommerce.BLL.Services
{
    /// <summary>
    /// Category Service: Business Logic for Category operations
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<Category>>> GetAllAsync()
        {
            return await _unitOfWork.Categories.GetAllAsync();
        }

        public async Task<Result<Category>> GetByIdAsync(int id)
        {
            return await _unitOfWork.Categories.GetByIdAsync(id);
        }

        public async Task<Result<Category>> CreateAsync(CategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            var result = await _unitOfWork.Categories.AddAsync(category);
            if (result.IsFailure)
                return result;

            await _unitOfWork.SaveChangesAsync();
            return Result<Category>.Success(result.Data!, "Category created successfully");
        }

        public async Task<Result<Category>> UpdateAsync(int id, CategoryDto dto)
        {
            var existingResult = await _unitOfWork.Categories.GetByIdAsync(id);
            if (existingResult.IsFailure)
                return Result<Category>.Failure("Category not found");

            var existing = existingResult.Data!;
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.ImageUrl = dto.ImageUrl;
            existing.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _unitOfWork.Categories.UpdateAsync(existing);
            if (updateResult.IsFailure)
                return updateResult;

            await _unitOfWork.SaveChangesAsync();
            return Result<Category>.Success(updateResult.Data!, "Category updated successfully");
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var existingResult = await _unitOfWork.Categories.GetByIdAsync(id);
            if (existingResult.IsFailure)
                return Result.Failure(existingResult.Message, existingResult.Errors);

            var deleteResult = await _unitOfWork.Categories.DeleteAsync(existingResult.Data!);
            if (deleteResult.IsFailure)
                return deleteResult;

            await _unitOfWork.SaveChangesAsync();
            return Result.Success("Category deleted successfully");
        }
    }
}
