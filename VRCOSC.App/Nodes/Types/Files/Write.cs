// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("File Write", "Files")]
public sealed class FileWriteTextNode : TryHandleFilePathActionAsyncNode
{
    public ValueInput<string?> Contents = new();

    protected override async Task<bool> TryHandlePathAsync(string path, IPulseContext c)
    {
        await c.Run(File.WriteAllTextAsync(path, Contents.Read(c)));
        return true;
    }
}

[Node("File Append", "Files")]
public sealed class FileAppendTextNode : TryHandleFilePathActionAsyncNode
{
    public ValueInput<string?> Contents = new();

    protected override async Task<bool> TryHandlePathAsync(string path, IPulseContext c)
    {
        await c.Run(File.AppendAllTextAsync(path, Contents.Read(c)));
        return true;
    }
}

[Node("File Delete", "Files")]
public sealed class FileDeleteNode : TryHandleFilePathActionNode
{
    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        File.Delete(path);
        return true;
    }
}

[Node("File Copy", "Files")]
public sealed class FileCopyNode() : TryHandleFilePathActionNode("Source Path")
{
    public ValueInput<string?> DestinationPath = new();
    public ValueInput<bool> Overwrite = new();

    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        var dest = DestinationPath.Read(c);
        if (string.IsNullOrEmpty(dest)) return false;

        var overwrite = Overwrite.Read(c);

        File.Copy(path, dest, overwrite);
        return true;
    }
}

[Node("File Move", "Files")]
public sealed class FileMoveNode() : TryHandleFilePathActionNode("Source Path")
{
    public ValueInput<string?> DestinationPath = new();
    public ValueInput<bool> Overwrite = new();

    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        var dest = DestinationPath.Read(c);
        if (string.IsNullOrEmpty(dest)) return false;

        var overwrite = Overwrite.Read(c);

        File.Move(path, dest, overwrite);
        return true;
    }
}

[Node("File Set Attributes", "Files")]
public sealed class FileSetAttributesNode : TryHandleFilePathActionNode
{
    public ValueInput<FileAttributes> Attributes = new(modes: ValueInputMode.Connection);

    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        File.SetAttributes(path, Attributes.Read(c));
        return true;
    }
}

[Node("File Set Creation Time", "Files")]
public sealed class FileSetCreationTimeNode : TryHandleFilePathActionNode
{
    public ValueInput<DateTime> CreationTime = new();

    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        File.SetCreationTime(path, CreationTime.Read(c));
        return true;
    }
}

[Node("File Set Last Access Time", "Files")]
public sealed class FileSetLastAccessTimeNode : TryHandleFilePathActionNode
{
    public ValueInput<DateTime> LastAccessTime = new();

    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        File.SetLastAccessTime(path, LastAccessTime.Read(c));
        return true;
    }
}

[Node("File Set Last Write Time", "Files")]
public sealed class FileSetLastWriteTimeNode : TryHandleFilePathActionNode
{
    public ValueInput<DateTime> LastWriteTime = new();

    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        File.SetLastWriteTime(path, LastWriteTime.Read(c));
        return true;
    }
}

[Node("File Replace", "Files")]
public sealed class FileReplaceNode() : TryHandleFilePathActionNode("Source Path")
{
    public ValueInput<string?> DestinationPath = new();
    public ValueInput<string?> BackupPath = new();
    public ValueInput<bool> IgnoreMetadataErrors = new();

    protected override bool IsPathValid([NotNullWhen(true)] string? path) => base.IsPathValid(path) && File.Exists(path);

    protected override bool TryHandlePath(string path, IPulseContext c)
    {
        var dest = DestinationPath.Read(c);
        if (string.IsNullOrEmpty(dest) || !File.Exists(dest)) return false;

        var backup = BackupPath.Read(c);
        var ignoreErrors = IgnoreMetadataErrors.Read(c);

        File.Replace(path, dest, backup, ignoreErrors);
        return true;
    }
}