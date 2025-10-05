using AI.Shared.Settings;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AI.Shared.Plugins;

public sealed class FileManagementPlugin
{
    private readonly IEnumerable<string> allowedFolders;

    public FileManagementPlugin(IOptions<FileIOSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);

        if (options.Value.AllowedFolders is null || options.Value.AllowedFolders.Length == 0)
        {
            throw new ArgumentException("At least one allowed folder must be configured.", nameof(options.Value.AllowedFolders));
        }

        allowedFolders = options.Value.AllowedFolders;
    }

    [KernelFunction("get_allowed_folders")]
    [Description("Gets the list of folders where file operations are allowed.")]
    public string GetAllowedFolders()
    {
        return string.Join(", ", allowedFolders);
    }

    [KernelFunction("check_folder_access")]
    [Description("Checks if a specific folder path is accessible for file operations.")]
    public string CheckFolderAccess([Description("The folder path to check")] string folderPath)
    {
        try
        {
            string normalizedPath = ValidateAndNormalizePath(folderPath);
            bool hasAccess = IsPathAccessible(normalizedPath);

            return hasAccess
                ? $"Access granted: {normalizedPath}"
                : $"Access denied: {normalizedPath} is not in the allowed folders list.";
        }
        catch (ArgumentException ex)
        {
            return $"Access denied: {ex.Message}";
        }
    }

    [KernelFunction("list_files")]
    [Description("Lists all files in a folder if access is allowed.")]
    public string ListFiles([Description("The folder path to list files from")] string folderPath)
    {
        try
        {
            string normalizedPath = ValidateAndNormalizePath(folderPath);
            bool hasAccess = IsPathAccessible(normalizedPath);

            if (!hasAccess)
            {
                return $"Access denied: {normalizedPath} is not in the allowed folders list.";
            }

            // Access granted, list files
            try
            {
                if (!Directory.Exists(normalizedPath))
                {
                    return $"Error: Directory does not exist - {normalizedPath}";
                }

                var files = Directory.GetFiles(normalizedPath);
                if (files.Length == 0)
                {
                    return $"No files found in {normalizedPath}";
                }

                return $"Files in {normalizedPath}:\n" + string.Join("\n", files);
            }
            catch (UnauthorizedAccessException)
            {
                return $"Access denied: Cannot list files in {normalizedPath}";
            }
            catch (DirectoryNotFoundException)
            {
                return $"Error: Directory not found - {normalizedPath}";
            }
            catch (Exception ex)
            {
                return $"Error: Failed to list files - {ex.Message}";
            }
        }
        catch (ArgumentException ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    private string ValidateAndNormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid path format - {ex.Message}", nameof(path), ex);
        }
        catch (PathTooLongException)
        {
            throw new ArgumentException("Path exceeds maximum length.", nameof(path));
        }
        catch (NotSupportedException)
        {
            throw new ArgumentException("Path format is not supported.", nameof(path));
        }
    }

    private bool IsPathAccessible(string normalizedPath)
    {
        return allowedFolders.Any(allowed =>
        {
            var normalizedAllowed = Path.GetFullPath(allowed)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var normalizedCheck = normalizedPath
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            // Check if path is inside allowed folder OR is the folder itself
            return normalizedCheck.StartsWith(normalizedAllowed, StringComparison.OrdinalIgnoreCase) ||
                   normalizedPath.Equals(normalizedAllowed.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        });
    }
}
