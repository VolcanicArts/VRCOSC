// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Reflection;
using VRCOSC.App.ChatBox.Clips.Variables;

namespace VRCOSC.App.UI.Views.ChatBox.Variables;

public partial class DateTimeVariableOptionView
{
    private readonly ClipVariable instance;
    private readonly PropertyInfo propertyInfo;

    public DateTimeVariableOptionView(ClipVariable instance, PropertyInfo propertyInfo)
    {
        InitializeComponent();

        this.instance = instance;
        this.propertyInfo = propertyInfo;

        proxyValue = ((DateTimeOffset)propertyInfo.GetValue(instance)!).DateTime;

        DataContext = this;
    }

    private DateTime proxyValue;

    public DateTime ProxyValue
    {
        get => proxyValue;
        set
        {
            proxyValue = value;
            propertyInfo.SetValue(instance, new DateTimeOffset(proxyValue, TimeZoneInfo.Local.GetUtcOffset(proxyValue)));
        }
    }
}
