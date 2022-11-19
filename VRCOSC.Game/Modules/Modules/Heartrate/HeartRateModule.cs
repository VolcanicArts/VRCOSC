// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Modules.Heartrate;

public abstract class HeartRateModule : Module
{
    private const int heartrateTimeout = 10;

    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Heartrate";
    public override ModuleType ModuleType => ModuleType.Health;
    protected override int DeltaUpdate => 2000;
    protected override int ChatBoxPriority => 2;

    private HeartRateProvider? heartRateProvider;
    private int lastHeartrate;
    private DateTimeOffset lastHeartrateTime;
    private bool alreadyCleared;
    private int connectionCount;

    protected bool IsReceiving => lastHeartrateTime + TimeSpan.FromSeconds(heartrateTimeout) >= DateTimeOffset.Now;

    protected abstract HeartRateProvider CreateHeartRateProvider();

    protected override void CreateAttributes()
    {
        CreateSetting(HeartrateSetting.UseChatBox, "Use ChatBox", "Should your heartrate be displayed in the ChatBox?", false);
        CreateSetting(HeartrateSetting.ChatBoxFormat, "ChatBox Format", "The format of the ChatBox display.\nAvailable values: %hr%", "Heartrate                        %hr% bpm");

        CreateParameter<bool>(HeartrateParameter.Enabled, ParameterMode.Write, "VRCOSC/Heartrate/Enabled", "Whether this module is attempting to emit values");
        CreateParameter<float>(HeartrateParameter.Normalised, ParameterMode.Write, "VRCOSC/Heartrate/Normalised", "The heartrate value normalised to 60bpm");
        CreateParameter<float>(HeartrateParameter.Units, ParameterMode.Write, "VRCOSC/Heartrate/Units", "The units digit 0-9 mapped to a float");
        CreateParameter<float>(HeartrateParameter.Tens, ParameterMode.Write, "VRCOSC/Heartrate/Tens", "The tens digit 0-9 mapped to a float");
        CreateParameter<float>(HeartrateParameter.Hundreds, ParameterMode.Write, "VRCOSC/Heartrate/Hundreds", "The hundreds digit 0-9 mapped to a float");
    }

    protected override void OnStart()
    {
        attemptConnection();

        lastHeartrateTime = DateTimeOffset.Now - TimeSpan.FromSeconds(heartrateTimeout);
        alreadyCleared = false;
    }

    private void attemptConnection()
    {
        if (connectionCount >= 3)
        {
            Log("Connection cannot be established");
            return;
        }

        connectionCount++;
        heartRateProvider = CreateHeartRateProvider();
        heartRateProvider.OnHeartRateUpdate += HandleHeartRateUpdate;
        heartRateProvider.OnConnected += () => connectionCount = 0;
        heartRateProvider.OnDisconnected += async () =>
        {
            if (State.Value is ModuleState.Stopping or ModuleState.Stopped) return;

            SendParameter(HeartrateParameter.Enabled, false);
            await Task.Delay(2000);
            attemptConnection();
        };
        heartRateProvider.Initialise();
        heartRateProvider.Connect();
    }

    protected override async void OnStop()
    {
        if (heartRateProvider is null) return;

        if (connectionCount < 3) await heartRateProvider.Disconnect();
        SendParameter(HeartrateParameter.Enabled, false);
    }

    protected override void OnUpdate()
    {
        if (!IsReceiving)
        {
            SendParameter(HeartrateParameter.Enabled, false);
        }

        if (GetSetting<bool>(HeartrateSetting.UseChatBox))
        {
            if (!IsReceiving)
            {
                if (!alreadyCleared) ClearChatBox();
                alreadyCleared = true;
            }
            else
            {
                alreadyCleared = false;

                var text = GetSetting<string>(HeartrateSetting.ChatBoxFormat).Replace("%hr%", lastHeartrate.ToString());
                SetChatBoxText(text);
            }
        }
    }

    protected virtual void HandleHeartRateUpdate(int heartrate)
    {
        lastHeartrate = heartrate;
        lastHeartrateTime = DateTimeOffset.Now;

        var normalisedHeartRate = heartrate / 60.0f;
        var individualValues = toDigitArray(heartrate, 3);

        SendParameter(HeartrateParameter.Enabled, true);
        SendParameter(HeartrateParameter.Normalised, normalisedHeartRate);
        SendParameter(HeartrateParameter.Units, individualValues[2] / 10f);
        SendParameter(HeartrateParameter.Tens, individualValues[1] / 10f);
        SendParameter(HeartrateParameter.Hundreds, individualValues[0] / 10f);
    }

    private static int[] toDigitArray(int num, int totalWidth)
    {
        return num.ToString().PadLeft(totalWidth, '0').Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected enum HeartrateSetting
    {
        UseChatBox,
        ChatBoxFormat
    }

    protected enum HeartrateParameter
    {
        Enabled,
        Normalised,
        Units,
        Tens,
        Hundreds
    }
}
