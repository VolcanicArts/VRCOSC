// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Modules;

public enum ParameterMode
{
    /// <summary>
    /// Has the ability to read from VRChat's value
    /// </summary>
    Read = 1 << 0,

    /// <summary>
    /// Has the ability to write to VRChat's value
    /// </summary>
    Write = 1 << 1,

    /// <summary>
    /// Has the ability to read from and write to VRChat's value
    /// </summary>
    ReadWrite = Read | Write
}
