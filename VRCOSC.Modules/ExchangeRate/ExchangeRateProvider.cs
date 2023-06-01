// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.ExchangeRate;

public class ExchangeRateProvider
{
    private readonly string apiKey;

    private const string api_base_url = @"https://v6.exchangerate-api.com/v6/";
    private const string exchange_rate_url = api_base_url + "{0}/latest/{1}";

    private readonly HttpClient httpClient = new();

    private DateTimeOffset lastUpdate = DateTimeOffset.MinValue;
    private string lastCurrency = string.Empty;
    private ExchangeRate? exchangeRate;

    public ExchangeRateProvider(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<ExchangeRate?> GetExchangeRate(string baseCurrency)
    {
        if (lastUpdate + TimeSpan.FromMinutes(10) >= DateTimeOffset.Now && baseCurrency == lastCurrency) return exchangeRate;

        lastUpdate = DateTimeOffset.Now;
        lastCurrency = baseCurrency;

        var exchangeRateUrl = string.Format(exchange_rate_url, apiKey, baseCurrency);
        var exchangeRateData = await httpClient.GetAsync(new Uri(exchangeRateUrl));
        var exchangeRateResponseString = await exchangeRateData.Content.ReadAsStringAsync();
        var exchangeRateResponse = JsonConvert.DeserializeObject<ExchangeRate>(exchangeRateResponseString);

        if (exchangeRateResponse is null) return exchangeRate;
        if (exchangeRateResponse.Result != "success") return exchangeRate;

        exchangeRate = exchangeRateResponse;
        return exchangeRate;
    }
}
