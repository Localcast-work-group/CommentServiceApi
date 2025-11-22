namespace CommentService.Api.Models.Reaction.DTOs
{
    public class ToggleReactionDTO
    {
        public Guid TargetId { get; set; }
        public char ToggleType { get; set; }
    }
}
