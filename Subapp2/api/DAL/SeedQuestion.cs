namespace Subapp2.DAL;

// Represents a question. This is only used for loading seed data.
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