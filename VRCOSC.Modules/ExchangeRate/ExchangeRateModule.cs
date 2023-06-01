// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules.ChatBox;

namespace VRCOSC.Modules.ExchangeRate;

public class ExchangeRateModule : ChatBoxModule
{
    public override string Title => "Exchange Rate";
    public override string Description => "Calculates the exchange rate from a base currency into others";
    public override string Author => "VolcanicArts";
    public override ModuleType Type => ModuleType.General;
    protected override TimeSpan DeltaUpdate => TimeSpan.FromSeconds(5);

    public override IEnumerable<string> Info => new[] { @"Exchange rate can be accessed using {exchangerate.rate_CURRENCYCODE}" };

    private ExchangeRateProvider? provider;

    protected override void CreateAttributes()
    {
        CreateSetting(ExchangeRateSetting.BaseCurrency, "Base Currency", "The base currency to convert from. All rates are displayed as a multiplier from this currency", "USD", "Supported Currency Codes", () => OpenUrlExternally("https://www.exchangerate-api.com/docs/supported-currencies"));

        CreateVariable(ExchangeRateVariable.BaseCurrency, "Base Currency", "base");
        CreateVariable(ExchangeRateVariable.Rate, "Rate", "rate");

        CreateState(ExchangeRateState.Default, "Default", $"1 {GetVariableFormat(ExchangeRateVariable.BaseCurrency)} = {GetVariableFormat(ExchangeRateVariable.Rate, "GBP")} GBP");
    }

    protected override void OnModuleStart()
    {
        provider ??= new ExchangeRateProvider(OfficialModuleSecrets.GetSecret(OfficialModuleSecretsKeys.ExchangeRate));

        ChangeStateTo(ExchangeRateState.Default);
    }

    protected override async void OnModuleUpdate()
    {
        SetVariableValue(ExchangeRateVariable.BaseCurrency, GetSetting<string>(ExchangeRateSetting.BaseCurrency));

        var exchangeRate = await provider!.GetExchangeRate(GetSetting<string>(ExchangeRateSetting.BaseCurrency));

        var currencyFormatter = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        currencyFormatter.CurrencySymbol = string.Empty;

        exchangeRate?.ConversionRates.ForEach(pair =>
        {
            var (currencyCode, rate) = pair;
            SetVariableValue(ExchangeRateVariable.Rate, string.Format(currencyFormatter, "{0:C}", rate), currencyCode);
        });
    }

    private enum ExchangeRateSetting
    {
        BaseCurrency
    }

    private enum ExchangeRateVariable
    {
        BaseCurrency,
        Rate
    }

    private enum ExchangeRateState
    {
        Default
    }
}
