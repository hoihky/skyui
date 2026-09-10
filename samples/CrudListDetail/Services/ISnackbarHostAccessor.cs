using SkyUI.Controls;

namespace SkyUI.Samples.CrudListDetail.Services;

public interface ISnackbarHostAccessor
{
    SkySnackbarHost Host { get; }

    void Attach(SkySnackbarHost host);
}
