// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes.Types.Player;

[Node("Player Movement", "Player/Info")]
public sealed class PlayerMovementNode : Node, IUpdateNode
{
    public ValueOutput<Vector3> Velocity = new();
    public ValueOutput<float> AngularY = new("Angular Y");
    public ValueOutput<float> Upright = new();

    protected override Task Process(PulseContext c)
    {
        var player = c.GetPlayer();

        Velocity.Write(new Vector3(player.VelocityX, player.VelocityY, player.VelocityZ), c);
        AngularY.Write(player.AngularY, c);
        Upright.Write(player.Upright, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Player Gesture", "Player/Info")]
public sealed class PlayerGestureNode : Node, IUpdateNode
{
    public ValueOutput<GestureType> LeftType = new("Left Type");
    public ValueOutput<float> LeftWeight = new("Left Weight");
    public ValueOutput<GestureType> RightType = new("Right Type");
    public ValueOutput<float> RightWeight = new("Right Weight");

    protected override Task Process(PulseContext c)
    {
        var player = c.GetPlayer();

        LeftType.Write(player.GestureTypeLeft, c);
        RightType.Write(player.GestureTypeRight, c);
        LeftWeight.Write(player.GestureLeftWeight, c);
        RightWeight.Write(player.GestureRightWeight, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Player Voice", "Player/Info")]
public sealed class PlayerVoiceNode : Node, IUpdateNode
{
    public ValueOutput<Viseme> Viseme = new();
    public ValueOutput<float> Voice = new();

    protected override Task Process(PulseContext c)
    {
        var player = c.GetPlayer();

        Viseme.Write(player.Viseme, c);
        Voice.Write(player.Voice, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Player Identity", "Player/Info")]
public sealed class PlayerIdentityNode : Node, IUpdateNode
{
    public ValueOutput<bool> IsVR = new("Is VR");
    public ValueOutput<bool> IsMuted = new("Is Muted");
    public ValueOutput<bool> Earmuffs = new();
    public ValueOutput<bool> AFK = new();
    public ValueOutput<bool> InStation = new("In Station");
    public ValueOutput<bool> Seated = new();
    public ValueOutput<bool> Grounded = new();
    public ValueOutput<TrackingType> TrackingType = new("Tracking Type");

    protected override Task Process(PulseContext c)
    {
        var player = c.GetPlayer();

        IsVR.Write(player.IsVR, c);
        IsMuted.Write(player.IsMuted, c);
        Earmuffs.Write(player.Earmuffs, c);
        AFK.Write(player.AFK, c);
        InStation.Write(player.InStation, c);
        Seated.Write(player.Seated, c);
        Grounded.Write(player.Grounded, c);
        TrackingType.Write(player.TrackingType, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}

[Node("Player Size", "Player/Info")]
public sealed class PlayerSizeNode : Node, IUpdateNode
{
    public ValueOutput<bool> ScaleModified = new("Scale Modified");
    public ValueOutput<float> ScaleFactor = new("Scale Factor");
    public ValueOutput<float> ScaleFactorInverse = new("Scale Factor Inverse");
    public ValueOutput<float> EyeHeightAsMeters = new("Eye Height As Meters");
    public ValueOutput<float> EyeHeightAsPercent = new("Eye Height As Percent");

    protected override Task Process(PulseContext c)
    {
        var player = c.GetPlayer();

        ScaleModified.Write(player.ScaleModified, c);
        ScaleFactor.Write(player.ScaleFactor, c);
        ScaleFactorInverse.Write(player.ScaleFactorInverse, c);
        EyeHeightAsMeters.Write(player.EyeHeightAsMeters, c);
        EyeHeightAsPercent.Write(player.EyeHeightAsPercent, c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}