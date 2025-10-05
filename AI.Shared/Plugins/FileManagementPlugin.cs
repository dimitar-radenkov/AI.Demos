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
        var normalizedPath = Path.GetFullPath(folderPath);
        var hasAccess = allowedFolders.Any(allowed => 
            normalizedPath.StartsWith(Path.GetFullPath(allowed), StringComparison.OrdinalIgnoreCase));
        
        return hasAccess 
            ? $"Access granted: {normalizedPath}" 
            : $"Access denied: {normalizedPath} is not in the allowed folders list.";
    }
}
