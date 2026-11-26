using Microsoft.AspNetCore.Mvc;
using Subapp2.DAL;
using Subapp2.DTOs;
using Subapp2.Models;
using Microsoft.AspNetCore.Authorization;

namespace Subapp2.Controllers;

// API controller for categories.
[ApiController]
[Route("api/[controller]")]   // Base route: api/categoryapi
// Category management is restricted to administrators.
[Authorize(Roles = IdentitySeeder.AdminRole)]
// Validate antiforgery tokens for POST, PUT, and DELETE; GET remains read-only.
[AutoValidateAntiforgeryToken]
public class CategoryAPIController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryAPIController> _logger;

    public CategoryAPIController(ICategoryRepository categoryRepository, ILogger<CategoryAPIController> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    // GET api/categoryapi/categorylist
    [HttpGet("categorylist")]
    public async Task<IActionResult> CategoryList()
    {
        var categories = await _categoryRepository.GetAllAsync();
        if (categories == null)
        {
            _logger.LogError("[CategoryAPIController] Category list not found while executing _categoryRepository.GetAllAsync()");
            return StatusCode(500, "Unable to load categories. Please try again.");
        }

        var categoryDtos = categories.Select(category => new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        });

        return Ok(categoryDtos);
    }

    // GET api/categoryapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogError("[CategoryAPIController] Category not found for the CategoryId {CategoryId:0000}", id);
            return NotFound("Category not found for the CategoryId");
        }

        var categoryDto = new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        };

        return Ok(categoryDto);
    }

    // POST api/categoryapi/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CategoryDto categoryDto)
    {
        if (categoryDto == null)
        {
            return BadRequest("Category cannot be null");
        }

        var newCategory = new Category
        {
            Name = categoryDto.Name
        };

        bool returnOk = await _categoryRepository.AddAsync(newCategory);
        if (returnOk)
        {
            categoryDto.CategoryId = newCategory.CategoryId;
            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.CategoryId }, categoryDto);
        }

        _logger.LogWarning("[CategoryAPIController] Category creation failed {@category}", newCategory);
        return StatusCode(500, "Internal server error");
    }

    // PUT api/categoryapi/update/5
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto categoryDto)
    {
        if (categoryDto == null)
        {
            return BadRequest("Category data cannot be null");
        }

        // Find the category in the database
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return NotFound("Category not found");
        }

        // Update the category properties
        existingCategory.Name = categoryDto.Name;

        // Save the changes
        bool updateSuccessful = await _categoryRepository.UpdateAsync(existingCategory);
        if (updateSuccessful)
        {
            categoryDto.CategoryId = existingCategory.CategoryId;
            return Ok(categoryDto);
        }

        _logger.LogWarning("[CategoryAPIController] Category update failed {@category}", existingCategory);
        return StatusCode(500, "Internal server error");
    }

    // DELETE api/categoryapi/delete/5
    // Delete only empty categories and distinguish refusal from server failure.
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _categoryRepository.DeleteAsync(id);

        // The repository logs the outcome and any exception details.
        return result switch
        {
            CategoryDeleteResult.Deleted => NoContent(),
            CategoryDeleteResult.NotFound => NotFound("Category not found."),
            CategoryDeleteResult.HasQuestions => BadRequest(
                "Unable to delete this category because it contains questions."),
            _ => StatusCode(500, "Unable to delete the category. Please try again.")
        };
    }
}
