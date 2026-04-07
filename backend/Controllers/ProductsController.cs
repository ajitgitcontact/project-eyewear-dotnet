using backend.DTOs.ProductDtos;
using backend.DTOs.CustomizationOptionDtos;
using backend.DTOs.CustomizationValueDtos;
using backend.DTOs.ProductImageDtos;
using backend.DTOs.CustomizationImageDtos;
using backend.Services.ProductsService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductBusinessService _businessService;
    private readonly IProductService _productService;
    private readonly ICustomizationOptionService _optionService;
    private readonly ICustomizationValueService _valueService;
    private readonly IProductImageService _imageService;
    private readonly ICustomizationImageService _custImageService;

    public ProductsController(
        IProductBusinessService businessService,
        IProductService productService,
        ICustomizationOptionService optionService,
        ICustomizationValueService valueService,
        IProductImageService imageService,
        ICustomizationImageService custImageService)
    {
        _businessService = businessService;
        _productService = productService;
        _optionService = optionService;
        _valueService = valueService;
        _imageService = imageService;
        _custImageService = custImageService;
    }

    /// <summary>
    /// Create a full product with customizations, values, images, and customization images in one call.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateFullProduct([FromBody] CreateFullProductDto dto)
    {
        try
        {
            var result = await _businessService.CreateFullProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get a full product with all nested customizations and images.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _businessService.GetFullProductByIdAsync(id);
        if (product is null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    /// <summary>
    /// Get all products with full nested data.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _businessService.GetAllFullProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Update basic product details.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, dto);
            if (product is null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a product and all related data (cascade).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = "Product not found." });

        return NoContent();
    }

    /// <summary>
    /// Get a product by SKU.
    /// </summary>
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku)
    {
        var product = await _productService.GetBySkuAsync(sku);
        if (product is null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }

    // ─── Customization Options ──────────────────────────────────

    [HttpPost("{productId}/options")]
    public async Task<IActionResult> AddOption(int productId, [FromBody] CreateCustomizationOptionDto dto)
    {
        try
        {
            dto.ProductId = productId;
            var result = await _optionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = productId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("options/{optionId}")]
    public async Task<IActionResult> UpdateOption(int optionId, [FromBody] UpdateCustomizationOptionDto dto)
    {
        var result = await _optionService.UpdateAsync(optionId, dto);
        if (result is null)
            return NotFound(new { message = "Customization option not found." });

        return Ok(result);
    }

    [HttpDelete("options/{optionId}")]
    public async Task<IActionResult> DeleteOption(int optionId)
    {
        var deleted = await _optionService.DeleteAsync(optionId);
        if (!deleted)
            return NotFound(new { message = "Customization option not found." });

        return NoContent();
    }

    // ─── Customization Values ───────────────────────────────────

    [HttpPost("options/{optionId}/values")]
    public async Task<IActionResult> AddValue(int optionId, [FromBody] CreateCustomizationValueDto dto)
    {
        try
        {
            dto.CustomizationOptionId = optionId;
            var result = await _valueService.CreateAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("values/{valueId}")]
    public async Task<IActionResult> UpdateValue(int valueId, [FromBody] UpdateCustomizationValueDto dto)
    {
        var result = await _valueService.UpdateAsync(valueId, dto);
        if (result is null)
            return NotFound(new { message = "Customization value not found." });

        return Ok(result);
    }

    [HttpDelete("values/{valueId}")]
    public async Task<IActionResult> DeleteValue(int valueId)
    {
        var deleted = await _valueService.DeleteAsync(valueId);
        if (!deleted)
            return NotFound(new { message = "Customization value not found." });

        return NoContent();
    }

    // ─── Product Images ─────────────────────────────────────────

    [HttpPost("{productId}/images")]
    public async Task<IActionResult> AddImage(int productId, [FromBody] CreateProductImageDto dto)
    {
        try
        {
            dto.ProductId = productId;
            var result = await _imageService.CreateAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("images/{imageId}")]
    public async Task<IActionResult> UpdateImage(int imageId, [FromBody] UpdateProductImageDto dto)
    {
        var result = await _imageService.UpdateAsync(imageId, dto);
        if (result is null)
            return NotFound(new { message = "Product image not found." });

        return Ok(result);
    }

    [HttpDelete("images/{imageId}")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var deleted = await _imageService.DeleteAsync(imageId);
        if (!deleted)
            return NotFound(new { message = "Product image not found." });

        return NoContent();
    }

    // ─── Customization Images ───────────────────────────────────

    [HttpPost("{productId}/customization-images")]
    public async Task<IActionResult> AddCustomizationImage(int productId, [FromBody] CreateCustomizationImageDto dto)
    {
        try
        {
            dto.ProductId = productId;
            var result = await _custImageService.CreateAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("customization-images/{imageId}")]
    public async Task<IActionResult> UpdateCustomizationImage(int imageId, [FromBody] UpdateCustomizationImageDto dto)
    {
        var result = await _custImageService.UpdateAsync(imageId, dto);
        if (result is null)
            return NotFound(new { message = "Customization image not found." });

        return Ok(result);
    }

    [HttpDelete("customization-images/{imageId}")]
    public async Task<IActionResult> DeleteCustomizationImage(int imageId)
    {
        var deleted = await _custImageService.DeleteAsync(imageId);
        if (!deleted)
            return NotFound(new { message = "Customization image not found." });

        return NoContent();
    }
}
