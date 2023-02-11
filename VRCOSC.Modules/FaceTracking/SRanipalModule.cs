// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules;
using VRCOSC.Game.SRanipal;
using VRCOSC.Modules.FaceTracking.Interface;
using VRCOSC.Modules.FaceTracking.Interface.Lips;

namespace VRCOSC.Modules.FaceTracking;

public partial class SRanipalModule : Module
{
    public override string Title => "SRanipal";
    public override string Description => "Hooks into SRanipal and sends face tracking data to VRChat. Interchangeable with VRCFaceTracking";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.Integrations;
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(1f / 20f);
    protected override bool ShouldUpdateImmediately => false;

    private readonly SRanipalInterface sRanipalInterface = new();

    private readonly Dictionary<Enum, SRanipalParameterData> parameterData = new();

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

        Enum.GetValues<LipShapeV2>().ForEach(shapeKey => createParameter(shapeKey));
        Enum.GetValues<LipParam>().ForEach(shapeKey => createParameter(shapeKey));
    }

    private void createParameter(Enum shapeKey)
    {
        CreateParameter<float>(shapeKey, ParameterMode.Write, shapeKey.ToString(), shapeKey.ToString(), $"{shapeKey} + 1/2/4/8/Negative");
    }

    protected override void OnModuleStart()
    {
        sRanipalInterface.Initialise(GetSetting<bool>(SRanipalSetting.EyeEnable), GetSetting<bool>(SRanipalSetting.LipEnable));
        parameterData.Clear();
    }

    protected override void OnAvatarChange(string avatarId)
    {
        parameterData.Clear();
        if (AvatarConfig is null) return;

        Log($"Avatar change detected. Parsing avatar {AvatarConfig.Name} with {AvatarConfig.Parameters.Count} parameters");
        auditParameters();

        var finalCount = parameterData.Select(pair => pair.Value.TotalCount).Sum();
        Log($"Detected {finalCount} usable parameters");
    }

    private void auditParameters()
    {
        Enum.GetValues<LipShapeV2>().Cast<Enum>().Concat(Enum.GetValues<LipParam>().Cast<Enum>()).ForEach(auditParameter);
    }

    private void auditParameter(Enum lookup)
    {
        var checkName = lookup.ToString();

        SRanipalParameterData? paramData = null;

        AvatarConfig!.Parameters.ForEach(param =>
        {
            if (param.Name.Length < checkName.Length || param.Name[..checkName.Length] != checkName) return;

            paramData ??= new SRanipalParameterData();
            var paramSuffix = param.Name[checkName.Length..];

            if (param.Name == checkName) paramData.FloatPresent = true;
            if (int.TryParse(paramSuffix, out _)) paramData.BoolCount++;
            if (paramSuffix == "Negative") paramData.NegativePresent = true;
        });

        if (paramData is null) return;

        parameterData.Add(lookup, paramData);
    }

    protected override void OnModuleUpdate()
    {
        sRanipalInterface.Update();

        if (GetSetting<bool>(SRanipalSetting.EyeEnable)) sendEyes();
        if (GetSetting<bool>(SRanipalSetting.LipEnable)) sendLips();
    }

    private void sendEyes()
    {
    }

    private void sendLips()
    {
        foreach (var srParam in Enum.GetValues<LipShapeV2>())
        {
            sendParameter(srParam, sRanipalInterface.LipData.Shapes[srParam]);
        }

        var rawValues = sRanipalInterface.LipData.Shapes.Values.ToArray();

        foreach (var shape in Enum.GetValues<LipParam>())
        {
            sendParameter(shape, LipShapeGenerator.SHAPES[shape].GetBlendedShape(rawValues));
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
        LipEnable
    }
}
