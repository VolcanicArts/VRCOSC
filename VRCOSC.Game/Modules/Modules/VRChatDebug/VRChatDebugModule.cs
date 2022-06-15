// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

    public override void CreateAttributes()
    {
        RegisterInputParameter(VRChatInputParameters.IsLocal, typeof(bool));
        RegisterInputParameter(VRChatInputParameters.Viseme, typeof(int));
        RegisterInputParameter(VRChatInputParameters.Voice, typeof(float));
        RegisterInputParameter(VRChatInputParameters.GestureLeft, typeof(int));
        RegisterInputParameter(VRChatInputParameters.GestureRight, typeof(int));
        RegisterInputParameter(VRChatInputParameters.GestureLeftWeight, typeof(float));
        RegisterInputParameter(VRChatInputParameters.GestureRightWeight, typeof(float));
        RegisterInputParameter(VRChatInputParameters.AngularY, typeof(int));
        RegisterInputParameter(VRChatInputParameters.VelocityX, typeof(int));
        RegisterInputParameter(VRChatInputParameters.VelocityY, typeof(int));
        RegisterInputParameter(VRChatInputParameters.VelocityZ, typeof(int));
        RegisterInputParameter(VRChatInputParameters.Upright, typeof(bool));
        RegisterInputParameter(VRChatInputParameters.Grounded, typeof(bool));
        RegisterInputParameter(VRChatInputParameters.Seated, typeof(bool));
        RegisterInputParameter(VRChatInputParameters.AFK, typeof(bool));
        RegisterInputParameter(VRChatInputParameters.TrackingType, typeof(int));
        RegisterInputParameter(VRChatInputParameters.VRMode, typeof(int));
        RegisterInputParameter(VRChatInputParameters.MuteSelf, typeof(bool));
        RegisterInputParameter(VRChatInputParameters.InStation, typeof(bool));
    }
}
