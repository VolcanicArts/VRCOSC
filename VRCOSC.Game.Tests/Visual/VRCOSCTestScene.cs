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
        private TestSceneTestRunner.TestRunner runner;

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
