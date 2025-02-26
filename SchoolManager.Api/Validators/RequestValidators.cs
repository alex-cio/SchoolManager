namespace SchoolManager.Api.Validators;
using SchoolManager.Models;
using FluentValidation;

/// <summary>
/// Validation rules for the PATCH assign endpoint.
/// Can be extended to validate all http requests.
/// </summary>
public class RequestValidator : AbstractValidator<Request>
{
    public RequestValidator()
    {
        RuleFor(r => r.Assignments)
            .NotNull().WithMessage("Assignments cannot be null.")
            .NotEmpty().WithMessage("Assignments list cannot be empty.")
            .Must(a => a.Count > 0).WithMessage("Assignments list must have at least one assignment.")
            .ForEach(a =>
                    a.SetValidator(new AssignmentValidator())  // Validate each item in the list
            );
    }
}

public class AssignmentValidator : AbstractValidator<Assignment>
{
    public AssignmentValidator()
    {
        RuleFor(a => a.PupilId)
            .NotEqual(0).WithMessage("PupilId must not be equal with 0.");

        RuleFor(a => a.ClassId)
            .NotEqual(0).WithMessage("ClassId must not be equal with 0.");
    }
}

