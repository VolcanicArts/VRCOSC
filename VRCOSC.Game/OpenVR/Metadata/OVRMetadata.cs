// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Valve.VR;

namespace VRCOSC.Game.OpenVR.Metadata;

public class OVRMetadata
{
    public EVRApplicationType ApplicationType { get; init; }
    public string ApplicationManifest { get; init; }
    public string ActionManifest { get; init; }
}
