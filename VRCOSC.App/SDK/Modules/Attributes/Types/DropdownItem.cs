// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Modules.Attributes.Types;

public class DropdownItem
{
    public string Title { get; }
    public string ID { get; }

    public DropdownItem(string title, string id)
    {
        Title = title;
        ID = id;
    }
}