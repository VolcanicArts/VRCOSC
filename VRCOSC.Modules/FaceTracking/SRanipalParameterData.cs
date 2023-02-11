// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Modules.FaceTracking;

public class SRanipalParameterData
{
    public int BoolCount;
    public bool NegativePresent;
    public bool FloatPresent;

    public bool ShouldEncode => BoolCount > 0;
}
