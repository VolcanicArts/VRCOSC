// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Modules.FaceTracking.Interface.Lips;

namespace VRCOSC.Modules.FaceTracking.Interface;

public class SRanipalInterface
{
    public readonly SRanipalAPIInterface APIInterface = new();
    public readonly EyeTrackingData EyeData = new();
    public readonly LipTrackingData LipData = new();

    public void Initialise(bool eye, bool lip)
    {
        APIInterface.Initialise(eye, lip);
        EyeData.Initialise();
        LipData.Initialise();
    }

    public void Update()
    {
        APIInterface.Update();
        EyeData.Update(APIInterface.EyeData);
        LipData.Update(APIInterface.LipData);
    }

    public void Release()
    {
        APIInterface.Release();
    }
}
