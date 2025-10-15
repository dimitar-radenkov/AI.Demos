namespace AI.Core.Settings;

public sealed class FileIOSettings
{
    public const string SectionName = "FileIOSettings";

    public required string[] AllowedFolders { get; init; }
    public bool DisableFileOverwrite { get; init; } = false;
}
