// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("File Exists", "Files")]
[NodeCollapsed]
public sealed class FileExistsNode() : SimpleValueTransformNode<string, bool>(File.Exists);

[Node("File Read", "Files")]
public sealed class FileReadTextNode() : HandleFilePathTransformAsyncNode<string>("Contents")
{
    protected override Task<string> HandlePathAsync(string path, PulseContext c) => File.ReadAllTextAsync(path, c.Token);
}

[Node("File Get Attributes", "Files")]
public sealed class FileGetAttributesNode() : SimpleHandleFilePathTransformNode<FileAttributes>(File.GetAttributes, "Attributes");

[Node("File Get Creation Time", "Files")]
public sealed class FileGetCreationTimeNode() : SimpleHandleFilePathTransformNode<DateTime>(File.GetCreationTime, "Creation Time");

[Node("File Get Last Access Time", "Files")]
public sealed class FileGetLastAccessTimeNode() : SimpleHandleFilePathTransformNode<DateTime>(File.GetLastAccessTime, "Last Access Time");

[Node("File Get Last Write Time", "Files")]
public sealed class FileGetLastWriteTimeNode() : SimpleHandleFilePathTransformNode<DateTime>(File.GetLastWriteTime, "Last Write Time");