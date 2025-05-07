using CCICustomerPortalApi.Models.DTOs;
using FluentValidation;

namespace CCICustomerPortalApi.Validators;

public class ReportDtoValidator : AbstractValidator<ReportDto>
{
    public ReportDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.PowerBiReportId)
            .NotEmpty()
            .Matches("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
            .WithMessage("PowerBiReportId must be a valid GUID");

        RuleFor(x => x.WorkspaceId)
            .GreaterThan(0);

        RuleFor(x => x.ReportCategoryId)
            .GreaterThan(0);
    }
}