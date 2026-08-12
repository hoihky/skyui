using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace SkyUI.Controls;

/// <summary>Attached properties for Sky button behaviors.</summary>
public static class SkyButtonProperties
{
    public static readonly AttachedProperty<bool> IsLoadingProperty =
        AvaloniaProperty.RegisterAttached<Button, bool>("IsLoading", typeof(SkyButtonProperties));

    private static readonly AttachedProperty<object?> LoadingContentBackupProperty =
        AvaloniaProperty.RegisterAttached<Button, object?>("LoadingContentBackup", typeof(SkyButtonProperties));

    private static readonly AttachedProperty<bool?> LoadingEnabledBackupProperty =
        AvaloniaProperty.RegisterAttached<Button, bool?>("LoadingEnabledBackup", typeof(SkyButtonProperties));

    static SkyButtonProperties()
    {
        IsLoadingProperty.Changed.AddClassHandler<Button>(OnIsLoadingChanged);
    }

    public static bool GetIsLoading(Button button) => button.GetValue(IsLoadingProperty);

    public static void SetIsLoading(Button button, bool value) => button.SetValue(IsLoadingProperty, value);

    private static void OnIsLoadingChanged(Button button, AvaloniaPropertyChangedEventArgs e)
    {
        var loading = e.GetNewValue<bool>();
        button.Classes.Set(SkyThemeClassNames.Loading, loading);

        if (loading)
            BeginLoading(button);
        else
            EndLoading(button);
    }

    private static void BeginLoading(Button button)
    {
        if (button.GetValue(LoadingContentBackupProperty) is null)
            button.SetValue(LoadingContentBackupProperty, button.Content);

        if (button.GetValue(LoadingEnabledBackupProperty) is null)
            button.SetValue(LoadingEnabledBackupProperty, button.IsEnabled);

        button.Content = CreateLoadingIndicator(button);
        button.IsEnabled = false;
    }

    private static void EndLoading(Button button)
    {
        if (button.GetValue(LoadingContentBackupProperty) is { } saved)
        {
            button.Content = saved;
            button.ClearValue(LoadingContentBackupProperty);
        }

        if (button.GetValue(LoadingEnabledBackupProperty) is bool wasEnabled)
        {
            button.IsEnabled = wasEnabled;
            button.ClearValue(LoadingEnabledBackupProperty);
        }
    }

    private static SkyProgressRing CreateLoadingIndicator(Button button) =>
        new()
        {
            IsIndeterminate = true,
            Width = 18,
            Height = 18,
            MinWidth = 18,
            MinHeight = 18,
            StrokeThickness = 2,
            Foreground = button.Foreground,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
}

/// <summary>Stable style class names used by primitive helpers.</summary>
internal static class SkyThemeClassNames
{
    public const string Loading = "sky-loading";
}
