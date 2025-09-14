using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DesktopFileProvider : IFileProvider
{
    private readonly string _baseDir;
    public DesktopFileProvider(string baseDir)
    {
        _baseDir = Path.Combine(baseDir, "Content");
    }

    public string GetFileContent(string relativePath)
    {
        string fullPath = Path.Combine(_baseDir, relativePath);
        return File.ReadAllText(fullPath);
    }

    public IEnumerable<string> ListFiles(string relativeDirectory, string searchPattern = "*.*")
    {
        string fullDir = Path.Combine(_baseDir, relativeDirectory);
        if (!Directory.Exists(fullDir))
            return Array.Empty<string>();
        return Directory.GetFiles(fullDir, searchPattern).Select(Path.GetFileName);
    }
}
