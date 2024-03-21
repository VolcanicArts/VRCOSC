// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        SizeChanged += (_, _) => drawLines();
    }

    private Clip? selectedClip;

    public Clip? SelectedClip
    {
        get => selectedClip;
        set
        {
            if (selectedClip == value) return;

            var selectedClipBefore = selectedClip;
            selectedClip = value;
            SelectedClipGrid.DataContext = selectedClip;

            if (selectedClip is null && selectedClipBefore is not null)
                fadeOut(SelectedClipGrid, 150);

            if (selectedClip is not null && selectedClipBefore is null)
                fadeIn(SelectedClipGrid, 150);
        }
    }

    private void ChatBoxPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ChatBoxManager.GetInstance().Timeline.Length.Subscribe(_ => drawLines());
        ChatBoxManager.GetInstance().Timeline.LayerCount.Subscribe(_ => drawLines());
        drawLines();
    }

    private void drawLines()
    {
        LineCanvas.Children.Clear();

        var verticalLineCount = ChatBoxManager.GetInstance().Timeline.LengthSeconds - 1;
        var resolution = ChatBoxManager.GetInstance().Timeline.Resolution;

        for (var i = 0; i < verticalLineCount; i++)
        {
            var x = (i + 1) * (resolution * LineCanvas.ActualWidth);

            var line = new Line
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

        var horizontalLineCount = ChatBoxManager.GetInstance().Timeline.LayerCount.Value - 1;

        for (var i = 0; i < horizontalLineCount; i++)
        {
            var y = (i + 1) * 50;

            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = LineCanvas.ActualWidth,
                Y2 = y,
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

        SelectedClip = clip;
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

            if (clipDragPoint == ClipDragPoint.Center && MathF.Abs(cumulativeDrag) >= ChatBoxManager.GetInstance().Timeline.Resolution)
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
                var newStart = (int)Math.Floor(mousePosNormalised * ChatBoxManager.GetInstance().Timeline.LengthSeconds);

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
                var newEnd = (int)Math.Ceiling(mousePosNormalised * ChatBoxManager.GetInstance().Timeline.LengthSeconds);

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

    private void EditClip_ButtonClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(SelectedClip is not null);
        var clipEditWindow = new ChatBoxClipEditWindow(SelectedClip);
        clipEditWindow.ShowDialog();
    }

    private void fadeIn(FrameworkElement grid, double fadeInTimeMilli) => Dispatcher.Invoke(() =>
    {
        grid.Visibility = Visibility.Visible;
        grid.Opacity = 0;

        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(fadeInTimeMilli)
        };

        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeInAnimation);
        storyboard.Begin(grid);
    });

    private void fadeOut(FrameworkElement grid, double fadeOutTime) => Dispatcher.Invoke(() =>
    {
        grid.Opacity = 1;

        var fadeOutAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromMilliseconds(fadeOutTime)
        };

        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeOutAnimation);
        storyboard.Completed += (_, _) => grid.Visibility = Visibility.Collapsed;
        storyboard.Begin(grid);
    });

    private void Layer_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var layerElement = (FrameworkElement)sender;
        var layer = (Layer)layerElement.Tag;

        Console.WriteLine(ChatBoxManager.GetInstance().Timeline.Layers.IndexOf(layer));
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

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
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

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ClipEnabledToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool enabled)
        {
            return enabled ? 1f : 0.6f;
        }

        return 1f;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
