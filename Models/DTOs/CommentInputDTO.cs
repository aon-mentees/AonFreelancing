using AonFreelancing.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class CommentInputDTO
    {
        [Required(ErrorMessage ="Can't create empty comment !")]
        public string Content { get; set; }

        [MaxFileSize(1024 * 1024 * 5)]
        [AllowedFileExtensions([".jpg", ".jpeg", ".png"])]
        public IFormFile? ImageFile { get; set; }
    }
}
