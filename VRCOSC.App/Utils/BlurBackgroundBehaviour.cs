// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Microsoft.Xaml.Behaviors;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Utils;

public class BlurBackgroundBehavior : Behavior<Shape>
{
    public static readonly DependencyProperty BlurContainerProperty
        = DependencyProperty.Register(
            nameof(BlurContainer),
            typeof(UIElement),
            typeof(BlurBackgroundBehavior),
            new PropertyMetadata(OnContainerChanged));

    private static readonly DependencyProperty BrushProperty
        = DependencyProperty.Register(
            nameof(Brush),
            typeof(VisualBrush),
            typeof(BlurBackgroundBehavior),
            new PropertyMetadata());

    private static readonly DependencyProperty BlurProperty
        = DependencyProperty.Register(
            nameof(BlurRadius),
            typeof(double),
            typeof(BlurBackgroundBehavior),
            new PropertyMetadata
            {
                DefaultValue = 15d
            });

    private VisualBrush? Brush
    {
        get => (VisualBrush)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public UIElement? BlurContainer
    {
        get => (UIElement)GetValue(BlurContainerProperty);
        set => SetValue(BlurContainerProperty, value);
    }

    public double BlurRadius
    {
        get => (double)GetValue(BlurProperty);
        set => SetValue(BlurProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.Effect = new BlurEffect
        {
            Radius = BlurRadius,
            KernelType = KernelType.Gaussian,
            RenderingBias = RenderingBias.Quality
        };

        AssociatedObject.SetBinding(Shape.FillProperty,
            new Binding
            {
                Source = this,
                Path = new PropertyPath(BrushProperty)
            });

        AssociatedObject.LayoutUpdated += (_, _) => updateBounds();
        updateBounds();
    }

    protected override void OnDetaching()
    {
        BindingOperations.ClearBinding(AssociatedObject, Border.BackgroundProperty);
    }

    private static void OnContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((BlurBackgroundBehavior)d).OnContainerChanged((UIElement)e.OldValue, (UIElement)e.NewValue);
    }

    private void OnContainerChanged(UIElement? oldValue, UIElement? newValue)
    {
        if (oldValue != null)
        {
            oldValue.LayoutUpdated -= OnContainerLayoutUpdated;
        }

        if (newValue != null)
        {
            Brush = new VisualBrush(newValue)
            {
                ViewboxUnits = BrushMappingMode.Absolute
            };

            newValue.LayoutUpdated += OnContainerLayoutUpdated;
            updateBounds();
        }
        else
        {
            Brush = null;
        }
    }

    private void OnContainerLayoutUpdated(object? sender, EventArgs eventArgs)
    {
        updateBounds();
    }

    private void updateBounds()
    {
        if (AssociatedObject != null && BlurContainer != null && Brush != null)
        {
            Point difference = AssociatedObject.TranslatePoint(new Point(), BlurContainer);
            Brush.Viewbox = new Rect(difference, AssociatedObject.RenderSize);
        }
    }
}
