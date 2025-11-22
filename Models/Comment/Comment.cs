using System.ComponentModel.DataAnnotations;
using CommentService.Api.Models.Reaction;
namespace CommentService.Api.Models.Comment
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid VideoId { get; set; }
        public Guid UserId { get; set; }
        public  string UserName { get; set; }  
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ParentCommentId { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDisLikes { get; set; }
        public virtual Comment ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; }
        public ICollection<Reaction.Reaction> Reactions { get; set; }

    }
}
