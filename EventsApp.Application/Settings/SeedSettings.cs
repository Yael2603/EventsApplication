namespace EventsApp.Application.Settings;

public class SeedSettings
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = "מנהל מערכת";
    public bool DemoUsers { get; set; }             // האם ליצור משתמשי דמו (מופעל רק ב-appsettings.Development.json)
}
