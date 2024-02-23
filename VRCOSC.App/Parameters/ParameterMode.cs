// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;

namespace VRCOSC.App.Parameters;

[Flags]
public enum ParameterMode
{
    /// <summary>
    /// Has the ability to read from VRChat
    /// </summary>
    [Description("Read")]
    Read = 1 << 0,

    /// <summary>
    /// Has the ability to write to VRChat
    /// </summary>
    [Description("Write")]
    Write = 1 << 1,

    /// <summary>
    /// Has the ability to read from and write to VRChat
    /// </summary>
    [Description("Read/Write")]
    ReadWrite = Read | Write
}
