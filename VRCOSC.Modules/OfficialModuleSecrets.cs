// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Modules;

internal static class OfficialModuleSecrets
{
    private static Dictionary<OfficialModuleSecretsKeys, string> secrets => new()
    {
    };

    internal static string GetSecret(OfficialModuleSecretsKeys key) => secrets.TryGetValue(key, out var secret) ? secret : string.Empty;
}

internal enum OfficialModuleSecretsKeys
{
    Hyperate,
    Weather,
    ExchangeRate
}
