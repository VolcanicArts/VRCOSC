// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("File Write", "Files")]
public sealed class FileWriteTextNode : TryActionAsyncNode
{
    public ValueInput<string?> Path = new();
    public ValueInput<string?> Contents = new();

    protected override async Task<bool> TryActionAsync(PulseContext c)
    {
        var path = Path.Read(c);
        var contents = Contents.Read(c);

        if (string.IsNullOrEmpty(path)) return false;

        await File.WriteAllTextAsync(path, contents, c.Token);
        return true;
    }
}

[Node("File Append", "Files")]
public sealed class FileAppendTextNode : TryActionAsyncNode
{
    public ValueInput<string?> Path = new();
    public ValueInput<string?> Contents = new();

    protected override async Task<bool> TryActionAsync(PulseContext c)
    {
        var path = Path.Read(c);
        var contents = Contents.Read(c);

        if (string.IsNullOrEmpty(path)) return false;

        await File.AppendAllTextAsync(path, contents, c.Token);
        return true;
    }
}

[Node("File Delete", "Files")]
public sealed class FileDeleteNode : TryActionAsyncNode
{
    public ValueInput<string?> Path = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var path = Path.Read(c);

        if (!File.Exists(path))
            return Task.FromResult(false);

        File.Delete(path);
        return Task.FromResult(true);
    }
}

[Node("File Copy", "Files")]
public sealed class FileCopyNode : TryActionAsyncNode
{
    public ValueInput<string?> SourcePath = new();
    public ValueInput<string?> DestinationPath = new();
    public ValueInput<bool> Overwrite = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var source = SourcePath.Read(c);
        var dest = DestinationPath.Read(c);
        var overwrite = Overwrite.Read(c);

        if (string.IsNullOrEmpty(dest) || !File.Exists(source))
            return Task.FromResult(false);

        File.Copy(source, dest, overwrite);
        return Task.FromResult(true);
    }
}

[Node("File Move", "Files")]
public sealed class FileMoveNode : TryActionAsyncNode
{
    public ValueInput<string?> SourcePath = new();
    public ValueInput<string?> DestinationPath = new();
    public ValueInput<bool> Overwrite = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var source = SourcePath.Read(c);
        var dest = DestinationPath.Read(c);
        var overwrite = Overwrite.Read(c);

        if (string.IsNullOrEmpty(dest) || !File.Exists(source))
            return Task.FromResult(false);

        File.Move(source, dest, overwrite);
        return Task.FromResult(true);
    }
}

[Node("File Set Attributes", "Files")]
public sealed class FileSetAttributesNode : TryActionAsyncNode
{
    public ValueInput<string?> Path = new();
    public ValueInput<FileAttributes> Attributes = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var path = Path.Read(c);
        var attributes = Attributes.Read(c);

        if (!File.Exists(path))
            return Task.FromResult(false);

        File.SetAttributes(path, attributes);
        return Task.FromResult(true);
    }
}

[Node("File Set Creation Time", "Files")]
public sealed class FileSetCreationTimeNode : TryActionAsyncNode
{
    public ValueInput<string?> Path = new();
    public ValueInput<DateTime> CreationTime = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var path = Path.Read(c);
        var time = CreationTime.Read(c);

        if (!File.Exists(path))
            return Task.FromResult(false);

        File.SetCreationTime(path, time);
        return Task.FromResult(true);
    }
}

[Node("File Set Last Access Time", "Files")]
public sealed class FileSetLastAccessTimeNode : TryActionAsyncNode
{
    public ValueInput<string?> Path = new();
    public ValueInput<DateTime> LastAccessTime = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var path = Path.Read(c);
        var time = LastAccessTime.Read(c);

        if (!File.Exists(path))
            return Task.FromResult(false);

        File.SetLastAccessTime(path, time);
        return Task.FromResult(true);
    }
}

[Node("File Set Last Write Time", "Files")]
public sealed class FileSetLastWriteTimeNode : TryActionAsyncNode
{
    public ValueInput<string?> Path = new();
    public ValueInput<DateTime> LastWriteTime = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var path = Path.Read(c);
        var time = LastWriteTime.Read(c);

        if (!File.Exists(path))
            return Task.FromResult(false);

        File.SetLastWriteTime(path, time);
        return Task.FromResult(true);
    }
}

[Node("File Replace", "Files")]
public sealed class FileReplaceNode : TryActionAsyncNode
{
    public ValueInput<string?> SourcePath = new();
    public ValueInput<string?> DestinationPath = new();
    public ValueInput<string?> BackupPath = new();
    public ValueInput<bool> IgnoreMetadataErrors = new();

    protected override Task<bool> TryActionAsync(PulseContext c)
    {
        var source = SourcePath.Read(c);
        var dest = DestinationPath.Read(c);
        var backup = BackupPath.Read(c);
        var ignoreErrors = IgnoreMetadataErrors.Read(c);

        if (!File.Exists(source) || !File.Exists(dest))
            return Task.FromResult(false);

        File.Replace(source, dest, backup, ignoreErrors);
        return Task.FromResult(true);
    }
}