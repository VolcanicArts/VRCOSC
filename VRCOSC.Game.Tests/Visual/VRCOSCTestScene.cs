// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Testing;

namespace VRCOSC.Game.Tests.Visual;

public class VRCOSCTestScene : TestScene
{
    protected override ITestSceneTestRunner CreateRunner()
    {
        return new VRCOSCTestSceneTestRunner();
    }

    private class VRCOSCTestSceneTestRunner : VRCOSCGameBase, ITestSceneTestRunner
    {
        private TestSceneTestRunner.TestRunner runner = null!;

        public void RunTestBlocking(TestScene test)
        {
            runner.RunTestBlocking(test);
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            Add(runner = new TestSceneTestRunner.TestRunner());
        }
    }
}
