using System.Collections.Generic;

public interface IFileProvider
{
    /// <summary>
    /// Récupère le contenu d'un fichier sous forme de chaîne, à partir d'un chemin relatif.
    /// </summary>
    string GetFileContent(string relativePath);
    /// <summary>
    /// Récupère la liste des fichiers dans un dossier relatif.
    /// </summary>
    IEnumerable<string> ListFiles(string relativeDirectory, string searchPattern = "*.*");
}
