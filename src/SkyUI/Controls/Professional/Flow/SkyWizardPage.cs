namespace SkyUI.Controls.Professional;

public sealed class SkyWizardPage
{
    public string Title { get; init; } = "";

    public object? Content { get; init; }

    public bool CanProceed { get; set; } = true;
}
