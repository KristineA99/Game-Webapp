using System.ComponentModel.DataAnnotations;

namespace Subapp2.DTOs;

public class GameAnswerDto
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid question ID is required.")]
    public int QuestionId { get; set; }

    // Match the existing answer numbering: 1 = A, 2 = B, 3 = C, 4 = D.
    [Range(1, 4, ErrorMessage = "Select an answer between 1 and 4.")]
    public int SelectedOption { get; set; }
}