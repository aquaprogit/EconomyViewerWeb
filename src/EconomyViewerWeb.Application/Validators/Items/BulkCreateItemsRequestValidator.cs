using EconomyViewerWeb.Application.Contracts.Items;
using FluentValidation;

namespace EconomyViewerWeb.Application.Validators.Items;

public sealed class BulkCreateItemsRequestValidator
    : AbstractValidator<BulkCreateItemsRequest>
{
    public BulkCreateItemsRequestValidator()
    {
        RuleFor(request => request.Mod)
            .NotEmpty()
            .WithMessage("Mod is required.");

        RuleFor(request => request.Lines)
            .NotEmpty()
            .WithMessage("Lines are required.");
    }
}
