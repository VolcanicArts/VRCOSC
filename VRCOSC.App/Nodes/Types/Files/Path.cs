// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Files;

[Node("Path Join", "Files")]
public sealed class PathJoinNode : Node
{
    public ValueInputList<string> Paths = new();
    public ValueOutput<string> Result = new();

    protected override Task Process(PulseContext c)
    {
        var result = Path.Join(Paths.Read(c).ToArray());
        Result.Write(result, c);
        return Task.CompletedTask;
    }
}

[Node("Path Exists", "Files")]
[NodeCollapsed]
public sealed class PathExistsNode() : SimpleValueTransformNode<string, bool>(Path.Exists);

[Node("Get File Name", "Files")]
[NodeCollapsed]
public sealed class PathGetFileNameNode() : SimpleValueTransformNode<string>(Path.GetFileName);

[Node("Get Directory Name", "Files")]
[NodeCollapsed]
public sealed class PathGetDirectoryNameNode : ValueComputeNode<string?>
{
    public ValueInput<string> Path = new();

    protected override string? ComputeValue(PulseContext c)
    {
        try
        {
            return System.IO.Path.GetDirectoryName(Path.Read(c));
        }
        catch
        {
            return null;
        }
    }
}

[Node("Get Extension", "Files")]
[NodeCollapsed]
public sealed class PathGetExtensionNode() : SimpleValueTransformNode<string>(Path.GetExtension);

[Node("Temp Path", "Files")]
public sealed class PathTempPathConstantNode() : ConstantNode<string>(Path.GetTempPath());