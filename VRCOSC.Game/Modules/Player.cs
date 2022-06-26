// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public class Player
{
    public Viseme? Viseme;
    public float? Voice;
    public Gesture? GestureLeft;
    public Gesture? GestureRight;
    public float? GestureLeftWeight;
    public float? GestureRightWeight;
    public float? AngularY;
    public float? VelocityX;
    public float? VelocityY;
    public float? VelocityZ;
    public float? Upright;
    public bool? Grounded;
    public bool? Seated;
    public bool? AFK;
    public TrackingType? TrackingType;
    public bool? IsVR;
    public bool? IsMuted;
    public bool? InStation;

    public Player()
    {
        Viseme = null;
        Voice = null;
        GestureLeft = null;
        GestureRight = null;
        GestureLeftWeight = null;
        GestureRightWeight = null;
        AngularY = null;
        VelocityX = null;
        VelocityY = null;
        VelocityZ = null;
        Upright = null;
        Grounded = null;
        Seated = null;
        AFK = null;
        TrackingType = null;
        IsVR = null;
        IsMuted = null;
        InStation = null;
    }
}

public enum Viseme
{
    SIL,
    PP,
    FF,
    TH,
    DD,
    KK,
    CH,
    SS,
    NN,
    RR,
    AA,
    E,
    I,
    O,
    U
}

public enum Gesture
{
    Neutral,
    Fist,
    HandOpen,
    FingerPoint,
    Victory,
    RockNRoll,
    HandGun,
    ThumbsUp
}

public enum TrackingType
{
    Uninitialised,
    Generic,
    HandsOnly,
    HeadAndHands,
    HeadHandsAndHip,
    FullBody
}

public enum VRChatInputParameter
{
    Viseme,
    Voice,
    GestureLeft,
    GestureRight,
    GestureLeftWeight,
    GestureRightWeight,
    AngularY,
    VelocityX,
    VelocityY,
    VelocityZ,
    Upright,
    Grounded,
    Seated,
    AFK,
    TrackingType,
    VRMode,
    MuteSelf,
    InStation
}
