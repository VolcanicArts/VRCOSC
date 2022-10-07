// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public class InputParameterData
{
    public readonly Type Type;
    public readonly ActionMenu ActionMenu;

    public InputParameterData(Type type, ActionMenu actionMenu = ActionMenu.None)
    {
        Type = type;
        ActionMenu = actionMenu;
    }
}

public sealed class ButtonInputParameterData : InputParameterData
{
    public ButtonInputParameterData()
        : base(typeof(bool), ActionMenu.Button)
    {
    }
}

public sealed class RadialInputParameterData : InputParameterData
{
    public float PreviousValue;

    public RadialInputParameterData()
        : base(typeof(float), ActionMenu.Radial)
    {
    }
}

public sealed class VRChatRadialPuppet
{
    /// <summary>
    /// The value that has just been received
    /// </summary>
    public readonly float Value;

    /// <summary>
    /// The value that was received last time
    /// </summary>
    public readonly float PreviousValue;

    /// <summary>
    /// The change in value between now and last receive
    /// </summary>
    public readonly float DeltaValue;

    public VRChatRadialPuppet(float value, float previousValue)
    {
        Value = value;
        PreviousValue = previousValue;
        DeltaValue = value - previousValue;
    }
}
