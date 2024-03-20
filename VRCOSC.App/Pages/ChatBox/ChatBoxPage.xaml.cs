// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows.Data;
using VRCOSC.App.ChatBox;

namespace VRCOSC.App.Pages.ChatBox;

public partial class ChatBoxPage
{
    public ChatBoxPage()
    {
        InitializeComponent();

        DataContext = ChatBoxManager.GetInstance();
    }
}

public class ClipPositionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [int start, double parentWidth])
        {
            var percentageStart = start / (float)ChatBoxManager.GetInstance().Timeline.Length.Value.TotalSeconds;
            return percentageStart * parentWidth;
        }

        return 0d;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ClipWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [int start, int end, double parentWidth])
        {
            var percentageStart = start / (float)ChatBoxManager.GetInstance().Timeline.Length.Value.TotalSeconds;
            var percentageEnd = end / (float)ChatBoxManager.GetInstance().Timeline.Length.Value.TotalSeconds;

            return (percentageEnd - percentageStart) * parentWidth;
        }

        return 0d;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
