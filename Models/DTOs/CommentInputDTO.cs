using AonFreelancing.Attributes;
using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations;
using static AonFreelancing.Utilities.Constants;


namespace AonFreelancing.Models.DTOs
{
    public class CommentInputDTO
    {
        [Required(ErrorMessage ="Can't create empty comment !")]
        public string Content { get; set; }

        [MaxFileSize(Constants.MAX_FILE_SIZE)]
        [AllowedFileExtensions([JPG, JPEG, PNG, GIF])]
        public IFormFile? ImageFile { get; set; }
    }
}
