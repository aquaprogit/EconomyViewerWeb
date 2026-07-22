using EconomyViewerWeb.Application.Contracts.Items;
using FluentValidation;

namespace EconomyViewerWeb.Application.Validators.Items;

public sealed class CreateItemRequestValidator
    : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(request => request.Mod)
            .NotEmpty()
            .WithMessage("Mod is required.");

        RuleFor(request => request.Count)
            .GreaterThan(0)
            .WithMessage("Count must be greater than zero.");

        RuleFor(request => request.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");
    }
}
