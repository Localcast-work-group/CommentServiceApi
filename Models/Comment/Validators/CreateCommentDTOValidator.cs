using CommentService.Api.Models.Comment.DTOs;
using FluentValidation;

namespace CommentService.Api.Models.Comment.Validators
{
    public class CreateCommentDTOValidator : AbstractValidator<CreateCommentDTO>
    {

        public CreateCommentDTOValidator()
        {
            RuleFor(x => x.Content).NotEmpty().MaximumLength(500);
        }
    }
}
