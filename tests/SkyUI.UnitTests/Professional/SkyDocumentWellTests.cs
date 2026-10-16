using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using SkyUI.Controls.Professional;

namespace SkyUI.UnitTests.Professional;

public sealed class SkyDocumentWellTests
{
    [Fact]
    public void SkyDocument_notifies_title_and_dirty_changes()
    {
        var document = new SkyDocument("A");
        var names = new List<string?>();
        document.PropertyChanged += (_, e) => names.Add(e.PropertyName);

        document.Title = "B";
        document.IsDirty = true;

        Assert.Contains(nameof(SkyDocument.Title), names);
        Assert.Contains(nameof(SkyDocument.IsDirty), names);
    }

    [Fact]
    public void DefaultSkyDocumentTabTitleFormatter_appends_dirty_suffix()
    {
        var formatter = new DefaultSkyDocumentTabTitleFormatter();
        var document = new SkyDocument("Report") { IsDirty = true };

        Assert.Equal("Report •", formatter.Format(document, " •"));
        document.IsDirty = false;
        Assert.Equal("Report", formatter.Format(document, " •"));
    }

    [Fact]
    public void DefaultSkyDocumentTabTitleFormatter_uses_untitled_for_blank_title()
    {
        var formatter = new DefaultSkyDocumentTabTitleFormatter();
        var document = new SkyDocument { Title = "   " };
        Assert.Equal("Untitled", formatter.Format(document, "*"));
    }

    [Fact]
    public void SkyDocumentWellController_try_add_and_index()
    {
        var documents = new ObservableCollection<ISkyDocument>
        {
            new SkyDocument("One"),
            new SkyDocument("Two"),
        };
        var controller = new SkyDocumentWellController { ItemsSource = documents };
        var fresh = new SkyDocument("Three");

        Assert.True(controller.TryAdd(fresh));
        Assert.Equal(3, controller.Documents.Count);
        Assert.Equal(2, controller.IndexOf(fresh));
        Assert.False(controller.TryAdd(fresh));
    }

    [Fact]
    public void SkyDocumentWellController_try_remove()
    {
        var one = new SkyDocument("One");
        var documents = new ObservableCollection<ISkyDocument> { one, new SkyDocument("Two") };
        var controller = new SkyDocumentWellController { ItemsSource = documents };

        Assert.True(controller.TryRemove(one));
        Assert.Equal(1, controller.Documents.Count);
        Assert.False(controller.TryRemove(one));
    }

    [Fact]
    public void SkyDocumentWellController_try_move_reorders_list()
    {
        var a = new SkyDocument("A");
        var b = new SkyDocument("B");
        var c = new SkyDocument("C");
        var documents = new ObservableCollection<ISkyDocument> { a, b, c };
        var controller = new SkyDocumentWellController { ItemsSource = documents };

        Assert.True(controller.TryMove(0, 2));
        Assert.Equal("B", controller.Documents[0].Title);
        Assert.Equal("C", controller.Documents[1].Title);
        Assert.Equal("A", controller.Documents[2].Title);
    }

    [Fact]
    public void SkyDocumentWellController_clamp_selection_when_empty()
    {
        var controller = new SkyDocumentWellController { ItemsSource = new ObservableCollection<ISkyDocument>() };
        Assert.Equal(-1, controller.ClampSelectionIndex(3, null));
        Assert.Null(controller.ResolveSelection(0, null));
    }

    [Fact]
    public void SkyDocumentWellController_clamp_selection_prefers_selected_document()
    {
        var first = new SkyDocument("First");
        var second = new SkyDocument("Second");
        var controller = new SkyDocumentWellController
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { first, second },
        };

