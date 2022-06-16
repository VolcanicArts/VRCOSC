// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.VRChatDebug;

// ReSharper disable once InconsistentNaming
public class VRChatDebugModule : Module
{
    public override string Title => "VRChat Debug";
    public override string Description => "A debug module for taking input from all the VRChat parameters";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.White.Darken(0.5f);
    public override ModuleType ModuleType => ModuleType.Debug;

    public override void CreateAttributes()
    {
        RegisterInputParameter(VRChatInputParameter.IsLocal, typeof(bool));
        RegisterInputParameter(VRChatInputParameter.Viseme, typeof(int));
        RegisterInputParameter(VRChatInputParameter.Voice, typeof(float));
        RegisterInputParameter(VRChatInputParameter.GestureLeft, typeof(int));
        RegisterInputParameter(VRChatInputParameter.GestureRight, typeof(int));
        RegisterInputParameter(VRChatInputParameter.GestureLeftWeight, typeof(float));
        RegisterInputParameter(VRChatInputParameter.GestureRightWeight, typeof(float));
        RegisterInputParameter(VRChatInputParameter.AngularY, typeof(int));
        RegisterInputParameter(VRChatInputParameter.VelocityX, typeof(int));
        RegisterInputParameter(VRChatInputParameter.VelocityY, typeof(int));
        RegisterInputParameter(VRChatInputParameter.VelocityZ, typeof(int));
        RegisterInputParameter(VRChatInputParameter.Upright, typeof(bool));
        RegisterInputParameter(VRChatInputParameter.Grounded, typeof(bool));
        RegisterInputParameter(VRChatInputParameter.Seated, typeof(bool));
        RegisterInputParameter(VRChatInputParameter.AFK, typeof(bool));
        RegisterInputParameter(VRChatInputParameter.TrackingType, typeof(int));
        RegisterInputParameter(VRChatInputParameter.VRMode, typeof(int));
        RegisterInputParameter(VRChatInputParameter.MuteSelf, typeof(bool));
        RegisterInputParameter(VRChatInputParameter.InStation, typeof(bool));
    }
}
