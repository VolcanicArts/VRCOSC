// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.Weather;

[ModuleTitle("Weather")]
[ModuleDescription("Retrieves weather information for a specific area")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
public class WeatherModule : ChatBoxModule
{
    private WeatherProvider? weatherProvider;

    protected override void CreateAttributes()
    {
        CreateSetting(WeatherSetting.Postcode, "Location", "The post/zip code or city name to retrieve weather data for", string.Empty);

        CreateParameter<int>(WeatherParameter.Code, ParameterMode.Write, "VRCOSC/Weather/Code", "Weather Code", "The current weather's code");

        CreateVariable(WeatherVariable.TempC, @"Temp C", @"tempc");
        CreateVariable(WeatherVariable.TempF, @"Temp F", @"tempf");
        CreateVariable(WeatherVariable.Humidity, @"Humidity", @"humidity");
        CreateVariable(WeatherVariable.Condition, @"Condition", @"condition");

        CreateState(WeatherState.Default, @"Default", $@"Local Weather/v{GetVariableFormat(WeatherVariable.Condition)}/v{GetVariableFormat(WeatherVariable.TempC)}C - {GetVariableFormat(WeatherVariable.TempF)}F");
    }

    protected override void OnModuleStart()
    {
        if (string.IsNullOrEmpty(GetSetting<string>(WeatherSetting.Postcode))) Log("Please provide a post/zip code or city name");

        weatherProvider ??= new WeatherProvider(OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.Weather));
        ChangeStateTo(WeatherState.Default);
    }

    protected override void OnAvatarChange()
    {
        updateParameters();
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 5000)]
    private async void updateParameters()
    {
        if (weatherProvider is null)
        {
            Log("Warning. Module not started correctly");
            return;
        }

        if (string.IsNullOrEmpty(GetSetting<string>(WeatherSetting.Postcode)))
        {
            Log("Please fill in a postcode/city name");
            return;
        }

        var weather = await weatherProvider.RetrieveFor(GetSetting<string>(WeatherSetting.Postcode));

        if (weather is null)
        {
            Log("Cannot retrieve weather for provided location");
            Log("If you've entered a post/zip code, try your closest city's name");
            return;
        }

        SendParameter(WeatherParameter.Code, getConvertedWeatherCode(weather));

        SetVariableValue(WeatherVariable.TempC, weather.TempC.ToString("0.0"));
        SetVariableValue(WeatherVariable.TempF, weather.TempF.ToString("0.0"));
        SetVariableValue(WeatherVariable.Humidity, weather.Humidity.ToString());
        SetVariableValue(WeatherVariable.Condition, weather.ConditionString);
    }

    private static int getConvertedWeatherCode(Weather weather) => weather.Condition.Code switch
    {
        1000 => 1,
        1003 => 2,
        1006 => 3,
        1009 => 4,
        1030 => 5,
        1063 => 6,
        1066 => 7,
        1069 => 8,
        1072 => 9,
        1087 => 10,
        1114 => 11,
        1117 => 12,
        1135 => 13,
        1147 => 14,
        1150 => 15,
        1153 => 16,
        1168 => 17,
        1171 => 18,
        1180 => 19,
        1183 => 20,
        1186 => 21,
        1189 => 22,
        1192 => 23,
        1195 => 24,
        1198 => 25,
        1201 => 26,
        1204 => 27,
        1207 => 28,
        1210 => 29,
        1213 => 30,
        1216 => 31,
        1219 => 32,
        1222 => 33,
        1225 => 34,
        1237 => 35,
        1240 => 36,
        1243 => 37,
        1246 => 38,
        1249 => 39,
        1252 => 40,
        1255 => 41,
        1258 => 42,
        1261 => 43,
        1264 => 44,
        1273 => 45,
        1276 => 46,
        1279 => 47,
        1282 => 48,
        _ => 0
    };

    private enum WeatherSetting
    {
        Postcode
    }

    private enum WeatherParameter
    {
        Code
    }

    private enum WeatherState
    {
        Default
    }

    private enum WeatherVariable
    {
        TempC,
        TempF,
        Humidity,
        Condition
    }
}
