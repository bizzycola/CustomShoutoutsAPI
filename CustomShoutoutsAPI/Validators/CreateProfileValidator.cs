using CustomShoutoutsAPI.DTOs;
using FluentValidation;

namespace CustomShoutoutsAPI.Validators
{
    public class CreateProfileValidator : AbstractValidator<CreateAccountDTO>
    {
        public CreateProfileValidator()
        {
            RuleFor(r => r.SignupCode)
                .NotEmpty()
                .WithMessage("Signup code is required");

        }
    }
}
