// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.SDK.Parameters;

[Flags]
public enum ParameterMode
{
    /// <summary>
    /// Has the ability to read from VRChat
    /// </summary>
    Read = 1 << 0,

    /// <summary>
    /// Has the ability to write to VRChat
    /// </summary>
    Write = 1 << 1,

    /// <summary>
    /// Has the ability to read from and write to VRChat
    /// </summary>
    ReadWrite = Read | Write
}

public static class ParameterModeExtensions
{
    public static string ToReadableName(this ParameterMode mode) => mode switch
    {
        ParameterMode.Read => "Read",
        ParameterMode.Write => "Write",
        ParameterMode.ReadWrite => "Read/Write",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };
}
