using Avalonia.Controls;
using SkyUI.Controls;

namespace SkyUI.UnitTests;

public class SkyValidationSummaryTests
{
    [Fact]
    public void Has_sky_classes()
    {
        var summary = new SkyValidationSummary();
        Assert.Contains("sky", summary.Classes);
        Assert.Contains("sky-validation-summary", summary.Classes);
    }

    [Fact]
    public void Header_has_default_text()
    {
        var summary = new SkyValidationSummary();
        Assert.Equal("Please fix the following:", summary.Header);
    }

    [Fact]
    public void SetErrors_populates_items()
    {
        var summary = new SkyValidationSummary();
        summary.SetErrors([
            new SkyValidationSummaryItem("email", "Email is required."),
            new SkyValidationSummaryItem("phone", "Phone is invalid."),
        ]);

        Assert.Equal(2, summary.Items.Count);
        Assert.True(summary.HasErrors);
    }

    [Fact]
    public void ClearErrors_removes_all_items()
    {
        var summary = new SkyValidationSummary();
        summary.SetErrors([new SkyValidationSummaryItem("email", "Required")]);
        summary.ClearErrors();

        Assert.Empty(summary.Items);
        Assert.False(summary.HasErrors);
    }

    [Fact]
    public void RegisterField_stores_control_for_focus_navigation()
    {
        var summary = new SkyValidationSummary();
        var textBox = new TextBox();
        summary.RegisterField("email", textBox);
        summary.SetErrors([new SkyValidationSummaryItem("email", "Required")]);

        summary.OnErrorItemActivated((SkyValidationSummaryItem)summary.Items[0]!);
        Assert.True(summary.HasErrors);
    }

    [Fact]
    public void FocusFirstInvalidField_returns_false_when_no_errors()
    {
        var summary = new SkyValidationSummary();
        Assert.False(summary.FocusFirstInvalidField());
    }

    [Fact]
    public void FocusFirstInvalidField_returns_false_for_unregistered_field()
    {
        var summary = new SkyValidationSummary();
        summary.SetErrors([new SkyValidationSummaryItem("missing", "Error")]);
        Assert.False(summary.FocusFirstInvalidField());
    }
}
