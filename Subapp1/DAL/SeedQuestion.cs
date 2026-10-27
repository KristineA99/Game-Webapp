namespace Subapp1.DAL;

// Represents a question as it appears in questions.json.
// This is only used for loading seed data, not as a database entity.
public class SeedQuestion
{
    public string Category { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public int CorrectOption { get; set; }
}