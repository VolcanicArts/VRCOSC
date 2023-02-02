// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game;

namespace VRCOSC.Modules;

public class VRCOSCModuleSecrets : IVRCOSCSecrets
{
    private readonly Dictionary<VRCOSCSecretsKeys, string> secrets = new();

    public VRCOSCModuleSecrets()
    {
    }

    public string GetSecret(VRCOSCSecretsKeys key) => secrets.TryGetValue(key, out var value) ? value : string.Empty;
}
