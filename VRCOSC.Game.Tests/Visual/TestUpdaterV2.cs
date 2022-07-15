// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using NUnit.Framework;
using VRCOSC.Game.Graphics.UpdaterV2;

namespace VRCOSC.Game.Tests.Visual;

public class TestUpdaterV2 : VRCOSCTestScene
{
    [SetUp]
    public void SetUp()
    {
        Clear();
        Add(new UpdaterScreen());
    }
}
