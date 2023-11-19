// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Modules;

public static class OfficialModuleSecrets
{
    private static readonly Dictionary<Enum, string> secrets = new()
    {
    };

    public static string GetSecret(OfficialModuleSecretsKeys lookup) => secrets[lookup];
}

public enum OfficialModuleSecretsKeys
{
    Hyperate,
    Weather,
    ExchangeRate
}
