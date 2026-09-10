using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.Demo.ViewModels;

namespace SkyUI.Demo.Views.Demos;

public partial class FormsDemo : UserControl
{
    public FormsDemo()
    {
        InitializeComponent();
        DataContext = new FormsDemoViewModel();
        ValidationSummary.RegisterField("email", EmailField);
    }

    private void OnValidateFormClick(object? sender, RoutedEventArgs e)
    {
        var emailValid = EmailField.Validate();
        var errors = new List<SkyValidationSummaryItem>();
        if (!emailValid)
            errors.Add(new SkyValidationSummaryItem("email", EmailField.ErrorMessage ?? "Email is invalid."));

        ValidationSummary.SetErrors(errors);
        if (!emailValid)
            ValidationSummary.FocusFirstInvalidField();
    }
}
