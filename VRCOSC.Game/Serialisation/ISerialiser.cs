// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Serialisation;

public interface ISerialiser
{
    public bool Deserialise();
    public bool Serialise();
}
