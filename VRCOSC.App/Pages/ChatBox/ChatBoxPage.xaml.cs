// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips;

namespace VRCOSC.App.Pages.ChatBox;

public partial class ChatBoxPage
{
    private double mouseX;
    private Clip? draggingClip;
    private ClipDragPoint clipDragPoint;
    private float cumulativeDrag;

    public ChatBoxPage()
    {
        InitializeComponent();

        DataContext = ChatBoxManager.GetInstance();
        SizeChanged += (_, _) => drawVerticalLines();
    }

    private void ChatBoxPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ChatBoxManager.GetInstance().Timeline.Length.Subscribe(_ => drawVerticalLines(), true);
    }

    private void drawVerticalLines()
    {
        LineCanvas.Children.Clear();

        var numberOfLines = ChatBoxManager.GetInstance().Timeline.TimelineLengthSeconds - 1;
        var resolution = ChatBoxManager.GetInstance().Timeline.TimelineResolution;

        for (int i = 0; i < numberOfLines; i++)
        {
            var x = (i + 1) * (resolution * LineCanvas.ActualWidth);

            Line line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = LineCanvas.ActualHeight,
                Stroke = (Brush)FindResource("CBackground1"),
                StrokeThickness = 1
            };

            LineCanvas.Children.Add(line);
        }
    }

    private void Clip_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var clipGrid = (Grid)sender;
        var clip = (Clip)clipGrid.Tag;

        Console.WriteLine("Click " + clip.Name.Value);
    }

    private void ClipMain_OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;
        var clip = (Clip)frameworkElement.Tag;

        draggingClip = clip;
        clipDragPoint = ClipDragPoint.Center;
    }

    private void ClipMain_OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        draggingClip = null;
        cumulativeDrag = 0f;
    }

    private void ClipLeft_OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;
        var clip = (Clip)frameworkElement.Tag;

        draggingClip = clip;
        clipDragPoint = ClipDragPoint.Left;
    }

    private void ClipLeft_OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        draggingClip = null;
        cumulativeDrag = 0f;
    }

    private void ClipRight_OnLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;
        var clip = (Clip)frameworkElement.Tag;

        draggingClip = clip;
        clipDragPoint = ClipDragPoint.Right;
    }

    private void ClipRight_OnLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        draggingClip = null;
        cumulativeDrag = 0f;
    }

    private void Timeline_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        draggingClip = null;
        cumulativeDrag = 0f;
    }

    private void Timeline_MouseMove(object sender, MouseEventArgs e)
    {
        var newMouseX = e.GetPosition((FrameworkElement)sender).X;
        var xDelta = newMouseX - mouseX;
        mouseX = newMouseX;

        if (draggingClip is null)
        {
            cumulativeDrag = 0f;
            return;
        }

        var timelineWidth = Timeline.ActualWidth;
        var percentage = (float)xDelta / (float)timelineWidth;

        cumulativeDrag += percentage;

        if (draggingClip is not null)
        {
            var clipLayer = ChatBoxManager.GetInstance().Timeline.FindLayerOfClip(draggingClip);

            if (clipDragPoint == ClipDragPoint.Center && MathF.Abs(cumulativeDrag) >= ChatBoxManager.GetInstance().Timeline.TimelineResolution)
            {
                var newStart = draggingClip.Start.Value + MathF.Sign(cumulativeDrag);
                var newEnd = draggingClip.End.Value + MathF.Sign(cumulativeDrag);

                var (lowerBound, _) = clipLayer.GetBoundsNearestTo(draggingClip.Start.Value, false);
                var (_, upperBound) = clipLayer.GetBoundsNearestTo(draggingClip.End.Value, true);

                if (newStart >= lowerBound && newEnd <= upperBound)
                {
                    draggingClip.Start.Value = newStart;
                    draggingClip.End.Value = newEnd;
                }

                cumulativeDrag = 0f;
            }

            if (clipDragPoint == ClipDragPoint.Left)
            {
                var mousePosNormalised = mouseX / Timeline.ActualWidth;
                var newStart = (int)Math.Floor(mousePosNormalised * ChatBoxManager.GetInstance().Timeline.TimelineLengthSeconds);

                if (newStart != draggingClip.Start.Value && newStart < draggingClip.End.Value)
                {
                    var (lowerBound, upperBound) = clipLayer.GetBoundsNearestTo(newStart < draggingClip.Start.Value ? draggingClip.Start.Value : newStart, false);

                    if (newStart >= lowerBound && newStart < upperBound)
                    {
                        draggingClip.Start.Value = newStart;
                    }
                }
            }

            if (clipDragPoint == ClipDragPoint.Right)
            {
                var mousePosNormalised = mouseX / Timeline.ActualWidth;
                var newEnd = (int)Math.Ceiling(mousePosNormalised * ChatBoxManager.GetInstance().Timeline.TimelineLengthSeconds);

                if (newEnd != draggingClip.End.Value && newEnd > draggingClip.Start.Value)
                {
                    var (lowerBound, upperBound) = clipLayer.GetBoundsNearestTo(newEnd < draggingClip.End.Value ? newEnd : draggingClip.End.Value, true);

                    if (newEnd > lowerBound && newEnd <= upperBound)
                    {
                        draggingClip.End.Value = newEnd;
                    }
                }
            }
        }
    }
}

public enum ClipDragPoint
{
    Left,
    Center,
    Right
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
