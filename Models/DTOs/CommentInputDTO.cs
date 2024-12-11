using AonFreelancing.Attributes;
using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations;


namespace AonFreelancing.Models.DTOs
{
    public class CommentInputDTO
    {
        [Required(ErrorMessage ="Can't create empty comment !")]
        public string Content { get; set; }

        [MaxFileSize(Constants.MAX_FILE_SIZE)]
        [AllowedFileExtensions([".jpg", ".jpeg", ".png"])]
        public IFormFile? ImageFile { get; set; }
    }
}
