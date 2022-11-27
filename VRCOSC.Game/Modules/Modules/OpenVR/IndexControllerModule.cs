// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.OpenVR;

public class IndexControllerModule : Module
{
    public override string Title => "Index Controller Stats";
    public override string Description => "Gets finger and thumb values and positions from Index controllers";
    public override string Author => "VolcanicArts";
    public override ModuleType ModuleType => ModuleType.General;
    protected override int DeltaUpdate => 50;

    protected override Task OnUpdate()
    {
        //OpenVrInterface.GetCurrentButtonPressRightController();

        return Task.CompletedTask;
    }
}
