// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using VRCOSC.Game.Modules;
using VRCOSC.Game.SRanipal;
using VRCOSC.Modules.FaceTracking.Interface;
using VRCOSC.Modules.FaceTracking.Interface.Eyes;
using VRCOSC.Modules.FaceTracking.Interface.Lips;

namespace VRCOSC.Modules.FaceTracking;

public partial class SRanipalModule : Module
{
    public override string Title => "SRanipal";
    public override string Description => "Hooks into SRanipal and sends face tracking data to VRChat. Interchangeable with VRCFaceTracking";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.Integrations;

    protected override TimeSpan DeltaUpdate
    {
        get
        {
            return GetSetting<TrackingQuality>(SRanipalSetting.TrackingQuality) switch
            {
                TrackingQuality.Low => TimeSpan.FromSeconds(1f / 20f),
                TrackingQuality.Medium => TimeSpan.FromSeconds(1f / 40f),
                TrackingQuality.High => TimeSpan.FromSeconds(1f / 60f),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    protected override bool ShouldUpdateImmediately => false;

    private readonly SRanipalInterface sRanipalInterface = new();
    private readonly Dictionary<Enum, SRanipalParameterData> parameterData = new();
    private readonly List<LipParam> lipParams = Enum.GetValues<LipParam>().ToList();
    private readonly List<EyeParam> eyeParams = Enum.GetValues<EyeParam>().ToList();

    public SRanipalModule()
    {
        sRanipalInterface.APIInterface.EyeStatus.BindValueChanged(e => handleStatus("Eye", e.NewValue));
        sRanipalInterface.APIInterface.LipStatus.BindValueChanged(e => handleStatus("Lip", e.NewValue));
    }

    private void handleStatus(string name, Error status)
    {
        switch (status)
        {
            case Error.WORK:
                Log($"{name} tracking has been initialised and is available");
                break;

            case Error.NOT_SUPPORTED:
                Log($"{name} tracking is not supported in this session");
                break;
        }
    }

    protected override void CreateAttributes()
    {
        CreateSetting(SRanipalSetting.EyeEnable, "Enable Eye", "Whether to enable eye tracking", true);
        CreateSetting(SRanipalSetting.LipEnable, "Enable Lip", "Whether to enable lip tracking", true);
        CreateSetting(SRanipalSetting.TrackingQuality, "Tracking Quality", "The tracking quality level of eye and lip parameters", TrackingQuality.High);

        lipParams.ForEach(shapeKey => createParameter(shapeKey));
        eyeParams.ForEach(key => createParameter(key));
    }

    private void createParameter(Enum shapeKey)
    {
        CreateParameter<float>(shapeKey, ParameterMode.Write, shapeKey.ToString(), shapeKey.ToString(), $"{shapeKey} + 1/2/4/8/Negative");
    }

    protected override void OnModuleStart()
    {
        if (!isOpenVROpen())
        {
            Log("Cannot start module without SteamVR launched");
            return;
        }

        sRanipalInterface.Initialise(GetSetting<bool>(SRanipalSetting.EyeEnable), GetSetting<bool>(SRanipalSetting.LipEnable));
        parameterData.Clear();

        BlendedShape.ShapeTolerance = GetSetting<TrackingQuality>(SRanipalSetting.TrackingQuality) switch
        {
            TrackingQuality.Low => 1f / 64f,
            TrackingQuality.Medium => 1f / 128f,
            TrackingQuality.High => 1f / 256f,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private bool isOpenVROpen() => Process.GetProcessesByName(@"vrmonitor").Any();

    protected override void OnAvatarChange(string avatarId)
    {
        parameterData.Clear();
        if (AvatarConfig is null) return;

        Log($"Avatar change detected. Parsing avatar {AvatarConfig.Name} with {AvatarConfig.Parameters.Count} parameters");
        lipParams.ForEach(key => auditParameter(key));
        eyeParams.ForEach(key => auditParameter(key));

        var finalCount = parameterData.Select(pair => pair.Value.TotalCount).Sum();
        Log($"Detected {finalCount} usable parameters");
    }

    private void auditParameter(Enum lookup)
    {
        var checkName = lookup.ToString();

        SRanipalParameterData? paramData = null;

        AvatarConfig!.Parameters.ForEach(param =>
        {
            if (param.Name.Length < checkName.Length || param.Name[..checkName.Length] != checkName) return;

            var paramSuffix = param.Name[checkName.Length..];

            if (param.Name == checkName)
            {
                paramData ??= new SRanipalParameterData();
                paramData.FloatPresent = true;
            }

            if (int.TryParse(paramSuffix, out _))
            {
                paramData ??= new SRanipalParameterData();
                paramData.BoolCount++;
            }

            if (paramSuffix == "Negative")
            {
                paramData ??= new SRanipalParameterData();
                paramData.NegativePresent = true;
            }
        });

        if (paramData is null) return;

        parameterData.Add(lookup, paramData);
    }

    protected override void OnModuleUpdate()
    {
        sRanipalInterface.Update();

        if (GetSetting<bool>(SRanipalSetting.EyeEnable) && sRanipalInterface.APIInterface.EyeStatus.Value == Error.WORK) sendEyes();
        if (GetSetting<bool>(SRanipalSetting.LipEnable) && sRanipalInterface.APIInterface.LipStatus.Value == Error.WORK) sendLips();
    }

    private void sendEyes()
    {
        foreach (var key in eyeParams)
        {
            if (EyeParameterGenerator.FLOAT_VALUES.TryGetValue(key, out var floatFunc))
            {
                var value = floatFunc(sRanipalInterface.EyeData);
                sendParameter(key, value); // may be binary encoded
            }

            if (EyeParameterGenerator.BOOL_VALUES.TryGetValue(key, out var boolFunc))
            {
                var value = boolFunc(sRanipalInterface.EyeData);
                SendParameter(key, value); // directly send bool values
            }
        }
    }

    private void sendLips()
    {
        foreach (var key in lipParams)
        {
            var shape = LipShapeGenerator.SHAPES[key];
            var value = shape.GetShape(sRanipalInterface.LipData.Shapes);
            if (value.HasChanged) sendParameter(key, value.Value);
        }
    }

    private void sendParameter(Enum lookup, float value)
    {
        if (!parameterData.TryGetValue(lookup, out var paramData)) return;

        if (paramData.FloatPresent)
            SendParameter(lookup, value);

        if (!paramData.ShouldEncode) return;

        if (paramData.NegativePresent)
        {
            SendParameter(lookup, value < 0, "Negative");
        }
        else
        {
            value = Math.Max(value, 0);
        }

        encodeAndSend(lookup, value, paramData.BoolCount);
    }

    private readonly string[] addonCache = { "1", "2", "4", "8" };

    private void encodeAndSend(Enum lookup, float value, int binaryDepth)
    {
        var binaryRep = encodeFloat(value, binaryDepth);

        for (var i = 0; i < binaryDepth; i++)
        {
            SendParameter(lookup, binaryRep[i], addonCache[i]);
        }
    }

    private static bool[] encodeFloat(float value, int depth)
    {
        var array = new bool[depth];
        var binaryRepresentation = (int)(Math.Abs(value) * (MathF.Pow(2, depth) - 1));

        for (var i = 0; i < depth; i++)
        {
            array[i] = (binaryRepresentation >> i & 1) == 1;
        }

        return array;
    }

    protected override void OnModuleStop()
    {
        sRanipalInterface.Release();
    }

    private enum SRanipalSetting
    {
        EyeEnable,
        LipEnable,
        TrackingQuality
    }

    private enum TrackingQuality
    {
        Low,
        Medium,
        High
    }
}
