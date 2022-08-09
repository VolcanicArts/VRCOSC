// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using NUnit.Framework;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game.Tests.Visual;

public class TestUpdaterManager : VRCOSCTestScene
{
    private VRCOSCUpdateManager updateManager = null!;

    [SetUp]
    public void SetUp()
    {
        Clear();
        Add(updateManager = new DummyUpdateManager());
    }

    [Test]
    public void TestPhasesAuto()
    {
        float progress = 0f;

        AddStep("start", () => updateManager.Show());

        AddStep("phase check", () =>
        {
            updateManager.SetPhase(UpdatePhase.Check);
            progress = 0f;
        });

        AddRepeatStep("add progress", () => updateManager.UpdateProgress(progress += 0.1f), 10);

        AddStep("phase download", () =>
        {
            updateManager.SetPhase(UpdatePhase.Download);
            progress = 0f;
        });
        AddRepeatStep("add progress", () => updateManager.UpdateProgress(progress += 0.1f), 10);

        AddStep("phase install", () =>
        {
            updateManager.SetPhase(UpdatePhase.Install);
            progress = 0f;
        });
        AddRepeatStep("add progress", () => updateManager.UpdateProgress(progress += 0.1f), 10);

        AddStep("phase success", () => updateManager.SetPhase(UpdatePhase.Success));
    }

    [Test]
    public void TestPhasesManual()
    {
        AddStep("start", () => updateManager.Show());
        AddStep("phase check", () => updateManager.SetPhase(UpdatePhase.Check));
        AddStep("phase download", () => updateManager.SetPhase(UpdatePhase.Download));
        AddStep("phase install", () => updateManager.SetPhase(UpdatePhase.Install));
        AddStep("phase success", () => updateManager.SetPhase(UpdatePhase.Success));
        AddStep("phase fail", () => updateManager.SetPhase(UpdatePhase.Fail));
        AddStep("end", () => updateManager.Hide());
        AddSliderStep("progress bar", 0f, 1f, 0f, v => updateManager?.UpdateProgress(v));
    }

    private class DummyUpdateManager : VRCOSCUpdateManager
    {
        protected override void RequestRestart()
        {
        }

        public override Task CheckForUpdate(bool useDelta = true)
        {
            return Task.CompletedTask;
        }
    }
}
