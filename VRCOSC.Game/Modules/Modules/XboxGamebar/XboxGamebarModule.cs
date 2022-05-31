using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.XboxGamebar;

public class XboxGamebarModule : IntegrationModule
{
    public override string Title => "Xbox Gamebar";
    public override string Description => "Integrate with the Xbox Gamebar recording and HDR functions.";
    public override string Author => "Buckminsterfullerene";
    public override Colour4 Colour => Colour4.Coral.Darken(0.5f);
    public override ModuleType Type => ModuleType.Integrations;

    private XboxGamebarModuleValues values;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        XboxInputParameters.XboxToggleRecording,
        XboxInputParameters.XboxRecordPrevious,
        XboxInputParameters.XboxToggleHDR
    };

    protected override IReadOnlyDictionary<Enum, WindowsVKey[]> KeyCombinations => new Dictionary<Enum, WindowsVKey[]>
    {
        { XboxInputParameters.XboxToggleRecording, new[] { WindowsVKey.VK_LWIN, WindowsVKey.VK_MENU, WindowsVKey.VK_R } },
        { XboxInputParameters.XboxRecordPrevious, new[] { WindowsVKey.VK_LWIN, WindowsVKey.VK_MENU, WindowsVKey.VK_G } },
        { XboxInputParameters.XboxToggleHDR, new[] { WindowsVKey.VK_LWIN, WindowsVKey.VK_MENU, WindowsVKey.VK_B } }
    };

    public override void CreateAttributes()
    {
        CreateParameter(XboxInputParameters.XboxToggleRecording, "Is Currently Recording", "If you are currently recording or not.", "/avatar/parameters/XboxGamebarIsRecording");
        CreateParameter(XboxInputParameters.XboxToggleHDR, "Is In HDR", "If you are currently in HDR mode or not", "/avatar/parameters/XboxGamebarIsHDR");
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        ExecuteShortcut(key);

        switch (key)
        {
            case XboxInputParameters.XboxToggleRecording:
                if (values.IsRecording)
                {
                    values.IsRecording = false;
                    Terminal.Log("Stopped recording");
                }
                else
                {
                    values.IsRecording = true;
                    Terminal.Log("Started recording");
                }

                SendParameter(XboxInputParameters.XboxToggleRecording, values.IsRecording);
                break;

            case XboxInputParameters.XboxToggleHDR:
                if (values.IsHDR)
                {
                    values.IsHDR = false;
                    Terminal.Log("Stopped HDR");
                }
                else
                {
                    values.IsHDR = true;
                    Terminal.Log("Started HDR");
                }

                SendParameter(XboxInputParameters.XboxToggleHDR, values.IsHDR);
                break;
        }
    }
}

public struct XboxGamebarModuleValues
{
    // ReSharper disable once InconsistentNaming
    public bool IsHDR;
    public bool IsRecording;
}

public enum XboxInputParameters
{
    XboxToggleRecording,
    XboxRecordPrevious,
    XboxToggleHDR
}
