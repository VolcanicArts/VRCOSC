using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

// Taken from MahApps.Metro
public static class PasswordHelper
{
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordHelper),
            new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordHelper),
            new PropertyMetadata(false, attach));

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordHelper));

    private static void attach(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= passwordChanged;
                passwordBox.KeyDown -= passwordKeyDown;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += passwordChanged;
                passwordBox.KeyDown += passwordKeyDown;
            }
        }
    }

    public static string GetPassword(DependencyObject d)
    {
        return (string)d.GetValue(PasswordProperty);
    }

    public static void SetPassword(DependencyObject d, string value)
    {
        d.SetValue(PasswordProperty, value);
    }

    private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged -= passwordChanged;

            if (!(bool)passwordBox.GetValue(IsUpdatingProperty))
            {
                passwordBox.Password = (string)e.NewValue;
            }

            passwordBox.PasswordChanged += passwordChanged;
        }
    }

    private static void passwordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            passwordBox.SetValue(IsUpdatingProperty, true);
            SetPassword(passwordBox, passwordBox.Password);
            passwordBox.SetValue(IsUpdatingProperty, false);
        }
    }

    private static void passwordKeyDown(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
        {
            Clipboard.SetText(((PasswordBox)sender).Password);
        }
    }

    public static bool GetAttach(DependencyObject d)
    {
        return (bool)d.GetValue(AttachProperty);
    }

    public static void SetAttach(DependencyObject d, bool value)
    {
        d.SetValue(AttachProperty, value);
    }
}
