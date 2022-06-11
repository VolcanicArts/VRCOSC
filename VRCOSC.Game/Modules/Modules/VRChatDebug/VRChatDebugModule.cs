// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.VRChatDebug;

public class VRChatDebugModule : Module
{
    public override string Title => "VRChat Debug";
    public override string Description => "A debug module for taking input from all the VRChat parameters";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.White.Darken(0.5f);
    public override ModuleType Type => ModuleType.Debug;

    protected override List<Enum> InputParameters => new()
    {
        VRChatInputParameters.IsLocal,
        VRChatInputParameters.Viseme,
        VRChatInputParameters.Voice,
        VRChatInputParameters.GestureLeft,
        VRChatInputParameters.GestureRight,
        VRChatInputParameters.GestureLeftWeight,
        VRChatInputParameters.GestureRightWeight,
        VRChatInputParameters.AngularY,
        VRChatInputParameters.VelocityX,
        VRChatInputParameters.VelocityY,
        VRChatInputParameters.VelocityZ,
        VRChatInputParameters.Upright,
        VRChatInputParameters.Grounded,
        VRChatInputParameters.Seated,
        VRChatInputParameters.AFK,
        VRChatInputParameters.TrackingType,
        VRChatInputParameters.VRMode,
        VRChatInputParameters.MuteSelf,
        VRChatInputParameters.InStation
    };
}
