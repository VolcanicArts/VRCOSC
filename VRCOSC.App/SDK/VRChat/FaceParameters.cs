// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;

namespace VRCOSC.App.SDK.VRChat;

public enum FaceParameter
{
    EyeLookOutRight,
    EyeLookInRight,
    EyeLookUpRight,
    EyeLookDownRight,
    EyeLookOutLeft,
    EyeLookInLeft,
    EyeLookUpLeft,
    EyeLookDownLeft,
    EyeClosedRight,
    EyeClosedLeft,
    EyeDilationRight,
    EyeDilationLeft,
    EyeConstrictRight,
    EyeConstrictLeft,
    EyeSquintRight,
    EyeSquintLeft,
    EyeWideRight,
    EyeWideLeft,
    BrowPinchRight,
    BrowPinchLeft,
    BrowLowererRight,
    BrowLowererLeft,
    BrowInnerUpRight,
    BrowInnerUpLeft,
    BrowOuterUpRight,
    BrowOuterUpLeft,
    NasalDilationRight,
    NasalDilationLeft,
    NasalConstrictRight,
    NasalConstrictLeft,
    CheekSquintRight,
    CheekSquintLeft,
    CheekPuffRight,
    CheekPuffLeft,
    CheekSuckRight,
    CheekSuckLeft,
    JawOpen,
    JawRight,
    JawLeft,
    JawForward,
    JawBackward,
    JawClench,
    JawMandibleRaise,
    MouthClosed,
    LipSuckUpperRight,
    LipSuckUpperLeft,
    LipSuckLowerRight,
    LipSuckLowerLeft,
    LipSuckCornerRight,
    LipSuckCornerLeft,
    LipFunnelUpperRight,
    LipFunnelUpperLeft,
    LipFunnelLowerRight,
    LipFunnelLowerLeft,
    LipPuckerUpperRight,
    LipPuckerUpperLeft,
    LipPuckerLowerRight,
    LipPuckerLowerLeft,
    MouthUpperUpRight,
    MouthUpperUpLeft,
    MouthUpperDeepenRight,
    MouthUpperDeepenLeft,
    NoseSneerRight,
    NoseSneerLeft,
    MouthLowerDownRight,
    MouthLowerDownLeft,
    MouthUpperRight,
    MouthUpperLeft,
    MouthLowerRight,
    MouthLowerLeft,
    MouthCornerPullRight,
    MouthCornerPullLeft,
    MouthCornerSlantRight,
    MouthCornerSlantLeft,
    MouthFrownRight,
    MouthFrownLeft,
    MouthStretchRight,
    MouthStretchLeft,
    MouthDimpleRight,
    MouthDimpleLeft,
    MouthRaiserUpper,
    MouthRaiserLower,
    MouthPressRight,
    MouthPressLeft,
    MouthTightenerRight,
    MouthTightenerLeft,
    TongueOut,
    TongueUp,
    TongueDown,
    TongueRight,
    TongueLeft,
    TongueRoll,
    TongueBendDown,
    TongueCurlUp,
    TongueSquish,
    TongueFlat,
    TongueTwistRight,
    TongueTwistLeft,
    SoftPalateClose,
    ThroatSwallow,
    NeckFlexRight,
    NeckFlexLeft
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FaceParameters
{
    public byte EyeLookOutRight;
    public byte EyeLookInRight;
    public byte EyeLookUpRight;
    public byte EyeLookDownRight;
    public byte EyeLookOutLeft;
    public byte EyeLookInLeft;
    public byte EyeLookUpLeft;
    public byte EyeLookDownLeft;
    public byte EyeClosedRight;
    public byte EyeClosedLeft;
    public byte EyeDilationRight;
    public byte EyeDilationLeft;
    public byte EyeConstrictRight;
    public byte EyeConstrictLeft;
    public byte EyeSquintRight;
    public byte EyeSquintLeft;
    public byte EyeWideRight;
    public byte EyeWideLeft;
    public byte BrowPinchRight;
    public byte BrowPinchLeft;
    public byte BrowLowererRight;
    public byte BrowLowererLeft;
    public byte BrowInnerUpRight;
    public byte BrowInnerUpLeft;
    public byte BrowOuterUpRight;
    public byte BrowOuterUpLeft;
    public byte NasalDilationRight;
    public byte NasalDilationLeft;
    public byte NasalConstrictRight;
    public byte NasalConstrictLeft;
    public byte CheekSquintRight;
    public byte CheekSquintLeft;
    public byte CheekPuffRight;
    public byte CheekPuffLeft;
    public byte CheekSuckRight;
    public byte CheekSuckLeft;
    public byte JawOpen;
    public byte JawRight;
    public byte JawLeft;
    public byte JawForward;
    public byte JawBackward;
    public byte JawClench;
    public byte JawMandibleRaise;
    public byte MouthClosed;
    public byte LipSuckUpperRight;
    public byte LipSuckUpperLeft;
    public byte LipSuckLowerRight;
    public byte LipSuckLowerLeft;
    public byte LipSuckCornerRight;
    public byte LipSuckCornerLeft;
    public byte LipFunnelUpperRight;
    public byte LipFunnelUpperLeft;
    public byte LipFunnelLowerRight;
    public byte LipFunnelLowerLeft;
    public byte LipPuckerUpperRight;
    public byte LipPuckerUpperLeft;
    public byte LipPuckerLowerRight;
    public byte LipPuckerLowerLeft;
    public byte MouthUpperUpRight;
    public byte MouthUpperUpLeft;
    public byte MouthUpperDeepenRight;
    public byte MouthUpperDeepenLeft;
    public byte NoseSneerRight;
    public byte NoseSneerLeft;
    public byte MouthLowerDownRight;
    public byte MouthLowerDownLeft;
    public byte MouthUpperRight;
    public byte MouthUpperLeft;
    public byte MouthLowerRight;
    public byte MouthLowerLeft;
    public byte MouthCornerPullRight;
    public byte MouthCornerPullLeft;
    public byte MouthCornerSlantRight;
    public byte MouthCornerSlantLeft;
    public byte MouthFrownRight;
    public byte MouthFrownLeft;
    public byte MouthStretchRight;
    public byte MouthStretchLeft;
    public byte MouthDimpleRight;
    public byte MouthDimpleLeft;
    public byte MouthRaiserUpper;
    public byte MouthRaiserLower;
    public byte MouthPressRight;
    public byte MouthPressLeft;
    public byte MouthTightenerRight;
    public byte MouthTightenerLeft;
    public byte TongueOut;
    public byte TongueUp;
    public byte TongueDown;
    public byte TongueRight;
    public byte TongueLeft;
    public byte TongueRoll;
    public byte TongueBendDown;
    public byte TongueCurlUp;
    public byte TongueSquish;
    public byte TongueFlat;
    public byte TongueTwistRight;
    public byte TongueTwistLeft;
    public byte SoftPalateClose;
    public byte ThroatSwallow;
    public byte NeckFlexRight;
    public byte NeckFlexLeft;
}