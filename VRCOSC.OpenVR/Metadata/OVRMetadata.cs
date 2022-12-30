using Valve.VR;

namespace VRCOSC.OpenVR.Metadata;

public class OVRMetadata
{
    public EVRApplicationType ApplicationType { get; init; }
    public string ApplicationManifest { get; init; }
    public string ActionManifest { get; init; }
}
