namespace SkyUI.Controls.Professional;

/// <summary>Logical document hosted in <see cref="SkyDocumentWell"/>.</summary>
public interface ISkyDocument
{
    Guid Id { get; }

    string Title { get; set; }

    bool IsDirty { get; set; }

    object? Content { get; set; }

    /// <summary>When false, close UI is hidden and <see cref="SkyDocumentWell"/> refuses to close.</summary>
    bool CanClose { get; set; }

    /// <summary>Dock shell region for this document (central well, tool band, or float).</summary>
    SkyDocumentShellPlacement ShellPlacement { get; set; }

    /// <summary>Target placement for float-window dock-back affordance.</summary>
    SkyDocumentShellPlacement ShellDockBack { get; set; }

    double ShellFloatLeft { get; set; }

    double ShellFloatTop { get; set; }
}

