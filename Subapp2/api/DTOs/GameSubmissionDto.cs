using System.ComponentModel.DataAnnotations;

namespace Subapp2.DTOs;

public class GameSubmissionDto
{
    [Required(ErrorMessage = "Answers are required.")]
    [MinLength(1, ErrorMessage = "Submit at least one answer.")]
    public List<GameAnswerDto> Answers { get; set; } = new();
}