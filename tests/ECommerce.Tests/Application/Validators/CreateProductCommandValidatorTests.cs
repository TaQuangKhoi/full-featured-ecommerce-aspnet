using ECommerce.Application.Products.Commands;
using ECommerce.Application.Products.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.Tests.Application.Validators;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new CreateProductCommand(
            Name: "Valid Product",
            Description: "A valid description",
            Price: 9.99m,
            Stock: 10,
            ImageUrl: "https://example.com/img.png",
            CategoryId: Guid.NewGuid(),
            IsActive: true);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var command = new CreateProductCommand(
            Name: "",
            Description: "Some description",
            Price: 9.99m,
            Stock: 10,
            ImageUrl: "https://example.com/img.png",
            CategoryId: Guid.NewGuid(),
            IsActive: true);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NegativePrice_Fails()
    {
        var command = new CreateProductCommand(
            Name: "Product",
            Description: "Some description",
            Price: -5.00m,
            Stock: 10,
            ImageUrl: "https://example.com/img.png",
            CategoryId: Guid.NewGuid(),
            IsActive: true);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_NegativeStock_Fails()
    {
        var command = new CreateProductCommand(
            Name: "Product",
            Description: "Some description",
            Price: 9.99m,
            Stock: -1,
            ImageUrl: "https://example.com/img.png",
            CategoryId: Guid.NewGuid(),
            IsActive: true);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Stock);
    }
}
