// using FluentValidation;
// using KubeOps.Operator.Webhooks;
//
// namespace CostSaver.Infrastructure.Validators;
//
// public class CostSaverValidator : IValidationWebhook<Entities.CostSaver>
// {
//     public AdmissionOperations Operations => AdmissionOperations.Create | AdmissionOperations.Update;
//
//     public ValidationResult Create(Entities.CostSaver newEntity, bool dryRun)
//     {
//         var validator = new Validator();
//         var result = validator.Validate(newEntity);
//         
//         return result.IsValid
//             ? ValidationResult.Success()
//             : ValidationResult.Fail(StatusCodes.Status400BadRequest, string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
//     }
//
//     private sealed class Validator : AbstractValidator<Entities.CostSaver>
//     {
//         public Validator()
//         {
//             RuleFor(cs => cs.Spec.NamespaceLabel)
//                 .NotEmpty()
//                 .WithMessage($"{nameof(Entities.CostSaverSpec.NamespaceLabel)} must be set.");
//         }
//     }
// }
