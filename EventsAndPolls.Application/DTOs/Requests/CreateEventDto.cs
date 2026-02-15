using System.ComponentModel.DataAnnotations;

namespace EventsAndPolls.Application.DTOs.Requests;

public class CreateEventDto
{
     [Required(ErrorMessage = "Title is required")]
     [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
     public string Title { get; set; } = string.Empty;

     [Required(ErrorMessage = "Description is required")]
     [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
     public string Description { get; set; } = string.Empty;

     [Required(ErrorMessage = "Start date is required")]
     public DateTime StartDate { get; set; }

     [Required(ErrorMessage = "End date is required")]
     public DateTime EndDate { get; set; }

     [Required(ErrorMessage = "Location is required")]
     [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
     public string Location { get; set; } = string.Empty;

     [Required]
     [Range(1, 10000, ErrorMessage = "Max participants must be between 1 and 10000")]
     public int MaxParticipants { get; set; }
}