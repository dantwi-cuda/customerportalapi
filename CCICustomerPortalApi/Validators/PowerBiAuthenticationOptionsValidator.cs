using CCICustomerPortalApi.Models;
using FluentValidation;

namespace CCICustomerPortalApi.Validators;

public class PowerBiAuthenticationOptionsValidator : AbstractValidator<PowerBiAuthenticationOptions>
{
    public PowerBiAuthenticationOptionsValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .Matches("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
            .WithMessage("WorkspaceId must be a valid GUID");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .Matches("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
            .WithMessage("TenantId must be a valid GUID");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .Matches("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")
            .WithMessage("ClientId must be a valid GUID");

        RuleFor(x => x.ClientSecret)
            .NotEmpty();

        RuleFor(x => x.AuthorityUri)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("AuthorityUri must be a valid URI");

        RuleFor(x => x.ResourceUri)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("ResourceUri must be a valid URI");

        RuleFor(x => x.EmbedUrlBase)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("EmbedUrlBase must be a valid URI");
    }
}