using FluentValidation;
using RESTAuth.Domain.Dtos;

namespace RESTAuth.Api.Validators;

public class UserLoginDtoValidator: AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(dto => dto.Email)
            .MaximumLength(100).WithMessage("Email cannot be longer than 100 characters.")
            .NotEmpty().NotNull().WithMessage("Email cannot be empty or null")
            .EmailAddress().WithMessage("Invalid email address");
        
        RuleFor(dto => dto.Password)
            .NotNull().NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must contain at least 8 characters")
            .MaximumLength(32).WithMessage("Password must not exceed 32 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}