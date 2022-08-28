using System.Linq;

namespace VRCOSC.Game.Modules.Modules.Heartrate;

public abstract class HeartrateModule : Module
{
    protected override void CreateAttributes()
    {
        CreateOutputParameter(HeartrateOutputParameter.HeartrateEnabled, "Heartrate Enabled", "Whether this module is attempting to emit values", "/avatar/parameters/HeartrateEnabled");
        CreateOutputParameter(HeartrateOutputParameter.HeartrateNormalised, "Heartrate Normalised", "The heartrate value normalised to 60bpm", "/avatar/parameters/HeartrateNormalised");
        CreateOutputParameter(HeartrateOutputParameter.HeartrateUnits, "Heartrate Units", "The units digit 0-9 mapped to a float", "/avatar/parameters/HeartrateUnits");
        CreateOutputParameter(HeartrateOutputParameter.HeartrateTens, "Heartrate Tens", "The tens digit 0-9 mapped to a float", "/avatar/parameters/HeartrateTens");
        CreateOutputParameter(HeartrateOutputParameter.HeartrateHundreds, "Heartrate Hundreds", "The hundreds digit 0-9 mapped to a float", "/avatar/parameters/HeartrateHundreds");
    }

    protected void HandleHeartRateUpdate(int heartrate)
    {
        var normalisedHeartRate = heartrate / 60.0f;
        var individualValues = toDigitArray(heartrate, 3);

        SendParameter(HeartrateOutputParameter.HeartrateEnabled, true);
        SendParameter(HeartrateOutputParameter.HeartrateNormalised, normalisedHeartRate);
        SendParameter(HeartrateOutputParameter.HeartrateUnits, individualValues[2] / 10f);
        SendParameter(HeartrateOutputParameter.HeartrateTens, individualValues[1] / 10f);
        SendParameter(HeartrateOutputParameter.HeartrateHundreds, individualValues[0] / 10f);
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected enum HeartrateOutputParameter
    {
        HeartrateEnabled,
        HeartrateNormalised,
        HeartrateUnits,
        HeartrateTens,
        HeartrateHundreds
    }
}
