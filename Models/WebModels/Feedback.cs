using System.ComponentModel.DataAnnotations;

namespace KiCData.Models.WebModels
{
    public class Feedback
    {
        [Required(ErrorMessage = "This field is required.")]
        [Display(Name="What would you like us to know?")]
        public string? Text { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Display(Name="Name")]
        public string? Name { get; set; }
    }
}
