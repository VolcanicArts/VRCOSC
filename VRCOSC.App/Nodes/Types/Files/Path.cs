// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Linq;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("Path Join", "Files/Paths")]
public sealed class PathJoinNode : ValueComputeNode<string>
{
    public ValueInputList<string?> Paths = new();

    protected override string ComputeValue(IPulseContext c) => Path.Join(Paths.Read(c).ToArray());
}

[Node("Path Exists", "Files/Paths")]
[NodeCollapsed]
public sealed class PathExistsNode() : SimpleValueTransformNode<string?, bool>(Path.Exists);

[Node("Get File Name", "Files/Paths")]
[NodeCollapsed]
public sealed class PathGetFileNameNode() : SimpleValueTransformNode<string?>(Path.GetFileName);

[Node("Get File Name Without Extension", "Files/Paths")]
[NodeCollapsed]
public sealed class PathGetFileNameWithoutExtensionNode() : SimpleValueTransformNode<string?>(Path.GetFileNameWithoutExtension);

[Node("Get Directory Name", "Files/Paths")]
[NodeCollapsed]
public sealed class PathGetDirectoryNameNode() : SimpleValueTransformNode<string?>(Path.GetDirectoryName);

[Node("Get Extension", "Files/Paths")]
[NodeCollapsed]
public sealed class PathGetExtensionNode() : SimpleValueTransformNode<string?>(Path.GetExtension);

[Node("Get Full Path", "Files/Paths")]
[NodeCollapsed]
public sealed class PathGetFullPathNode() : SimpleValueTransformNode<string>(Path.GetFullPath);

[Node("Get Path Root", "Files/Paths")]
[NodeCollapsed]
public sealed class PathGetPathRootNode() : SimpleValueTransformNode<string?>(Path.GetPathRoot);

[Node("Has Extension", "Files/Paths")]
[NodeCollapsed]
public sealed class PathHasExtensionNode() : SimpleValueTransformNode<string, bool>(Path.HasExtension);

[Node("Is Path Fully Qualified", "Files/Paths")]
[NodeCollapsed]
public sealed class PathIsPathFullyQualifiedNode() : SimpleValueTransformNode<string, bool>(Path.IsPathFullyQualified);

[Node("Is Path Rooted", "Files/Paths")]
[NodeCollapsed]
public sealed class PathIsPathRootedNode() : SimpleValueTransformNode<string, bool>(Path.IsPathRooted);

[Node("Change Extension", "Files/Paths")]
public sealed class PathChangeExtensionNode() : SimpleResultComputeNode<string?>(Path.ChangeExtension, "Path", "Extension");

[Node("Get Relative Path", "Files/Paths")]
public sealed class PathGetRelativePathNode() : SimpleResultComputeNode<string, string, string?>(Path.GetRelativePath, "Relative To", "Path");

[Node("Temp Path", "Files/Paths")]
[NodeCollapsed]
public sealed class PathTempPathConstantNode() : ConstantNode<string>(Path.GetTempPath());

[Node("Create Temp File", "Files/Paths")]
public sealed class PathCreateTempFileNode() : SimpleActionValueComputeNode<string>(Path.GetTempFileName, "Path");

[Node("Get Random File Name", "Files/Paths")]
public sealed class PathGetRandomFileNameNode() : SimpleActionValueComputeNode<string>(Path.GetRandomFileName, "Name");

[Node("Directory Separator Char", "Files/Paths")]
public sealed class PathDirectorySeparatorCharNode() : ConstantNode<char>(Path.DirectorySeparatorChar);

[Node("Alt Directory Separator Char", "Files/Paths")]
public sealed class PathAltDirectorySeparatorCharNode() : ConstantNode<char>(Path.AltDirectorySeparatorChar);

[Node("Volume Separator Char", "Files/Paths")]
public sealed class PathVolumeSeparatorCharNode() : ConstantNode<char>(Path.VolumeSeparatorChar);

[Node("Path Separator Char", "Files/Paths")]
public sealed class PathPathSeparatorCharNode() : ConstantNode<char>(Path.PathSeparator);

[Node("Trim Ending Directory Separator", "Files/Paths")]
[NodeCollapsed]
public sealed class PathTrimEndingDirectorySeparatorNode() : SimpleValueTransformNode<string>(Path.TrimEndingDirectorySeparator);

[Node("Ends In Directory Separator", "Files/Paths")]
[NodeCollapsed]
public sealed class PathEndsInDirectorySeparatorNode() : SimpleValueTransformNode<string, bool>(Path.EndsInDirectorySeparator);