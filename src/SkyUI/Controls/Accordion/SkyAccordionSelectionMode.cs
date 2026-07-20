namespace SkyUI.Controls;

/// <summary>How many <see cref="SkyAccordionItem"/> panels may be open at once.</summary>
public enum SkyAccordionSelectionMode
{
    /// <summary>Only one item may be expanded; expanding another collapses the rest.</summary>
    Single,

    /// <summary>Any number of items may be expanded.</summary>
    Multiple,
}
