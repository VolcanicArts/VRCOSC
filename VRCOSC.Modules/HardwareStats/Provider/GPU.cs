// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Modules.HardwareStats.Provider;

public class GPU : Component
{
    public float Usage { get; internal set; }
    public int Temperature { get; internal set; }
}
