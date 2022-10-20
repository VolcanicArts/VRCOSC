// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using NUnit.Framework;
using VRCOSC.Desktop;

namespace VRCOSC.Game.Tests.Visual;

public class VRCOSCGame : VRCOSCTestScene
{
    [SetUp]
    public void SetUp()
    {
        AddGame(new VRCOSCGameDesktop());
    }
}
