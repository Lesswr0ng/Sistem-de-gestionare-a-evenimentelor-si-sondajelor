using System.ComponentModel.DataAnnotations;

namespace EventsAndPolls.Application.DTOs.Requests;
public class CreatePollDto
{
     [Required]
     public int EventId { get; set; }

     [Required(ErrorMessage = "Question is required")]
     [StringLength(500, ErrorMessage = "Question cannot exceed 500 characters")]
     public string Question { get; set; } = string.Empty;

     [Required(ErrorMessage = "At least one option is required")]
     [MinLength(1, ErrorMessage = "At least one option is required")]
     public List<string> Options { get; set; } = new();

     public bool AllowMultipleChoices { get; set; }

     public DateTime? ClosesAt { get; set; }
}