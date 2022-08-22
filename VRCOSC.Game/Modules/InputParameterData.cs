// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

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

public class RadialInputParameterData : InputParameterData
{
    public float PreviousValue;

    public RadialInputParameterData()
        : base(typeof(float), ActionMenu.Radial)
    {
    }
}

public class VRChatRadialPuppet
{
    /// <summary>
    /// The value that has just been received
    /// </summary>
    public float Value;

    /// <summary>
    /// The value that was received last time
    /// </summary>
    public float PreviousValue;

    /// <summary>
    /// The change in value between now and last receive
    /// </summary>
    public float DeltaValue => Value - PreviousValue;

    public VRChatRadialPuppet(float value, float previousValue)
    {
        Value = value;
        PreviousValue = previousValue;
    }
}
