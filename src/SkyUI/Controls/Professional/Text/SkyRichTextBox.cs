using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace SkyUI.Controls.Professional;

/// <summary>Multiline text editor with lightweight markdown-style formatting helpers for MVVM apps.</summary>
public class SkyRichTextBox : TemplatedControl
{
    public const string EditorPartName = "PART_Editor";
    public const string ToolbarPartName = "PART_Toolbar";
    public const string PreviewPartName = "PART_Preview";

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SkyRichTextBox, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> ShowToolbarProperty =
        AvaloniaProperty.Register<SkyRichTextBox, bool>(nameof(ShowToolbar), true);

    public static readonly StyledProperty<bool> ShowLivePreviewProperty =
        AvaloniaProperty.Register<SkyRichTextBox, bool>(nameof(ShowLivePreview), true);

    private TextBox? editor;
    private Panel? toolbar;
    private TextBlock? preview;
    private bool syncingText;

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool ShowToolbar
    {
        get => GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    public bool ShowLivePreview
    {
        get => GetValue(ShowLivePreviewProperty);
        set => SetValue(ShowLivePreviewProperty, value);
    }

    static SkyRichTextBox()
    {
        TextProperty.Changed.AddClassHandler<SkyRichTextBox>((b, e) => b.OnTextPropertyChanged(e.NewValue as string));
        ShowToolbarProperty.Changed.AddClassHandler<SkyRichTextBox>((b, _) => b.RefreshToolbar());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        editor = e.NameScope.Find<TextBox>(EditorPartName);
        toolbar = e.NameScope.Find<Panel>(ToolbarPartName);
        preview = e.NameScope.Find<TextBlock>(PreviewPartName);
        RefreshToolbar();
        RefreshPreview();

        if (editor is null)
            return;

        editor.Text = Text ?? "";
        editor.TextChanged -= OnEditorTextChanged;
        editor.TextChanged += OnEditorTextChanged;
        editor.LostFocus -= OnEditorLostFocus;
        editor.LostFocus += OnEditorLostFocus;
    }

    private void OnEditorLostFocus(object? sender, EventArgs e) => SyncTextFromEditor();

    private void RefreshToolbar()
    {
        if (toolbar is null)
            return;
        toolbar.Children.Clear();
        if (!ShowToolbar)
            return;
        toolbar.Children.Add(CreateFormatButton("Bold", SkyRichTextFormat.Bold));
        toolbar.Children.Add(CreateFormatButton("Italic", SkyRichTextFormat.Italic));
        toolbar.Children.Add(CreateFormatButton("Underline", SkyRichTextFormat.Underline));
    }

    private void OnTextPropertyChanged(string? value)
    {
        RefreshPreview();
        if (syncingText || editor is null)
            return;
        var next = value ?? "";
        if (editor.Text != next)
            editor.Text = next;
    }

    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        SyncTextFromEditor();
        RefreshPreview();
    }

    private void SyncTextFromEditor()
    {
        if (editor is null)
            return;
        syncingText = true;
        SetCurrentValue(TextProperty, editor.Text);
        syncingText = false;
    }

    private Button CreateFormatButton(string label, SkyRichTextFormat format)
    {
        var button = new Button { Classes = { "sky" }, Content = label, MinWidth = 64 };
        button.Click += (_, _) =>
        {
            editor?.Focus();
            ApplyFormat(format);
        };
        return button;
    }

    public void ApplyFormat(SkyRichTextFormat format)
    {
        if (editor is null)
            return;
        var start = Math.Max(0, editor.SelectionStart);
        var end = Math.Max(start, editor.SelectionEnd);
        if (end <= start)
            return;
        var text = editor.Text ?? "";
        var selection = text[start..end];
        var wrapped = format switch
        {
            SkyRichTextFormat.Bold => $"**{selection}**",
            SkyRichTextFormat.Italic => $"*{selection}*",
            SkyRichTextFormat.Underline => $"_{selection}_",
            _ => selection,
        };
        editor.Text = text[..start] + wrapped + text[end..];
        editor.SelectionStart = start;
        editor.SelectionEnd = start + wrapped.Length;
        SyncTextFromEditor();
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        if (preview is null || !ShowLivePreview)
            return;
        SkyRichTextMarkdown.ApplyPreview(preview, Text);
    }
}
