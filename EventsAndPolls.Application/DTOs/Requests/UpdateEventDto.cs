using System.ComponentModel.DataAnnotations;

namespace EventsAndPolls.Application.DTOs.Requests;

public class UpdateEventDto
{
     [Required]
     public int Id { get; set; }

     [Required(ErrorMessage = "Title is required")]
     [StringLength(100, MinimumLength = 3)]
     public string Title { get; set; } = string.Empty;

     [Required(ErrorMessage = "Description is required")]
     public string Description { get; set; } = string.Empty;

     [Required(ErrorMessage = "Start date is required")]
     public DateTime StartDate { get; set; }

     [Required(ErrorMessage = "End date is required")]
     public DateTime EndDate { get; set; }

     [Required(ErrorMessage = "Location is required")]
     public string Location { get; set; } = string.Empty;

     [Required]
     [Range(1, 10000)]
     public int MaxParticipants { get; set; }
}