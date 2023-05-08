// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Serialisation;

/// <summary>
/// Represents an object that can be serialised using an <see cref="ISerialiser"/>
/// </summary>
public interface ICanSerialise
{
    public void Deserialise();
    public void Serialise();
}
