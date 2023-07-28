// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Modules.ExchangeRate;

[ModuleTitle("Exchange Rate")]
[ModuleDescription("Calculates the exchange rate from a base currency into others")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.General)]
[ModuleInfo(@"Exchange rate can be accessed using {exchangerate.rate_CURRENCYCODE}")]
public class ExchangeRateModule : ChatBoxModule
{
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

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 5000)]
    private async void updateVariables()
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
