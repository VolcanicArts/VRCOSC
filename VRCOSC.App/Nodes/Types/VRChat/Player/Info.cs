// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes.Types.VRChat.Player;

[Node("Player Movement", "VRChat/Player/Info")]
public sealed class PlayerMovementNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<Vector3> Velocity = new();
    public ValueOutput<float> AngularY = new();
    public ValueOutput<float> Upright = new();

    protected override Task Process(PulseContext c)
    {
        var player = c.GetClient().Player;

        Velocity.Write(new Vector3(player.VelocityX, player.VelocityY, player.VelocityZ), c);
        AngularY.Write(player.AngularY, c);
        Upright.Write(player.Upright, c);
        return Task.CompletedTask;
    }
}

[Node("Player Gesture", "VRChat/Player/Info")]
public sealed class PlayerGestureNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<GestureType> LeftType = new();
    public ValueOutput<float> LeftWeight = new();
    public ValueOutput<GestureType> RightType = new();
    public ValueOutput<float> RightWeight = new();

    protected override Task Process(PulseContext c)
    {
        var player = c.GetClient().Player;

        LeftType.Write(player.GestureTypeLeft, c);
        RightType.Write(player.GestureTypeRight, c);
        LeftWeight.Write(player.GestureLeftWeight, c);
        RightWeight.Write(player.GestureRightWeight, c);
        return Task.CompletedTask;
    }
}

[Node("Player Voice", "VRChat/Player/Info")]
public sealed class PlayerVoiceNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<Viseme> Viseme = new();
    public ValueOutput<float> Voice = new();

    protected override Task Process(PulseContext c)
    {
        var player = c.GetClient().Player;

        Viseme.Write(player.Viseme, c);
        Voice.Write(player.Voice, c);
        return Task.CompletedTask;
    }
}

[Node("Player Identity", "VRChat/Player/Info")]
public sealed class PlayerIdentityNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<bool> IsVR = new();
    public ValueOutput<bool> IsMuted = new();
    public ValueOutput<bool> Earmuffs = new();
    public ValueOutput<bool> AFK = new();
    public ValueOutput<bool> InStation = new();
    public ValueOutput<bool> Seated = new();
    public ValueOutput<bool> Grounded = new();
    public ValueOutput<TrackingType> TrackingType = new();

    protected override Task Process(PulseContext c)
    {
        var player = c.GetClient().Player;

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
}

[Node("Player Size", "VRChat/Player/Info")]
public sealed class PlayerSizeNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<bool> ScaleModified = new();
    public ValueOutput<float> ScaleFactor = new();
    public ValueOutput<float> ScaleFactorInverse = new();
    public ValueOutput<float> EyeHeightAsMeters = new();
    public ValueOutput<float> EyeHeightAsPercent = new();

    protected override Task Process(PulseContext c)
    {
        var player = c.GetClient().Player;

        ScaleModified.Write(player.ScaleModified, c);
        ScaleFactor.Write(player.ScaleFactor, c);
        ScaleFactorInverse.Write(player.ScaleFactorInverse, c);
        EyeHeightAsMeters.Write(player.EyeHeightAsMeters, c);
        EyeHeightAsPercent.Write(player.EyeHeightAsPercent, c);
        return Task.CompletedTask;
    }
}