        Assert.Equal(1, controller.ClampSelectionIndex(0, second));
        Assert.Same(second, controller.ResolveSelection(-1, second));
    }

    [Fact]
    public void SkyDocumentWell_open_document_adds_and_selects()
    {
        var documents = new ObservableCollection<ISkyDocument>();
        var well = new SkyDocumentWell { ItemsSource = documents, Width = 400, Height = 240 };
        var doc = new SkyDocument("New", new TextBlock { Text = "Body" });

        well.OpenDocument(doc);

        Assert.Same(doc, well.SelectedDocument);
        Assert.Equal(0, well.SelectedIndex);
        Assert.Single(documents);
    }

    [Fact]
    public void SkyDocumentWell_close_document_removes_and_updates_selection()
    {
        var a = new SkyDocument("A");
        var b = new SkyDocument("B");
        var documents = new ObservableCollection<ISkyDocument> { a, b };
        var well = new SkyDocumentWell { ItemsSource = documents, SelectedDocument = a };

        Assert.True(well.TryCloseDocument(a));
        Assert.Same(b, well.SelectedDocument);
        Assert.Single(documents);
    }

    [Fact]
    public void SkyDocumentWell_close_can_be_cancelled()
    {
        var doc = new SkyDocument("Keep");
        var documents = new ObservableCollection<ISkyDocument> { doc };
        var well = new SkyDocumentWell { ItemsSource = documents };
        well.DocumentClosing += (_, e) => e.Cancel = true;

        Assert.False(well.TryCloseDocument(doc));
        Assert.Single(documents);
    }

    [Fact]
    public void SkyDocumentWell_respects_can_close_flag()
    {
        var doc = new SkyDocument("Pinned") { CanClose = false };
        var documents = new ObservableCollection<ISkyDocument> { doc };
        var well = new SkyDocumentWell { ItemsSource = documents };

        Assert.False(well.TryCloseDocument(doc));
        Assert.Single(documents);
    }

    [Fact]
    public void SkyDocumentWell_activate_document_updates_index()
    {
        var a = new SkyDocument("A");
        var b = new SkyDocument("B");
        var well = new SkyDocumentWell
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { a, b },
            SelectedDocument = a,
        };

        well.ActivateDocument(b);

        Assert.Equal(1, well.SelectedIndex);
        Assert.Same(b, well.SelectedDocument);
    }

    [Fact]
    public void SkyDocumentWell_move_document_raises_reordered_event()
    {
        var a = new SkyDocument("A");
        var b = new SkyDocument("B");
        var well = new SkyDocumentWell
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { a, b },
            SelectedDocument = a,
        };

        SkyDocumentReorderedEventArgs? args = null;
        well.DocumentReordered += (_, e) => args = e;
        well.MoveDocument(0, 1);

        Assert.NotNull(args);
        Assert.Same(a, args!.Document);
        Assert.Equal(0, args.OldIndex);
        Assert.Equal(1, args.NewIndex);
    }

    [Fact]
    public void SkyDocumentWell_document_closed_event_fires_after_remove()
    {
        var doc = new SkyDocument("Gone");
        var well = new SkyDocumentWell
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { doc },
        };

        ISkyDocument? closed = null;
        well.DocumentClosed += (_, e) => closed = e.Document;
        well.TryCloseDocument(doc);

        Assert.Same(doc, closed);
    }

    [Fact]
    public void SkyDocumentWell_close_all_stops_when_cancelled()
    {
        var a = new SkyDocument("A");
        var b = new SkyDocument("B");
        var documents = new ObservableCollection<ISkyDocument> { a, b };
        var well = new SkyDocumentWell { ItemsSource = documents };
        well.DocumentClosing += (_, e) =>
        {
            if (e.Document.Title == "B")
                e.Cancel = true;
        };

        well.CloseAllDocuments();

        Assert.Empty(documents.Where(d => d.Title == "A"));
        Assert.Single(documents);
        Assert.Equal("B", documents[0].Title);
    }

    [Fact]
    public void SkyDocumentWell_custom_document_model_via_interface()
    {
        var model = new TestDocument("Custom", "Payload");
        var well = new SkyDocumentWell
        {
            ItemsSource = new ObservableCollection<ISkyDocument> { model },
        };

        well.ActivateDocument(model);
        Assert.Same(model, well.SelectedDocument);
        Assert.Equal("Payload", model.Content);
    }

    private sealed class TestDocument : ISkyDocument
    {
        public TestDocument(string title, object? content)
        {
            Title = title;
            Content = content;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; set; }
        public bool IsDirty { get; set; }
        public object? Content { get; set; }
        public bool CanClose { get; set; } = true;

        public SkyDocumentShellPlacement ShellPlacement { get; set; } = SkyDocumentShellPlacement.Central;

        public SkyDocumentShellPlacement ShellDockBack { get; set; } = SkyDocumentShellPlacement.Central;

        public double ShellFloatLeft { get; set; } = double.NaN;

        public double ShellFloatTop { get; set; } = double.NaN;
    }
}
