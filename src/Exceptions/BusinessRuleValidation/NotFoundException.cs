namespace CommentService.Api.Exceptions.BusinessRuleValidation
{
    public class NotFoundException : BusinessRuleValidationException
    {
        public NotFoundException(string message) : base(message) { }
    }
}
