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
public sealed class PathExistsNode : Node
{
    public ValueInput<string> Path = new();
    public ValueOutput<bool> Exists = new();

    protected override Task Process(PulseContext c)
    {
        Exists.Write(System.IO.Path.Exists(Path.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Get File Name", "Files")]
[NodeCollapsed]
public sealed class PathGetFileNameNode : Node
{
    public ValueInput<string> Path = new();
    public ValueOutput<string> Name = new();

    protected override Task Process(PulseContext c)
    {
        Name.Write(System.IO.Path.GetFileName(Path.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Get Directory Name", "Files")]
[NodeCollapsed]
public sealed class PathGetDirectoryNameNode : Node
{
    public ValueInput<string> Path = new();
    public ValueOutput<string> Name = new();

    protected override Task Process(PulseContext c)
    {
        try
        {
            Name.Write(System.IO.Path.GetDirectoryName(Path.Read(c))!, c);
        }
        catch
        {
            Name.Write(null!, c);
        }

        return Task.CompletedTask;
    }
}

[Node("Get Extension", "Files")]
[NodeCollapsed]
public sealed class PathGetExtensionNode : Node
{
    public ValueInput<string> Path = new();
    public ValueOutput<string> Extension = new();

    protected override Task Process(PulseContext c)
    {
        Extension.Write(System.IO.Path.GetExtension(Path.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Temp Path", "Files")]
public sealed class PathTempPathConstantNode : ConstantNode<string>
{
    protected override string GetValue()
    {
        try
        {
            return Path.GetTempPath();
        }
        catch
        {
            return null!;
        }
    }
}