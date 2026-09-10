using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Controls;
using SkyUI.DataGrid;

namespace SkyUI.Demo.Views.Demos;

public partial class VirtualDataGridDemo : UserControl
{
    private readonly DemoSheetSource _source = new();

    public VirtualDataGridDemo()
    {
        InitializeComponent();
        Grid.Columns.Add(new SkyDataGridColumn { Header = "Index", Width = 120, BindingPath = "Index" });
        Grid.Columns.Add(new SkyDataGridColumn { Header = "Label", Width = 220, BindingPath = "Label" });
        Grid.Columns.Add(new SkyDataGridColumn { Header = "Value", Width = 100, BindingPath = "Value", IsReadOnly = false });
        Grid.DataSource = _source;
        Grid.Sorting += OnSorting;
        Grid.RowFormatting += OnRowFormatting;
        ExportCsvButton.Click += OnExportCsv;
        CopySelectionButton.Click += OnCopySelection;
        BumpButton.Click += OnBump;
        Grid.SelectedRowIndex = 0;
    }

    private void OnSorting(object? sender, SkyDataGridSortingEventArgs e)
    {
        StatusText.Text = $"Sort: {e.Column.Header} → {e.NewDirection}";
    }

    private void OnRowFormatting(object? sender, SkyDataGridRowFormattingEventArgs e)
    {
        if (e.RowIndex % 1000 == 0 && e.RowIndex >= 0)
            e.RowClasses.Add("sky-grid-milestone");
    }

    private async void OnCopySelection(object? sender, RoutedEventArgs e)
    {
        await Grid.CopySelectionToClipboardAsync().ConfigureAwait(true);
        var pasted = await SkyClipboard.GetTextAsync(this).ConfigureAwait(true);
        StatusText.Text = pasted is null ? "Clipboard unavailable." : $"Copied TSV ({pasted.Length} chars)";
    }

    private async void OnExportCsv(object? sender, RoutedEventArgs e)
    {
        var path = Path.Combine(Path.GetTempPath(), "skyui-grid-export.csv");
        await using (var fs = File.Create(path))
            await Grid.ExportToCsvAsync(fs, 0, 10_000).ConfigureAwait(true);
        StatusText.Text = $"Wrote {path}";
    }

    private void OnBump(object? sender, RoutedEventArgs e)
    {
        Task.Run(() =>
        {
            _source.Bump();
            Grid.InvalidateStructure();
        });
    }

    private sealed class DemoSheetSource : IVirtualGridDataSource
    {
        private readonly List<DemoRow> _rows;

        public DemoSheetSource()
        {
            const int n = 50_000;
            _rows = new List<DemoRow>(n);
            for (var i = 0; i < n; i++)
                _rows.Add(new DemoRow { Index = i, Label = $"Row {i.ToString("N0")}", Value = i % 997 });
        }

        public long RowCount => _rows.Count;

        public object? GetRow(long index) =>
            index >= 0 && index < _rows.Count ? _rows[(int)index] : null;

        public event EventHandler? StructureChanged;

        public void ApplySort(SkyDataGridColumn? column, SkyDataGridSortDirection direction)
        {
            if (direction == SkyDataGridSortDirection.None)
            {
                _rows.Sort((a, b) => a.Index.CompareTo(b.Index));
            }
            else
            {
                var path = column?.BindingPath;
                if (string.IsNullOrEmpty(path))
                    return;
                var prop = typeof(DemoRow).GetProperty(path, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop is null)
                    return;
                _rows.Sort((a, b) =>
                {
                    var va = prop.GetValue(a);
                    var vb = prop.GetValue(b);
                    var cmp = Comparer.Default.Compare(va, vb);
                    return direction == SkyDataGridSortDirection.Ascending ? cmp : -cmp;
                });
            }

            StructureChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Bump()
        {
            var i = _rows.Count;
            _rows.Add(new DemoRow { Index = i, Label = $"Row {i.ToString("N0")}", Value = i % 997 });
            StructureChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class DemoRow : INotifyPropertyChanged
    {
        private long _index;
        private string _label = "";
        private int _value;

        public long Index
        {
            get => _index;
            set
            {
                if (_index == value)
                    return;
                _index = value;
                OnPropertyChanged();
            }
        }

        public string Label
        {
            get => _label;
            set
            {
                if (_label == value)
                    return;
                _label = value;
                OnPropertyChanged();
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;
                _value = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
