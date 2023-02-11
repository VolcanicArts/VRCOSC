// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.SRanipal;

namespace VRCOSC.Modules.FaceTracking.Interface.Lips;

/// <summary>
/// Shape definitions taken and modified from VRCFaceTracking
/// https://github.com/benaclejames/VRCFaceTracking/blob/master/VRCFaceTracking/Params/Lip/LipShapeMerger.cs
/// </summary>
public static class LipShapeGenerator
{
    public static readonly Dictionary<LipParam, IBlendedShape> SHAPES = new()
    {
        // Default params
        { LipParam.JawX, new PercentageShape(LipShapeV2.JawRight, LipShapeV2.JawLeft) },
        { LipParam.MouthUpper, new PercentageShape(LipShapeV2.MouthUpperRight, LipShapeV2.MouthUpperLeft) },
        { LipParam.MouthLower, new PercentageShape(LipShapeV2.MouthLowerRight, LipShapeV2.MouthLowerLeft) },
        { LipParam.MouthX, new MaxAveragedShape(new[] { LipShapeV2.MouthUpperRight, LipShapeV2.MouthLowerRight }, new[] { LipShapeV2.MouthUpperLeft, LipShapeV2.MouthLowerLeft }) },
        { LipParam.SmileSadRight, new PercentageShape(LipShapeV2.MouthSmileRight, LipShapeV2.MouthSadRight) },
        { LipParam.SmileSadLeft, new PercentageShape(LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSadLeft) },
        { LipParam.SmileSad, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthSadLeft, LipShapeV2.MouthSadRight }) },
        { LipParam.TongueY, new PercentageShape(LipShapeV2.TongueUp, LipShapeV2.TongueDown) },
        { LipParam.TongueX, new PercentageShape(LipShapeV2.TongueRight, LipShapeV2.TongueLeft) },
        { LipParam.PuffSuckRight, new PercentageShape(LipShapeV2.CheekPuffRight, LipShapeV2.CheekSuck) },
        { LipParam.PuffSuckLeft, new PercentageShape(LipShapeV2.CheekPuffLeft, LipShapeV2.CheekSuck) },
        { LipParam.PuffSuck, new MaxAveragedShape(new[] { LipShapeV2.CheekPuffLeft, LipShapeV2.CheekPuffRight }, new[] { LipShapeV2.CheekSuck }) },

        // JawOpen based params
        { LipParam.JawOpenApe, new PercentageShape(LipShapeV2.JawOpen, LipShapeV2.MouthApeShape) },
        { LipParam.JawOpenPuff, new PercentageAveragedShape(new[] { LipShapeV2.JawOpen }, new[] { LipShapeV2.CheekPuffLeft, LipShapeV2.CheekPuffRight }) },
        { LipParam.JawOpenPuffRight, new PercentageShape(LipShapeV2.JawOpen, LipShapeV2.CheekPuffRight) },
        { LipParam.JawOpenPuffLeft, new PercentageShape(LipShapeV2.JawOpen, LipShapeV2.CheekPuffLeft) },
        { LipParam.JawOpenSuck, new PercentageShape(LipShapeV2.JawOpen, LipShapeV2.CheekSuck) },
        { LipParam.JawOpenForward, new PercentageShape(LipShapeV2.JawOpen, LipShapeV2.JawForward) },
        { LipParam.JawOpenOverlay, new PercentageShape(LipShapeV2.JawOpen, LipShapeV2.MouthLowerOverlay) },

        // MouthUpperUpRight based params
        { LipParam.MouthUpperUpRightUpperInside, new PercentageShape(LipShapeV2.MouthUpperUpRight, LipShapeV2.MouthUpperInside) },
        { LipParam.MouthUpperUpRightPuffRight, new PercentageShape(LipShapeV2.MouthUpperUpRight, LipShapeV2.CheekPuffRight) },
        { LipParam.MouthUpperUpRightApe, new PercentageShape(LipShapeV2.MouthUpperUpRight, LipShapeV2.MouthApeShape) },
        { LipParam.MouthUpperUpRightPout, new PercentageShape(LipShapeV2.MouthUpperUpRight, LipShapeV2.MouthPout) },
        { LipParam.MouthUpperUpRightOverlay, new PercentageShape(LipShapeV2.MouthUpperUpRight, LipShapeV2.MouthLowerOverlay) },
        { LipParam.MouthUpperUpRightSuck, new PercentageShape(LipShapeV2.MouthUpperUpRight, LipShapeV2.CheekSuck) },

        // MouthUpperUpLeft based params
        { LipParam.MouthUpperUpLeftUpperInside, new PercentageShape(LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperInside) },
        { LipParam.MouthUpperUpLeftPuffLeft, new PercentageShape(LipShapeV2.MouthUpperUpLeft, LipShapeV2.CheekPuffLeft) },
        { LipParam.MouthUpperUpLeftApe, new PercentageShape(LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthApeShape) },
        { LipParam.MouthUpperUpLeftPout, new PercentageShape(LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthPout) },
        { LipParam.MouthUpperUpLeftOverlay, new PercentageShape(LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthLowerOverlay) },
        { LipParam.MouthUpperUpLeftSuck, new PercentageShape(LipShapeV2.MouthUpperUpLeft, LipShapeV2.CheekSuck) },

        // MouthUpperUp Left+Right base params
        { LipParam.MouthUpperUpUpperInside, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.MouthUpperInside }) },
        { LipParam.MouthUpperUpInside, new MaxAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.MouthUpperInside, LipShapeV2.MouthLowerInside }) },
        { LipParam.MouthUpperUpPuff, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.CheekPuffLeft, LipShapeV2.CheekPuffRight }) },
        { LipParam.MouthUpperUpPuffLeft, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.CheekPuffLeft }) },
        { LipParam.MouthUpperUpPuffRight, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.CheekPuffRight }) },
        { LipParam.MouthUpperUpApe, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.MouthApeShape }) },
        { LipParam.MouthUpperUpPout, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.MouthPout }) },
        { LipParam.MouthUpperUpOverlay, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.MouthLowerOverlay }) },
        { LipParam.MouthUpperUpSuck, new PercentageAveragedShape(new[] { LipShapeV2.MouthUpperUpLeft, LipShapeV2.MouthUpperUpRight }, new[] { LipShapeV2.CheekSuck }) },

        // MouthLowerDownRight based params
        { LipParam.MouthLowerDownRightLowerInside, new PercentageShape(LipShapeV2.MouthLowerDownRight, LipShapeV2.MouthLowerInside) },
        { LipParam.MouthLowerDownRightPuffRight, new PercentageShape(LipShapeV2.MouthLowerDownRight, LipShapeV2.CheekPuffRight) },
        { LipParam.MouthLowerDownRightApe, new PercentageShape(LipShapeV2.MouthLowerDownRight, LipShapeV2.MouthApeShape) },
        { LipParam.MouthLowerDownRightPout, new PercentageShape(LipShapeV2.MouthLowerDownRight, LipShapeV2.MouthPout) },
        { LipParam.MouthLowerDownRightOverlay, new PercentageShape(LipShapeV2.MouthLowerDownRight, LipShapeV2.MouthLowerOverlay) },
        { LipParam.MouthLowerDownRightSuck, new PercentageShape(LipShapeV2.MouthLowerDownRight, LipShapeV2.CheekSuck) },

        // MouthLowerDownLeft based params
        { LipParam.MouthLowerDownLeftLowerInside, new PercentageShape(LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerInside) },
        { LipParam.MouthLowerDownLeftPuffLeft, new PercentageShape(LipShapeV2.MouthLowerDownLeft, LipShapeV2.CheekPuffLeft) },
        { LipParam.MouthLowerDownLeftApe, new PercentageShape(LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthApeShape) },
        { LipParam.MouthLowerDownLeftPout, new PercentageShape(LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthPout) },
        { LipParam.MouthLowerDownLeftOverlay, new PercentageShape(LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerOverlay) },
        { LipParam.MouthLowerDownLeftSuck, new PercentageShape(LipShapeV2.MouthLowerDownLeft, LipShapeV2.CheekSuck) },

        // MouthLowerDown Left+Right base params
        { LipParam.MouthLowerDownLowerInside, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.MouthLowerInside }) },
        { LipParam.MouthLowerDownInside, new MaxAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.MouthUpperInside, LipShapeV2.MouthLowerInside }) },
        { LipParam.MouthLowerDownPuff, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.CheekPuffLeft, LipShapeV2.CheekPuffRight }) },
        { LipParam.MouthLowerDownPuffLeft, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.CheekPuffLeft }) },
        { LipParam.MouthLowerDownPuffRight, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.CheekPuffRight }) },
        { LipParam.MouthLowerDownApe, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.MouthApeShape }) },
        { LipParam.MouthLowerDownPout, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.MouthPout }) },
        { LipParam.MouthLowerDownOverlay, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.MouthLowerOverlay }) },
        { LipParam.MouthLowerDownSuck, new PercentageAveragedShape(new[] { LipShapeV2.MouthLowerDownLeft, LipShapeV2.MouthLowerDownRight }, new[] { LipShapeV2.CheekSuck }) },

        // MouthInsideOverturn based params
        { LipParam.MouthUpperInsideOverturn, new PercentageShape(LipShapeV2.MouthUpperInside, LipShapeV2.MouthUpperOverturn) },
        { LipParam.MouthLowerInsideOverturn, new PercentageShape(LipShapeV2.MouthLowerInside, LipShapeV2.MouthLowerOverturn) },

        // SmileRight based params
        { LipParam.SmileRightUpperOverturn, new PercentageShape(LipShapeV2.MouthSmileRight, LipShapeV2.MouthUpperOverturn) },
        { LipParam.SmileRightLowerOverturn, new PercentageShape(LipShapeV2.MouthSmileRight, LipShapeV2.MouthLowerOverturn) },
        { LipParam.SmileRightOverturn, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthUpperOverturn, LipShapeV2.MouthLowerOverturn }) },
        { LipParam.SmileRightApe, new PercentageShape(LipShapeV2.MouthSmileRight, LipShapeV2.MouthApeShape) },
        { LipParam.SmileRightOverlay, new PercentageShape(LipShapeV2.MouthSmileRight, LipShapeV2.MouthLowerOverlay) },
        { LipParam.SmileRightPout, new PercentageShape(LipShapeV2.MouthSmileRight, LipShapeV2.MouthPout) },

        // SmileLeft based params
        { LipParam.SmileLeftUpperOverturn, new PercentageShape(LipShapeV2.MouthSmileLeft, LipShapeV2.MouthUpperOverturn) },
        { LipParam.SmileLeftLowerOverturn, new PercentageShape(LipShapeV2.MouthSmileLeft, LipShapeV2.MouthLowerOverturn) },
        { LipParam.SmileLeftOverturn, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft }, new[] { LipShapeV2.MouthUpperOverturn, LipShapeV2.MouthLowerOverturn }) },
        { LipParam.SmileLeftApe, new PercentageShape(LipShapeV2.MouthSmileLeft, LipShapeV2.MouthApeShape) },
        { LipParam.SmileLeftOverlay, new PercentageShape(LipShapeV2.MouthSmileLeft, LipShapeV2.MouthLowerOverlay) },
        { LipParam.SmileLeftPout, new PercentageShape(LipShapeV2.MouthSmileLeft, LipShapeV2.MouthPout) },

        // Smile Left+Right based params
        { LipParam.SmileUpperOverturn, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthUpperOverturn }) },
        { LipParam.SmileLowerOverturn, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthLowerOverturn }) },
        { LipParam.SmileOverturn, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthUpperOverturn, LipShapeV2.MouthLowerOverturn }) },
        { LipParam.SmileApe, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthApeShape }) },
        { LipParam.SmileOverlay, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthLowerOverlay }) },
        { LipParam.SmilePout, new PercentageAveragedShape(new[] { LipShapeV2.MouthSmileLeft, LipShapeV2.MouthSmileRight }, new[] { LipShapeV2.MouthPout }) },

        // CheekPuffRight based params
        { LipParam.PuffRightUpperOverturn, new PercentageShape(LipShapeV2.CheekPuffRight, LipShapeV2.MouthUpperOverturn) },
        { LipParam.PuffRightLowerOverturn, new PercentageShape(LipShapeV2.CheekPuffRight, LipShapeV2.MouthLowerOverturn) },
        { LipParam.PuffRightOverturn, new MaxAveragedShape(new[] { LipShapeV2.CheekPuffRight }, new[] { LipShapeV2.MouthUpperOverturn, LipShapeV2.MouthLowerOverturn }) },

        // CheekPuffLeft based params
        { LipParam.PuffLeftUpperOverturn, new PercentageShape(LipShapeV2.CheekPuffLeft, LipShapeV2.MouthUpperOverturn) },
        { LipParam.PuffLeftLowerOverturn, new PercentageShape(LipShapeV2.CheekPuffLeft, LipShapeV2.MouthLowerOverturn) },
        { LipParam.PuffLeftOverturn, new MaxAveragedShape(new[] { LipShapeV2.CheekPuffLeft }, new[] { LipShapeV2.MouthUpperOverturn, LipShapeV2.MouthLowerOverturn }) },

        // CheekPuff Left+Right based params
        { LipParam.PuffUpperOverturn, new PercentageAveragedShape(new[] { LipShapeV2.CheekPuffRight, LipShapeV2.CheekPuffLeft }, new[] { LipShapeV2.MouthUpperOverturn }) },
        { LipParam.PuffLowerOverturn, new PercentageAveragedShape(new[] { LipShapeV2.CheekPuffRight, LipShapeV2.CheekPuffLeft }, new[] { LipShapeV2.MouthLowerOverturn }) },
        { LipParam.PuffOverturn, new MaxAveragedShape(new[] { LipShapeV2.CheekPuffRight, LipShapeV2.CheekPuffLeft }, new[] { LipShapeV2.MouthUpperOverturn, LipShapeV2.MouthLowerOverturn }) },

        // Combine both TongueSteps
        { LipParam.TongueSteps, new SteppedPercentageShape(LipShapeV2.TongueLongStep1, LipShapeV2.TongueLongStep2) },
    };
}

