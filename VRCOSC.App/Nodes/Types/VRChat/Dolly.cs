// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.Dolly;

namespace VRCOSC.App.Nodes.Types.VRChat;

[Node("Dolly Import", "VRChat/Dolly")]
public sealed class DollyImportNode : TryActionAsyncNode
{
    public ValueInput<string> Path = new();

    protected override Task<bool> TryActionAsync(IPulseContext c)
    {
        var path = Path.Read(c);

        if (string.IsNullOrEmpty(path) || !System.IO.Path.Exists(path))
            return Task.FromResult(false);

        DollyManager.GetInstance().Import(path);
        return Task.FromResult(true);
    }
}

[Node("Dolly Export", "VRChat/Dolly")]
public sealed class DollyExportNode : TryActionAsyncNode
{
    public ValueInput<string> Path = new();

    protected override async Task<bool> TryActionAsync(IPulseContext c)
    {
        var path = Path.Read(c);

        if (string.IsNullOrEmpty(path))
            return false;

        return await DollyManager.GetInstance().Export(path);
    }
}

[Node("Dolly Play", "VRChat/Dolly")]
public sealed class DollyPlayNode : ActionNode
{
    public ValueInput<int> Delay = new();

    protected override void DoAction(IPulseContext c)
    {
        var delay = Delay.Read(c);

        if (delay <= 0)
            DollyManager.GetInstance().Play();
        else
            DollyManager.GetInstance().PlayDelayed(delay);
    }
}

[Node("Dolly Stop", "VRChat/Dolly")]
public sealed class DollyStopNode : ActionNode
{
    protected override void DoAction(IPulseContext c) => DollyManager.GetInstance().Stop();
}