// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.
// This class has been taken and modified from https://github.com/ppy/osu-framework
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace VRCOSC.App.Utils;

public class Storage
{
    public string BasePath { get; }

    public Storage(string path, string? subfolder = null)
    {
        BasePath = path;

        if (BasePath == null)
            throw new InvalidOperationException($"{nameof(BasePath)} not correctly initialized!");

        if (!string.IsNullOrEmpty(subfolder))
            BasePath = Path.Combine(BasePath, filenameStrip(subfolder));

        return;

        static string filenameStrip(string entry)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                entry = entry.Replace(c.ToString(), string.Empty);
            return entry;
        }
    }

    /// <summary>
    /// Get a usable filesystem path for the provided incomplete path.
    /// </summary>
    /// <param name="path">An incomplete path, usually provided as user input.</param>
    /// <param name="createIfNotExisting">Create the path if it doesn't already exist.</param>
    /// <returns>A usable filesystem path.</returns>
    public string GetFullPath(string path, bool createIfNotExisting = false)
    {
        path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        var basePath = Path.GetFullPath(BasePath).TrimEnd(Path.DirectorySeparatorChar);
        var resolvedPath = Path.GetFullPath(Path.Combine(basePath, path));

        if (!resolvedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException($"\"{resolvedPath}\" traverses outside of \"{basePath}\" and is probably malformed");

        if (createIfNotExisting) Directory.CreateDirectory(Path.GetDirectoryName(resolvedPath)!);
        return resolvedPath;
    }

    public void CopyTo(string fullDestinationDirPath, bool recursive = true)
    {
        copyDirectory(BasePath, fullDestinationDirPath, recursive);
    }

    private void copyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        var dir = new DirectoryInfo(sourceDir);
        var dirs = dir.GetDirectories();

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        if (!recursive) return;

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            copyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }

    /// <summary>
    /// Check whether a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>Whether a file exists.</returns>
    public bool Exists(string path) => File.Exists(GetFullPath(path));

    /// <summary>
    /// Check whether a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>Whether a directory exists.</returns>
    public bool ExistsDirectory(string path) => Directory.Exists(GetFullPath(path));

    /// <summary>
    /// Delete a file.
    /// </summary>
    /// <param name="path">The path of the file to delete.</param>
    public void Delete(string path)
    {
        path = GetFullPath(path);

        if (File.Exists(path))
            File.Delete(path);
    }

    /// <summary>
    /// Delete a directory and all its contents recursively.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    public void DeleteDirectory(string path)
    {
        path = GetFullPath(path);

        // handles the case where the directory doesn't exist, which will throw a DirectoryNotFoundException.
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    /// <summary>
    /// Retrieve a list of files at the specified path.
    /// </summary>
    /// <param name="path">The path to list.</param>
    /// <param name="pattern">An optional search pattern. Accepts "*" wildcard.</param>
    /// <returns>A list of files in the path, relative to the path of this storage.</returns>
    public IEnumerable<string> GetFiles(string path, string pattern = "*") => getRelativePaths(Directory.GetFiles(GetFullPath(path), pattern));

    /// <summary>
    /// Retrieve a list of directories at the specified path.
    /// </summary>
    /// <param name="path">The path to list.</param>
    /// <returns>A list of directories in the path, relative to the path of this storage.</returns>
    public IEnumerable<string> GetDirectories(string path) => getRelativePaths(Directory.GetDirectories(GetFullPath(path)));

    private IEnumerable<string> getRelativePaths(IEnumerable<string> paths)
    {
        var basePath = Path.GetFullPath(GetFullPath(string.Empty));

        return paths.Select(Path.GetFullPath).Select(path =>
        {
            if (!path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException($"\"{path}\" does not start with \"{basePath}\" and is probably malformed");

            return path.AsSpan(basePath.Length).TrimStart(Path.DirectorySeparatorChar).ToString();
        });
    }

    /// <summary>
    /// Retrieve a <see cref="Storage"/> for a contained directory.
    /// Creates the path if not existing.
    /// </summary>
    /// <param name="path">The subdirectory to use as a root.</param>
    /// <returns>A more specific storage.</returns>
    public Storage GetStorageForDirectory(string path)
    {
        if (path.Length > 0 && !path.EndsWith(Path.DirectorySeparatorChar))
            path += Path.DirectorySeparatorChar;

        // create non-existing path.
        var fullPath = GetFullPath(path, true);

        return new Storage(fullPath);
    }

    /// <summary>
    /// Move a file from one location to another. File must exist. Destination will be overwritten if exists.
    /// </summary>
    /// <param name="from">The file path to move.</param>
    /// <param name="to">The destination path.</param>
    public void Move(string from, string to)
    {
        // Retry move operations as it can fail on windows intermittently with IOExceptions:
        // The process cannot access the file because it is being used by another process.
        File.Move(GetFullPath(from), GetFullPath(to), true);
    }

    /// <summary>
    /// Create a new file on disk, using a temporary file to write to before moving to the final location to ensure a half-written file cannot exist at the specified location.
    /// </summary>
    /// <remarks>
    /// If the target file path already exists, it will be deleted before attempting to write a new version.
    /// </remarks>
    /// <param name="path">The path of the file to create or overwrite.</param>
    /// <returns>A stream associated with the requested path. Will only exist at the specified location after the stream is disposed.</returns>
    [Pure]
    public Stream CreateFileSafely(string path)
    {
        string temporaryPath = Path.Combine(Path.GetDirectoryName(path)!, $"_{Path.GetFileName(path)}_{Guid.NewGuid()}");

        return new SafeWriteStream(temporaryPath, path, this);
    }

    /// <summary>
    /// Retrieve a stream from an underlying file inside this storage.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="access">The access requirements.</param>
    /// <param name="mode">The mode in which the file should be opened.</param>
    /// <returns>A stream associated with the requested path.</returns>
    [Pure]
    public Stream? GetStream(string path, FileAccess access = FileAccess.Read, FileMode mode = FileMode.OpenOrCreate)
    {
        path = GetFullPath(path, access != FileAccess.Read);

        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));

        switch (access)
        {
            case FileAccess.Read:
                return !File.Exists(path) ? null : File.Open(path, FileMode.Open, access, FileShare.Read);

            default:
                return new FileStream(path, mode, access);
        }
    }

    /// <summary>
    /// Uses a temporary file to ensure a file is written to completion before existing at its specified location.
    /// </summary>
    private class SafeWriteStream : FileStream
    {
        private readonly string temporaryPath;
        private readonly string finalPath;
        private readonly Storage storage;

        public SafeWriteStream(string temporaryPath, string finalPath, Storage storage)
            : base(storage.GetFullPath(temporaryPath, true), FileMode.Create, FileAccess.Write)
        {
            this.temporaryPath = temporaryPath;
            this.finalPath = finalPath;
            this.storage = storage;
        }

        private bool isDisposed;

        protected override void Dispose(bool disposing)
        {
            // Don't perform any custom logic when arriving via the finaliser.
            // We assume that all usages of `SafeWriteStream` correctly follow a local disposal pattern.
            if (!disposing)
            {
                base.Dispose(false);
                return;
            }

            if (!isDisposed)
            {
                try
                {
                    Flush(true);
                }
                catch
                {
                    // this may fail due to a lower level file access issue.
                    // we don't want to throw in disposal though.
                }
            }

            base.Dispose(true);

            if (!isDisposed)
            {
                storage.Delete(finalPath);
                storage.Move(temporaryPath, finalPath);

                isDisposed = true;
            }
        }
    }
}