public enum LipParam
{
    JawX,
    MouthUpper,
    MouthLower,
    MouthX,
    SmileSadRight,
    SmileSadLeft,
    SmileSad,
    TongueY,
    TongueX,
    PuffSuckRight,
    PuffSuckLeft,
    PuffSuck,
    JawOpenApe,
    JawOpenPuff,
    JawOpenPuffRight,
    JawOpenPuffLeft,
    JawOpenSuck,
    JawOpenForward,
    JawOpenOverlay,
    MouthUpperUpRightUpperInside,
    MouthUpperUpRightPuffRight,
    MouthUpperUpRightApe,
    MouthUpperUpRightPout,
    MouthUpperUpRightOverlay,
    MouthUpperUpRightSuck,
    MouthUpperUpLeftUpperInside,
    MouthUpperUpLeftPuffLeft,
    MouthUpperUpLeftApe,
    MouthUpperUpLeftPout,
    MouthUpperUpLeftOverlay,
    MouthUpperUpLeftSuck,
    MouthUpperUpUpperInside,
    MouthUpperUpInside,
    MouthUpperUpPuff,
    MouthUpperUpPuffLeft,
    MouthUpperUpPuffRight,
    MouthUpperUpApe,
    MouthUpperUpPout,
    MouthUpperUpOverlay,
    MouthUpperUpSuck,
    MouthLowerDownRightLowerInside,
    MouthLowerDownRightPuffRight,
    MouthLowerDownRightApe,
    MouthLowerDownRightPout,
    MouthLowerDownRightOverlay,
    MouthLowerDownRightSuck,
    MouthLowerDownLeftLowerInside,
    MouthLowerDownLeftPuffLeft,
    MouthLowerDownLeftApe,
    MouthLowerDownLeftPout,
    MouthLowerDownLeftOverlay,
    MouthLowerDownLeftSuck,
    MouthLowerDownLowerInside,
    MouthLowerDownInside,
    MouthLowerDownPuff,
    MouthLowerDownPuffLeft,
    MouthLowerDownPuffRight,
    MouthLowerDownApe,
    MouthLowerDownPout,
    MouthLowerDownOverlay,
    MouthLowerDownSuck,
    MouthUpperInsideOverturn,
    MouthLowerInsideOverturn,
    SmileRightUpperOverturn,
    SmileRightLowerOverturn,
    SmileRightOverturn,
    SmileRightApe,
    SmileRightOverlay,
    SmileRightPout,
    SmileLeftUpperOverturn,
    SmileLeftLowerOverturn,
    SmileLeftOverturn,
    SmileLeftApe,
    SmileLeftOverlay,
    SmileLeftPout,
    SmileUpperOverturn,
    SmileLowerOverturn,
    SmileOverturn,
    SmileApe,
    SmileOverlay,
    SmilePout,
    PuffRightUpperOverturn,
    PuffRightLowerOverturn,
    PuffRightOverturn,
    PuffLeftUpperOverturn,
    PuffLeftLowerOverturn,
    PuffLeftOverturn,
    PuffUpperOverturn,
    PuffLowerOverturn,
    PuffOverturn,
    TongueSteps
}
