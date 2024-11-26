using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ChatMessageDTO
    {
        [Required(ErrorMessage ="sender field is required")]
        public string sender {  get; set; }
        [Required(ErrorMessage = "message field is required")]
        public string message { get; set; }
        [Required(ErrorMessage = "receiver field is required")]
        public string receiver {  get; set; }
    }
}
 