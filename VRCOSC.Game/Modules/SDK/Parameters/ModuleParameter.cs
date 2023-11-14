// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;

namespace VRCOSC.Game.Modules.SDK.Parameters;

internal class ModuleParameter
{
    internal readonly Bindable<string> Name;

    internal readonly string Title;
    internal readonly string Description;
    internal readonly ParameterMode Mode;
    internal readonly Type ExpectedType;

    internal ModuleParameter(string defaultName, string title, string description, ParameterMode mode, Type expectedType)
    {
        Name = new Bindable<string>(defaultName);
        Title = title;
        Description = description;
        Mode = mode;
        ExpectedType = expectedType;
    }
}
