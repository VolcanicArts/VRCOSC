// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.OSC.VRChat;

namespace VRCOSC.Game.Modules;

public class ParameterMetadata
{
    public readonly ParameterMode Mode;
    public readonly string Name;
    public readonly string Description;
    public readonly Type ExpectedType;

    public string FormattedAddress => $"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}/{Name}";

    public ParameterMetadata(ParameterMode mode, string name, string description, Type expectedType)
    {
        Mode = mode;
        Name = name;
        Description = description;
        ExpectedType = expectedType;
    }
}

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
