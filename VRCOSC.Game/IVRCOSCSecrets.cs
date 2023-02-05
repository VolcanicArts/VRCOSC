// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game;

public interface IVRCOSCSecrets
{
    public string GetSecret(VRCOSCSecretsKeys key);
}
