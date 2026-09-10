using SkyUI.Controls;

namespace SkyUI.Samples.CrudListDetail.Services;

public sealed class SnackbarHostAccessor : ISnackbarHostAccessor
{
    private SkySnackbarHost? host;

    public SkySnackbarHost Host =>
        host ?? throw new InvalidOperationException("Snackbar host has not been attached.");

    public void Attach(SkySnackbarHost host) => this.host = host;
}
