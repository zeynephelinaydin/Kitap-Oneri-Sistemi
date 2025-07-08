using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapOneri.Models

{
    [Table("blog_comments")]
    public class BlogComment
    {
        [Key]
        [Column("comment_id")]
        public int CommentId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("subject")]
        [MaxLength(200)]
        public string? Subject { get; set; } // Nullable yapılmış


        [Required]
        [Column("message")]
        public string Message { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } 

        [Required]
        [Column("page_name")]
        [MaxLength(50)]
        public string PageName { get; set; } = "single-blog-1";

        // İlişkili kullanıcı bilgisi
        public virtual User User { get; set; }
    }
}