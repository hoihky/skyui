using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SkyUI.Core.Theming;

namespace SkyUI.Controls;

internal static class CheckedListRowBuilder
{
    public static Control Build(CheckedListBox owner, CheckedListRowModel? model)
    {
        if (model is null)
            return new Border();

        var border = new Border
        {
            Padding = new Thickness(4, 2),
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(4),
        };

        border.AddHandler(InputElement.PointerPressedEvent, (_, e) =>
        {
            if (e.ClickCount >= 2 && owner.AllowInlineEdit)
            {
                owner.BeginInlineEdit(model);
                e.Handled = true;
                return;
            }

            if (CheckedListRowInput.IsInteractiveSource(e.Source, border))
                return;

            if (!owner.AllowReorder)
                owner.HandleRowPointerPressed(model, e);
        }, RoutingStrategies.Bubble);

        owner.DragReorderHandler.Attach(border, model);

        void SyncSelection()
        {
            border.Background = model.IsSelected
                ? owner.FindRowBrush(SkyTokenKeys.Brush.SelectedTint, new SolidColorBrush(Color.Parse("#331ED760")))
                : Brushes.Transparent;
        }

        model.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CheckedListRowModel.IsSelected))
                SyncSelection();
            if (args.PropertyName == nameof(CheckedListRowModel.IsLoadingChildren))
                owner.RequestRowVisualRefresh();
        };
        SyncSelection();

        var hasActions = owner.RowActionProvider?.GetActions(model.Item).Count > 0;
        var columns = owner.ShowCheckBoxes
            ? hasActions ? "Auto,Auto,Auto,*,Auto" : "Auto,Auto,Auto,*"
            : hasActions ? "Auto,Auto,*,Auto" : "Auto,Auto,*";

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(columns),
            MinHeight = 32,
        };

        var indent = new Border { Width = model.Depth * owner.Indent, Background = Brushes.Transparent };
        Grid.SetColumn(indent, 0);
        grid.Children.Add(indent);

        var expandHost = new Panel { Width = 26, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
        if (model.IsLoadingChildren)
        {
            var ring = new SkyProgressRing { Width = 18, Height = 18, IsIndeterminate = true };
            expandHost.Children.Add(ring);
        }
        else
        {
            var expand = new Button
            {
                Width = 26,
                Height = 26,
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = "›",
                FontSize = 14,
                IsVisible = model.HasChildren,
                Command = model.ToggleExpandCommand,
            };
            expand.Classes.Add("sky");
            expand.Classes.Add("sky-subtle");
            expand.Classes.Add("sky-tree-expander");
            expand.RenderTransform = new RotateTransform(model.IsExpanded ? 90 : 0);
            model.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CheckedListRowModel.IsExpanded))
                    expand.RenderTransform = new RotateTransform(model.IsExpanded ? 90 : 0);
            };
            expandHost.Children.Add(expand);
        }

        Grid.SetColumn(expandHost, 1);
        grid.Children.Add(expandHost);

        var contentColumn = 2;
        if (owner.ShowCheckBoxes)
        {
            var checkBox = new CheckBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            checkBox.Classes.Add("sky");
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(CheckedListRowModel.IsChecked))
            {
                Source = model,
                Mode = BindingMode.TwoWay,
            });
            Grid.SetColumn(checkBox, 2);
            grid.Children.Add(checkBox);
            contentColumn = 3;
        }

        if (owner.AllowInlineEdit && owner.EditableAdapter is not null)
        {
            var presenter = new ContentPresenter
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsVisible = !model.IsEditing,
            };
            presenter.Bind(ContentPresenter.ContentProperty, new Binding(nameof(CheckedListRowModel.Item)) { Source = model });
            presenter.Bind(ContentPresenter.ContentTemplateProperty, new Binding(nameof(CheckedListBox.ItemTemplate)) { Source = owner });

            var editor = new TextBox
            {
                Text = model.EditText,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsVisible = model.IsEditing,
            };
            editor.Classes.Add("sky");
            editor.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    owner.CommitInlineEdit(model, editor.Text ?? string.Empty);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    owner.CancelInlineEdit(model);
                    e.Handled = true;
                }
            };
            editor.LostFocus += (_, _) => owner.CommitInlineEdit(model, editor.Text ?? string.Empty);

            void SyncEditMode()
            {
                presenter.IsVisible = !model.IsEditing;
                editor.IsVisible = model.IsEditing;
                if (!model.IsEditing)
                    return;

                editor.Text = model.EditText;
                editor.Focus();
                editor.CaretIndex = editor.Text?.Length ?? 0;
            }

            model.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CheckedListRowModel.IsEditing))
                    SyncEditMode();
                else if (args.PropertyName == nameof(CheckedListRowModel.EditText) && model.IsEditing)
                    editor.Text = model.EditText;
            };

            var contentHost = new Panel();
            contentHost.Children.Add(presenter);
            contentHost.Children.Add(editor);
            Grid.SetColumn(contentHost, contentColumn);
            grid.Children.Add(contentHost);
            SyncEditMode();
        }
        else
        {
            var presenter = new ContentPresenter
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            presenter.Bind(ContentPresenter.ContentProperty, new Binding(nameof(CheckedListRowModel.Item)) { Source = model });
            presenter.Bind(ContentPresenter.ContentTemplateProperty, new Binding(nameof(CheckedListBox.ItemTemplate)) { Source = owner });
            Grid.SetColumn(presenter, contentColumn);
            grid.Children.Add(presenter);
        }

        if (hasActions)
        {
            var actionsButton = new Button
            {
                Content = "⋯",
                Width = 30,
                Height = 30,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            actionsButton.Classes.Add("sky");
            actionsButton.Classes.Add("sky-subtle");
            actionsButton.Click += (_, _) => owner.ShowRowActions(model, actionsButton);
            Grid.SetColumn(actionsButton, contentColumn + 1);
            grid.Children.Add(actionsButton);
        }

        border.Child = grid;
        return border;
    }
}
