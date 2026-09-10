namespace SkyUI.Controls;

/// <summary>Mask parsing and formatting for <see cref="SkyMaskedTextBox"/>.</summary>
public static class SkyInputMask
{
    public const char DigitPlaceholder = '9';
    public const char LetterPlaceholder = 'a';
    public const char AnyPlaceholder = '*';

    public static string ResolveMask(SkyInputMaskKind kind, string? customMask) => kind switch
    {
        SkyInputMaskKind.Phone => "(999) 999-9999",
        SkyInputMaskKind.CreditCard => "9999 9999 9999 9999",
        _ => customMask ?? string.Empty,
    };

    public static string Format(string mask, string rawValue)
    {
        if (string.IsNullOrEmpty(mask))
            return rawValue;

        var digits = rawValue ?? string.Empty;
        var index = 0;
        var result = new System.Text.StringBuilder(mask.Length);

        foreach (var maskChar in mask)
        {
            if (IsPlaceholder(maskChar))
            {
                while (index < digits.Length && !MatchesPlaceholder(maskChar, digits[index]))
                    index++;

                if (index >= digits.Length)
                    break;

                result.Append(digits[index++]);
            }
            else
            {
                result.Append(maskChar);
            }
        }

        return result.ToString();
    }

    public static string ExtractRaw(string mask, string? displayText)
    {
        if (string.IsNullOrEmpty(mask) || string.IsNullOrEmpty(displayText))
            return string.Empty;

        var raw = new System.Text.StringBuilder(displayText.Length);
        var textIndex = 0;

        foreach (var maskChar in mask)
        {
            if (textIndex >= displayText.Length)
                break;

            if (IsPlaceholder(maskChar))
            {
                var inputChar = displayText[textIndex++];
                if (MatchesPlaceholder(maskChar, inputChar))
                    raw.Append(inputChar);
            }
            else if (displayText[textIndex] == maskChar)
            {
                textIndex++;
            }
        }

        return raw.ToString();
    }

    public static bool IsComplete(string mask, string rawValue)
    {
        if (string.IsNullOrEmpty(mask))
            return true;

        var required = mask.Count(IsPlaceholder);
        return rawValue.Length >= required;
    }

    public static bool IsPlaceholder(char maskChar) =>
        maskChar is DigitPlaceholder or LetterPlaceholder or AnyPlaceholder;

    public static bool MatchesPlaceholder(char placeholder, char input) => placeholder switch
    {
        DigitPlaceholder => char.IsDigit(input),
        LetterPlaceholder => char.IsLetter(input),
        AnyPlaceholder => !char.IsWhiteSpace(input),
        _ => input == placeholder,
    };
}
