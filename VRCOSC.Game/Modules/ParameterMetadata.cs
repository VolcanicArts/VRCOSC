// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules;

public class ParameterMetadata
{
    public readonly string Name;
    public readonly string Description;
    public readonly Type ExpectedType;

    public string FormattedAddress => $"/avatar/parameters/{Name}";

    public ParameterMetadata(string name, string description, Type expectedType)
    {
        Name = name;
        Description = description;
        ExpectedType = expectedType;
    }
}

public class InputParameterMetadata : ParameterMetadata
{
    public readonly ActionMenu MenuLink;

    public InputParameterMetadata(string name, string description, Type expectedType, ActionMenu menuLink)
        : base(name, description, expectedType)
    {
        MenuLink = menuLink;
    }
}
