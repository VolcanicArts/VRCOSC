// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.Pages.ChatBox;
using VRCOSC.App.Profiles;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.UI.Windows.ChatBox;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.ChatBox;

public partial class ChatBoxView
{
    private double mouseX;
    private Clip? draggingClip;
    private ClipDragPoint clipDragPoint;
    private Window? manualInputWindow;

    public Clip? CopiedClip { get; private set; }
    public Observable<Visibility> PasteClipButtonVisibility { get; } = new(Visibility.Collapsed);

    public ChatBoxManager ChatBoxManager => ChatBoxManager.GetInstance();

    public ChatBoxView()
    {
        InitializeComponent();

        DataContext = this;
        SizeChanged += (_, _) => drawLines();

        AppManager.GetInstance().State.Subscribe(newState => Dispatcher.Invoke(() => Indicator.Visibility = newState == AppManagerState.Started ? Visibility.Visible : Visibility.Collapsed), true);
    }

    private Border? clipBorder;
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
                SelectedClipGrid.FadeOutFromOne(150);

            if (selectedClip is not null && selectedClipBefore is null)
                SelectedClipGrid.FadeInFromZero(150);
        }
    }

    private void ChatBoxPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        ChatBoxManager.GetInstance().Timeline.Length.Subscribe(_ => drawLines(), true);
    }

    private void ChatBoxPage_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        RightClickMenu.FadeOutFromOne(50);
    }

    private void ChatBoxPage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        draggingClip = null;
    }

    private void drawLines()
    {
        HeaderGrid.Children.Clear();
        HeaderLineCanvas.Children.Clear();
        LineCanvas.Children.Clear();

        var verticalLineCount = ChatBoxManager.GetInstance().Timeline.Length.Value - 1;
        var resolution = ChatBoxManager.GetInstance().Timeline.Resolution;

        for (var i = 1; i <= verticalLineCount; i++)
        {
            var significant = i % 5 == 0;
            var x = i * (resolution * LineCanvas.ActualWidth);

            var line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = LineCanvas.ActualHeight,
                Stroke = significant ? (Brush)FindResource("CForeground2") : (Brush)FindResource("CBackground1"),
                StrokeThickness = 1,
                Opacity = significant ? 0.5f : 1f
            };

            LineCanvas.Children.Add(line);

            if (significant)
            {
                var headerLine = new Line
                {
                    X1 = x,
                    Y1 = HeaderLineCanvas.ActualHeight * 0.75f,
                    X2 = x,
                    Y2 = HeaderLineCanvas.ActualHeight,
                    Stroke = (Brush)FindResource("CForeground2"),
                    StrokeThickness = 1
                };

                HeaderLineCanvas.Children.Add(headerLine);

                drawTimelineHeaderText(i.ToString(), x);
            }
        }

        var horizontalLineCount = ChatBoxManager.GetInstance().Timeline.LayerCount - 1;

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

    private void drawTimelineHeaderText(string text, double xPos)
    {
        var centeredText = new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = (Brush)FindResource("CForeground2")
        };

        centeredText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var xPosCentered = xPos - centeredText.DesiredSize.Width / 2f;
        var yPos = (HeaderGrid.ActualHeight - centeredText.DesiredSize.Height) * 0.25f;

        centeredText.Margin = new Thickness(xPosCentered, yPos, 0, 0);

        HeaderGrid.Children.Add(centeredText);
    }

    private void Clip_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var clipGrid = (Grid)sender;
        var clip = (Clip)clipGrid.Tag;
        e.Handled = true;

        if (clipBorder is not null)
            clipBorder!.Background = (Brush)FindResource("CBackground4");

        clipBorder = clipGrid.FindVisualParent<Border>("ClipBorder");
        clipBorder!.Background = (Brush)FindResource("CBackground6");
        SelectedClip = clip;
        RightClickMenu.FadeOutFromOne(50);
    }

    private void Clip_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var clipGrid = (Grid)sender;
        var clip = (Clip)clipGrid.Tag;
        e.Handled = true;

        if (clipBorder is not null)
            clipBorder!.Background = (Brush)FindResource("CBackground4");

        clipBorder = clipGrid.FindVisualParent<Border>("ClipBorder");
        clipBorder!.Background = (Brush)FindResource("CBackground6");
        SelectedClip = clip;

        showRightClickMenu(null, clip);
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
    }

    private void Timeline_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ChatBoxManager.GetInstance().Serialise();
        draggingClip = null;
        e.Handled = true;
    }

    private double mouseXPercentageOffset = -1;

    private void Timeline_MouseMove(object sender, MouseEventArgs e)
    {
        e.Handled = true;

        if (e.LeftButton == MouseButtonState.Released)
        {
            draggingClip = null;
        }

        var senderElement = (FrameworkElement)sender;
        mouseX = e.GetPosition(senderElement).X;

        if (draggingClip is null)
        {
            mouseXPercentageOffset = -1;
            return;
        }

        if (draggingClip is not null)
        {
            var mouseXPercentage = mouseX / Timeline.ActualWidth;

            var timelineLength = ChatBoxManager.GetInstance().Timeline.Length.Value;

            if (mouseXPercentageOffset == -1)
            {
                mouseXPercentageOffset = mouseXPercentage - ((double)draggingClip.Start.Value / timelineLength);
            }

            var clipLayer = draggingClip.Layer.Value;

            if (clipDragPoint == ClipDragPoint.Center)
            {
                var newStart = (int)Math.Floor((mouseXPercentage - mouseXPercentageOffset) * timelineLength);
                var newEnd = draggingClip.End.Value + (newStart - draggingClip.Start.Value);

                var (lowerBound, _) = ChatBoxManager.GetInstance().Timeline.GetBoundsNearestTo(draggingClip.Start.Value, clipLayer, false);
                var (_, upperBound) = ChatBoxManager.GetInstance().Timeline.GetBoundsNearestTo(draggingClip.End.Value, clipLayer, true);

                var noneIntersect = ChatBoxManager.GetInstance().Timeline.Clips.Where(clip => clip.Layer.Value == clipLayer && clip != draggingClip).All(clip => !clip.Intersects(new Clip
                {
                    Start = { Value = newStart },
                    End = { Value = newEnd }
                }));

                if ((newStart >= lowerBound && newEnd <= upperBound) || (noneIntersect && newStart >= 0 && newEnd <= timelineLength))
                {
                    draggingClip.Start.Value = newStart;
                    draggingClip.End.Value = newEnd;
                    ChatBoxManager.GetInstance().Timeline.GenerateDroppableAreas(clipLayer);
                }
            }

            if (clipDragPoint == ClipDragPoint.Left)
            {
                var newStart = (int)Math.Floor(mouseXPercentage * timelineLength);

                if (draggingClip.End.Value - newStart < 2) return;

                if (newStart != draggingClip.Start.Value && newStart < draggingClip.End.Value)
                {
                    var (lowerBound, upperBound) = ChatBoxManager.GetInstance().Timeline.GetBoundsNearestTo(newStart < draggingClip.Start.Value ? draggingClip.Start.Value : newStart, clipLayer, false);

                    if (newStart >= lowerBound && newStart < upperBound)
                    {
                        draggingClip.Start.Value = newStart;
                        ChatBoxManager.GetInstance().Timeline.GenerateDroppableAreas(clipLayer);
                    }
                }
            }

            if (clipDragPoint == ClipDragPoint.Right)
            {
                var newEnd = (int)Math.Ceiling(mouseXPercentage * timelineLength);

                if (newEnd - draggingClip.Start.Value < 2) return;

                if (newEnd != draggingClip.End.Value && newEnd > draggingClip.Start.Value)
                {
                    var (lowerBound, upperBound) = ChatBoxManager.GetInstance().Timeline.GetBoundsNearestTo(newEnd < draggingClip.End.Value ? newEnd : draggingClip.End.Value, clipLayer, true);

                    if (newEnd > lowerBound && newEnd <= upperBound)
                    {
                        draggingClip.End.Value = newEnd;
                        ChatBoxManager.GetInstance().Timeline.GenerateDroppableAreas(clipLayer);
                    }
                }
            }
        }
    }

    private readonly List<ChatBoxClipEditWindow> clipEditWindowCache = new();

    private void EditClip_ButtonClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(SelectedClip is not null);

        var clipEditWindow = clipEditWindowCache.FirstOrDefault(clipEditWindow => clipEditWindow.ReferenceClip == SelectedClip);

        if (clipEditWindow is null)
        {
            clipEditWindow = new ChatBoxClipEditWindow(SelectedClip);
            clipEditWindow.Closed += (_, _) => clipEditWindowCache.Remove(clipEditWindow);
            clipEditWindowCache.Add(clipEditWindow);
            clipEditWindow.Show();
        }
        else
        {
            clipEditWindow.WindowState = WindowState.Normal;
            clipEditWindow.Focus();
        }
    }

    private void TimelineContent_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (clipBorder is not null)
            clipBorder.Background = (Brush)FindResource("CBackground4");

        SelectedClip = null;
        e.Handled = true;
        RightClickMenu.FadeOutFromOne(50);
    }

    private void TimelineContent_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        var layer = (int)Math.Floor(e.GetPosition(Timeline).Y / 50d);

        if (clipBorder is not null)
            clipBorder.Background = (Brush)FindResource("CBackground4");

        SelectedClip = null;
        showRightClickMenu(layer, null);
    }

    private void TimelineContent_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (draggingClip is null) return;

        var layer = (int)Math.Floor(e.GetPosition(Timeline).Y / 50d);
        if (draggingClip.Layer.Value == layer) return;

        DragDrop.DoDragDrop(this, new object(), DragDropEffects.Move);
    }

    private void TextBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        textBox.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
    }

    private void TextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            Keyboard.ClearFocus();
    }

    private void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        WinForms.OpenFile("chatbox.json|*.json", filePath => Dispatcher.Invoke(() => ChatBoxManager.GetInstance().Deserialise(filePath)));
    }

    private void ExportButton_OnClick(object sender, RoutedEventArgs e)
    {
        var filePath = AppManager.GetInstance().Storage.GetFullPath($"profiles/{ProfileManager.GetInstance().ActiveProfile.Value.ID}/chatbox.json");
        WinForms.PresentFile(filePath);
    }

    private void ClearButton_OnClick(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(MainWindow.GetInstance(), "Warning. This will erase your entire timeline. Are you sure?", "Erase Timeline?", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        // Clearing the clips doesn't fire the collection changed event. This fixes it
        ChatBoxManager.GetInstance().Timeline.Clips.RemoveIf(_ => true);
    }

    private void showRightClickMenu(int? layer, Clip? clip)
    {
        RightClickMenu.Visibility = Visibility.Collapsed;

        RightClickMenuLayerOptions.Visibility = layer is not null ? Visibility.Visible : Visibility.Collapsed;
        RightClickMenuClipOptions.Visibility = clip is not null ? Visibility.Visible : Visibility.Collapsed;

        RightClickMenu.Tag = layer is not null ? layer : clip;

        var mousePos = Mouse.GetPosition(TimelineContainer);
        RightClickMenu.RenderTransform = new TranslateTransform(mousePos.X, mousePos.Y);

        RightClickMenu.FadeInFromZero(50);
    }

    private void RightClickMenu_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void createClip()
    {
        var layer = (int)RightClickMenu.Tag;

        var xPos = ((TranslateTransform)RightClickMenu.RenderTransform).X;
        var xPosNormalised = xPos / TimelineContainer.ActualWidth;
        var closestSecond = (int)Math.Floor(xPosNormalised * ChatBoxManager.GetInstance().Timeline.Length.Value);

        var (lowerBound, upperBound) = ChatBoxManager.GetInstance().Timeline.GetBoundsNearestTo(closestSecond, layer, false, true);

        var clip = new Clip
        {
            Layer = { Value = layer },
            Start = { Value = lowerBound },
            End = { Value = upperBound }
        };

        ChatBoxManager.GetInstance().Timeline.Clips.Add(clip);

        RightClickMenu.FadeOutFromOne(50);
    }

    private void CreateClipOnLayer_OnClick(object sender, RoutedEventArgs e)
    {
        createClip();
    }

    private void DeleteClip_OnClick(object sender, RoutedEventArgs e)
    {
        var clip = (Clip)RightClickMenu.Tag;

        var editWindow = clipEditWindowCache.FirstOrDefault(window => window.ReferenceClip == clip);
        editWindow?.Close();

        if (clipBorder is not null)
            clipBorder.Background = (Brush)FindResource("CBackground4");

        SelectedClip = null;

        ChatBoxManager.GetInstance().Timeline.Clips.Remove(clip);
        RightClickMenu.FadeOutFromOne(50);
    }

    private void DroppableArea_OnDrop(object sender, DragEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var droppableArea = (DroppableArea)element.Tag;

        Debug.Assert(draggingClip is not null);

        var clipWidth = draggingClip.End.Value - draggingClip.Start.Value;
        var areaWidth = droppableArea.End - droppableArea.Start;

        if (droppableArea.Start == draggingClip.End.Value) return;
        if (droppableArea.End == draggingClip.Start.Value) return;

        var previousLayer = draggingClip.Layer.Value;
        draggingClip.Layer.Value = droppableArea.Layer;

        if (areaWidth >= clipWidth)
        {
            draggingClip.Start.Value = droppableArea.Start;
            draggingClip.End.Value = droppableArea.Start + clipWidth;
        }
        else
        {
            draggingClip.Start.Value = droppableArea.Start;
            draggingClip.End.Value = droppableArea.End;
        }

        ChatBoxManager.GetInstance().Timeline.GenerateDroppableAreas(previousLayer);
        ChatBoxManager.GetInstance().Timeline.GenerateDroppableAreas(draggingClip.Layer.Value);

        draggingClip = null;
    }

    private void IgnoreErrors_ButtonClick(object sender, RoutedEventArgs e)
    {
        var confirmation = MessageBox.Show("Ignoring errors can result in loss of data if you don't know what you're doing. Are you sure you want to ignore the errors?", "Ignore errors?",
            MessageBoxButton.YesNo);

        if (confirmation != MessageBoxResult.Yes) return;

        ChatBoxManager.GetInstance().Deserialise(string.Empty, true);
    }

    private void WriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (manualInputWindow is null)
        {
            manualInputWindow = new ChatBoxManualInputWindow();
            manualInputWindow.Closed += (_, _) => manualInputWindow = null;
            manualInputWindow.Show();
        }
        else
        {
            manualInputWindow.WindowState = WindowState.Normal;
            manualInputWindow.Focus();
        }
    }

    private void CopyClip_OnClick(object sender, RoutedEventArgs e)
    {
        CopiedClip = SelectedClip;
        PasteClipButtonVisibility.Value = Visibility.Visible;
        RightClickMenu.FadeOutFromOne(50);
    }

    private void PasteClipOnLayer_OnClick(object sender, RoutedEventArgs e)
    {
        Debug.Assert(CopiedClip is not null);

        var layer = (int)RightClickMenu.Tag;

        var xPos = ((TranslateTransform)RightClickMenu.RenderTransform).X;
        var xPosNormalised = xPos / TimelineContainer.ActualWidth;
        var closestSecond = (int)Math.Floor(xPosNormalised * ChatBoxManager.GetInstance().Timeline.Length.Value);

        var (lowerBound, upperBound) = ChatBoxManager.GetInstance().Timeline.GetBoundsNearestTo(closestSecond, layer, false, true);

        var newClipEnd = Math.Min(CopiedClip.End.Value, upperBound);

        var newClip = new Clip
        {
            Layer = { Value = layer },
            Start = { Value = lowerBound },
            End = { Value = newClipEnd },
            Name = { Value = CopiedClip.Name.Value }
        };

        newClip.LinkedModules.AddRange(CopiedClip.LinkedModules);

        ChatBoxManager.GetInstance().Timeline.Clips.Add(newClip);

        CopiedClip = null;
        PasteClipButtonVisibility.Value = Visibility.Collapsed;
        RightClickMenu.FadeOutFromOne(50);
    }
}

public enum ClipDragPoint
{
    Left,
    Center,
    Right
}

public class IndicatorPositionConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is [double percentage, double timelineWidth])
        {
            return percentage * timelineWidth - 2.5f;
        }

        return 0d;
    }

    public object[]? ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => null;
}

public class ClipXPositionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [int start, double parentWidth])
        {
            var percentageStart = start / (double)ChatBoxManager.GetInstance().Timeline.Length.Value;
            return percentageStart * parentWidth;
        }

        return 0d;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}

public class ClipYPositionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [int layer, double layerHeight])
        {
            return layer * layerHeight;
        }

        return 0d;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}

public class ClipWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [int start, int end, double parentWidth])
        {
            var percentageStart = start / (double)ChatBoxManager.GetInstance().Timeline.Length.Value;
            var percentageEnd = end / (double)ChatBoxManager.GetInstance().Timeline.Length.Value;

            return (percentageEnd - percentageStart) * parentWidth;
        }

        return 0d;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}

public class ClipEnabledToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool enabled)
        {
            return enabled ? 1f : 0.6f;
        }

        return 1f;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class BoolToThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? new Thickness(1) : new Thickness(0);
        }

        return new Thickness(0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}