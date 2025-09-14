using Android.Content.Res;
using System;
using System.Collections.Generic;
using System.IO;

namespace VelomMonoGame.Android.Sources;

internal class AndroidFileProvider : IFileProvider
{
    private readonly AssetManager _assets;
    public AndroidFileProvider(AssetManager asset)
    {
        _assets = asset;
    }

    public string GetFileContent(string relativePath)
    {
        using var stream = _assets.Open(relativePath.Replace('\\', '/'));
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public IEnumerable<string> ListFiles(string relativeDirectory, string searchPattern = "*.*")
    {
        // searchPattern ignored for simplicity, you can filter manually if needed
        return _assets.List(relativeDirectory.Replace('\\', '/')) ?? Array.Empty<string>();
    }
}
