// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("File Exists", "Files")]
[NodeCollapsed]
public sealed class FileExistsNode() : SimpleValueTransformNode<string, bool>(File.Exists);

[Node("File Read", "Files")]
public sealed class FileReadTextNode : TryValueComputeAsyncNode<string>
{
    public ValueInput<string> Path = new();

    protected override async Task<Result<string>> TryComputeValueAsync(PulseContext c)
    {
        var path = Path.Read(c);

        if (!File.Exists(path))
            return Result<string>.Fail();

        return await File.ReadAllTextAsync(path, c.Token);
    }
}

[Node("File Get Attributes", "Files")]
public sealed class FileGetAttributesNode() : TryValueComputeNode<FileAttributes>("Attributes")
{
    public ValueInput<string> Path = new();

    protected override Result<FileAttributes> TryComputeValue(PulseContext c)
    {
        var path = Path.Read(c);

        if (!File.Exists(path))
            return Result<FileAttributes>.Fail();

        return File.GetAttributes(path);
    }
}

[Node("File Get Creation Time", "Files")]
public sealed class FileGetCreationTimeNode() : TryValueComputeNode<DateTime>("Creation Time")
{
    public ValueInput<string> Path = new();

    protected override Result<DateTime> TryComputeValue(PulseContext c)
    {
        var path = Path.Read(c);

        if (!File.Exists(path))
            return Result<DateTime>.Fail();

        return File.GetCreationTime(path);
    }
}

[Node("File Get Last Access Time", "Files")]
public sealed class FileGetLastAccessTimeNode() : TryValueComputeNode<DateTime>("Last Access Time")
{
    public ValueInput<string> Path = new();

    protected override Result<DateTime> TryComputeValue(PulseContext c)
    {
        var path = Path.Read(c);

        if (!File.Exists(path))
            return Result<DateTime>.Fail();

        return File.GetLastAccessTime(path);
    }
}

[Node("File Get Last Write Time", "Files")]
public sealed class FileGetLastWriteTimeNode() : TryValueComputeNode<DateTime>("Last Write Time")
{
    public ValueInput<string> Path = new();

    protected override Result<DateTime> TryComputeValue(PulseContext c)
    {
        var path = Path.Read(c);

        if (!File.Exists(path))
            return Result<DateTime>.Fail();

        return File.GetLastWriteTime(path);
    }
}