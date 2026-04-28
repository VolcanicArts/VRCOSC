// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.VRChat.Logs;

public class LogReaderState
{
    public string WorldId = null!;
    public string WorldName = null!;
    public string? InstanceId;
    public string? InstanceOwnerId;
    public string? InstanceNumber;
    public InstanceType InstanceType;
    public InstanceRegion InstanceRegion;
    public bool InstanceAgeGated;
    public bool InstanceHasQueue;
}