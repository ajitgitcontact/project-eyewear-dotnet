using backend.DTOs.ProductDtos;
using backend.DTOs.CustomizationOptionDtos;
using backend.DTOs.CustomizationValueDtos;
using backend.DTOs.ProductImageDtos;
using backend.DTOs.CustomizationImageDtos;
using backend.Application.Abstractions.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductBusinessService businessService,
        IProductService productService,
        ICustomizationOptionService optionService,
        ICustomizationValueService valueService,
        IProductImageService imageService,
        ICustomizationImageService custImageService,
        ILogger<ProductsController> logger)
    {
        _businessService = businessService;
        _productService = productService;
        _optionService = optionService;
        _valueService = valueService;
        _imageService = imageService;
        _custImageService = custImageService;
        _logger = logger;
    }

    /// <summary>
    /// Create a full product with customizations, values, images, and customization images in one call.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateFullProduct([FromBody] CreateFullProductDto dto)
    {
        _logger.LogInformation("CreateFullProduct request received. SKU={Sku}", dto.SKU);
        var result = await _businessService.CreateFullProductAsync(dto);
        _logger.LogInformation("CreateFullProduct succeeded. ProductId={ProductId}, SKU={Sku}", result.ProductId, result.SKU);
        return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
    }

    /// <summary>
    /// Get a full product with all nested customizations and images.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("GetById request received. ProductId={ProductId}", id);
        var product = await _businessService.GetFullProductByIdAsync(id);
        _logger.LogInformation("GetById succeeded. ProductId={ProductId}", id);
        return Ok(product);
    }

    /// <summary>
    /// Get all products with full nested data.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GetAll products request received.");
        var products = await _businessService.GetAllFullProductsAsync();
        _logger.LogInformation("GetAll products succeeded.");
        return Ok(products);
    }

    /// <summary>
    /// Update basic product details.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        _logger.LogInformation("Update product request received. ProductId={ProductId}, SKU={Sku}", id, dto.SKU);
        var product = await _productService.UpdateAsync(id, dto);
        _logger.LogInformation("Update product succeeded. ProductId={ProductId}", id);
        return Ok(product);
    }

    /// <summary>
    /// Delete a product and all related data (cascade).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Delete product request received. ProductId={ProductId}", id);
        await _productService.DeleteAsync(id);
        _logger.LogInformation("Delete product succeeded. ProductId={ProductId}", id);
        return NoContent();
    }

    /// <summary>
    /// Get a product by SKU.
    /// </summary>
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku)
    {
        _logger.LogInformation("GetBySku request received. SKU={Sku}", sku);
        var product = await _productService.GetBySkuAsync(sku);
        _logger.LogInformation("GetBySku succeeded. SKU={Sku}, ProductId={ProductId}", sku, product.ProductId);
        return Ok(product);
    }

    // ─── Customization Options ──────────────────────────────────

    [HttpGet("{productId}/options")]
    public async Task<IActionResult> GetOptionsByProductId(int productId)
    {
        var options = await _optionService.GetByProductIdAsync(productId);
        return Ok(options);
    }

    [HttpGet("options/{optionId}")]
    public async Task<IActionResult> GetOptionById(int optionId)
    {
        var option = await _optionService.GetByIdAsync(optionId);
        return Ok(option);
    }

    [HttpPost("{productId}/options")]
    public async Task<IActionResult> AddOption(int productId, [FromBody] CreateCustomizationOptionDto dto)
    {
        dto.ProductId = productId;
        var result = await _optionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = productId }, result);
    }

    [HttpPut("options/{optionId}")]
    public async Task<IActionResult> UpdateOption(int optionId, [FromBody] UpdateCustomizationOptionDto dto)
    {
        var result = await _optionService.UpdateAsync(optionId, dto);
        return Ok(result);
    }

    [HttpDelete("options/{optionId}")]
    public async Task<IActionResult> DeleteOption(int optionId)
    {
        await _optionService.DeleteAsync(optionId);
        return NoContent();
    }

    // ─── Customization Values ───────────────────────────────────

    [HttpGet("options/{optionId}/values")]
    public async Task<IActionResult> GetValuesByOptionId(int optionId)
    {
        var values = await _valueService.GetByOptionIdAsync(optionId);
        return Ok(values);
    }

    [HttpGet("values/{valueId}")]
    public async Task<IActionResult> GetValueById(int valueId)
    {
        var value = await _valueService.GetByIdAsync(valueId);
        return Ok(value);
    }

    [HttpPost("options/{optionId}/values")]
    public async Task<IActionResult> AddValue(int optionId, [FromBody] CreateCustomizationValueDto dto)
    {
        dto.CustomizationOptionId = optionId;
        var result = await _valueService.CreateAsync(dto);
        return Ok(result);
    }

    [HttpPut("values/{valueId}")]
    public async Task<IActionResult> UpdateValue(int valueId, [FromBody] UpdateCustomizationValueDto dto)
    {
        var result = await _valueService.UpdateAsync(valueId, dto);
        return Ok(result);
    }

    [HttpDelete("values/{valueId}")]
    public async Task<IActionResult> DeleteValue(int valueId)
    {
        await _valueService.DeleteAsync(valueId);
        return NoContent();
    }

    // ─── Product Images ─────────────────────────────────────────

    [HttpGet("{productId}/images")]
    public async Task<IActionResult> GetImagesByProductId(int productId)
    {
        var images = await _imageService.GetByProductIdAsync(productId);
        return Ok(images);
    }

    [HttpGet("images/{imageId}")]
    public async Task<IActionResult> GetImageById(int imageId)
    {
        var image = await _imageService.GetByIdAsync(imageId);
        return Ok(image);
    }

    [HttpPost("{productId}/images")]
    public async Task<IActionResult> AddImage(int productId, [FromBody] CreateProductImageDto dto)
    {
        dto.ProductId = productId;
        var result = await _imageService.CreateAsync(dto);
        return Ok(result);
    }

    [HttpPut("images/{imageId}")]
    public async Task<IActionResult> UpdateImage(int imageId, [FromBody] UpdateProductImageDto dto)
    {
        var result = await _imageService.UpdateAsync(imageId, dto);
        return Ok(result);
    }

    [HttpDelete("images/{imageId}")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        await _imageService.DeleteAsync(imageId);
        return NoContent();
    }

    // ─── Customization Images ───────────────────────────────────

    [HttpGet("{productId}/customization-images")]
    public async Task<IActionResult> GetCustomizationImagesByProductId(int productId)
    {
        var images = await _custImageService.GetByProductIdAsync(productId);
        return Ok(images);
    }

    [HttpGet("customization-images/{imageId}")]
    public async Task<IActionResult> GetCustomizationImageById(int imageId)
    {
        var image = await _custImageService.GetByIdAsync(imageId);
        return Ok(image);
    }

    [HttpPost("{productId}/customization-images")]
    public async Task<IActionResult> AddCustomizationImage(int productId, [FromBody] CreateCustomizationImageDto dto)
    {
        dto.ProductId = productId;
        var result = await _custImageService.CreateAsync(dto);
        return Ok(result);
    }

    [HttpPut("customization-images/{imageId}")]
    public async Task<IActionResult> UpdateCustomizationImage(int imageId, [FromBody] UpdateCustomizationImageDto dto)
    {
        var result = await _custImageService.UpdateAsync(imageId, dto);
        return Ok(result);
    }

    [HttpDelete("customization-images/{imageId}")]
    public async Task<IActionResult> DeleteCustomizationImage(int imageId)
    {
        await _custImageService.DeleteAsync(imageId);
        return NoContent();
    }
}
