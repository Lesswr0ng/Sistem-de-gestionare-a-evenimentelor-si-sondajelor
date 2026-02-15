using System.ComponentModel.DataAnnotations;

namespace EventsAndPolls.Application.DTOs.Requests;

public class CastVoteDto
{
     [Required]
     public int PollId { get; set; }

     [Required(ErrorMessage = "At least one option must be selected")]
     [MinLength(1, ErrorMessage = "At least one option must be selected")]
     public List<int> SelectedOptionIds { get; set; } = new();

     public string? Comment { get; set; }
}