// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.ExchangeRate;

public class ExchangeRate
{
    [JsonProperty("result")]
    public string Result = null!;

    [JsonProperty("conversion_rates")]
    public Dictionary<string, float> ConversionRates = null!;
}
