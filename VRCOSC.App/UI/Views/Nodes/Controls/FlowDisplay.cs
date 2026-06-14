// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VRCOSC.App.Nodes.Types.Utility;
using VRCOSC.App.UI.Views.Nodes.ViewModels;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Views.Nodes.Controls;

public class FlowDisplay : Control
{
    private Canvas? animationCanvas;
    private DispatcherTimer updateCountTimer = null!;
    private readonly Stopwatch spawnerStopwatch = Stopwatch.StartNew();
    private readonly Queue<double> spawnTimestamps = new();

    static FlowDisplay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlowDisplay),
            new FrameworkPropertyMetadata(typeof(FlowDisplay)));
    }

    public FlowDisplay()
    {
        Loaded += FlowDisplay_Loaded;
        DataContextChanged += FlowDisplay_DataContextChanged;
    }

    /// <summary>
    /// Gets or sets the speed of the animation in milliseconds (duration for full traverse).
    /// Default is 1000ms.
    /// </summary>
    public int AnimationDuration
    {
        get => (int)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(int),
            typeof(FlowDisplay),
            new PropertyMetadata(1000));

    /// <summary>
    /// Gets or sets the brush color of the animated border.
    /// Default is Black.
    /// </summary>
    public Brush BorderColor
    {
        get => (Brush)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public static readonly DependencyProperty BorderColorProperty =
        DependencyProperty.Register(
            nameof(BorderColor),
            typeof(Brush),
            typeof(FlowDisplay),
            new PropertyMetadata(Brushes.Black));

    /// <summary>
    /// Gets or sets the width of the animated border in pixels.
    /// Default is 2.
    /// </summary>
    public double BorderWidth
    {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    public static readonly DependencyProperty BorderWidthProperty =
        DependencyProperty.Register(
            nameof(BorderWidth),
            typeof(double),
            typeof(FlowDisplay),
            new PropertyMetadata(2d));

    /// <summary>
    /// Gets or sets the corner radius of the animated border.
    /// Default is 2.
    /// </summary>
    public CornerRadius BorderCornerRadius
    {
        get => (CornerRadius)GetValue(BorderCornerRadiusProperty);
        set => SetValue(BorderCornerRadiusProperty, value);
    }

    public static readonly DependencyProperty BorderCornerRadiusProperty =
        DependencyProperty.Register(
            nameof(BorderCornerRadius),
            typeof(CornerRadius),
            typeof(FlowDisplay),
            new PropertyMetadata(new CornerRadius(2d)));

    /// <summary>
    /// Gets the current number of spawns per second.
    /// </summary>
    public int SpawnsPerSecond
    {
        get => (int)GetValue(SpawnsPerSecondProperty);
        private set => SetValue(SpawnsPerSecondProperty, value);
    }

    public static readonly DependencyProperty SpawnsPerSecondProperty =
        DependencyProperty.Register(
            nameof(SpawnsPerSecond),
            typeof(int),
            typeof(FlowDisplay),
            new PropertyMetadata(0));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        animationCanvas = GetTemplateChild("AnimationCanvas") as Canvas;

        if (animationCanvas == null)
            throw new InvalidOperationException($"{nameof(FlowDisplay)} template must contain a Canvas named 'AnimationCanvas'.");
    }

    private void UpdateSpawnCount()
    {
        var currentMs = spawnerStopwatch.Elapsed.TotalMilliseconds;
        var oneSecondAgoMs = currentMs - 1000;

        while (spawnTimestamps.Count > 0 && spawnTimestamps.Peek() < oneSecondAgoMs)
        {
            spawnTimestamps.Dequeue();
        }

        SpawnsPerSecond = spawnTimestamps.Count;
    }

    private void FlowDisplay_Loaded(object sender, RoutedEventArgs e)
    {
        if (animationCanvas == null) return;

        animationCanvas.Width = ActualWidth;
        animationCanvas.Height = ActualHeight;

        updateCountTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        updateCountTimer.Tick += (_, _) => UpdateSpawnCount();
        updateCountTimer.Start();
    }

    private void FlowDisplay_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is NodeViewModel { Node: FlowDisplayNode node })
            bind(node);
    }

    private void bind(FlowDisplayNode node) => node.OnCall += Spawn;

    public void Spawn() => Task.Run(() => Dispatcher.Invoke(() =>
    {
        if (animationCanvas == null) return;
        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        spawnTimestamps.Enqueue(spawnerStopwatch.Elapsed.TotalMilliseconds);

        var animatedBorder = new Border
        {
            Width = BorderWidth,
            Height = ActualHeight,
            Background = BorderColor,
            Opacity = 0.9,
            CornerRadius = BorderCornerRadius
        };

        animationCanvas.Children.Add(animatedBorder);

        Canvas.SetLeft(animatedBorder, 0);
        Canvas.SetTop(animatedBorder, 0);

        var moveAnimation = new DoubleAnimation
        {
            From = 0,
            To = ActualWidth - BorderWidth,
            Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration))
        };

        moveAnimation.Completed += (_, _) => animationCanvas.Children.Remove(animatedBorder);
        animatedBorder.BeginAnimation(Canvas.LeftProperty, moveAnimation);
    }));

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (animationCanvas == null) return;

        animationCanvas.Width = sizeInfo.NewSize.Width;
        animationCanvas.Height = sizeInfo.NewSize.Height;
    }
}