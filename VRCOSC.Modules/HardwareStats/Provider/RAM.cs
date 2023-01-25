// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Modules.HardwareStats.Provider;

public class RAM : Component
{
    public float Usage { get; internal set; }
    public float Used { get; internal set; }
    public float Available { get; internal set; }
    public float Total => Used + Available;
}
