using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FootballManager.Domain.Finances;

/// <summary>
/// The currency the game's screens show, learnt from one amount the game sends both ways: as its text in full, e.g.
/// "R$3.233.473", and in its own unit, pounds sterling, e.g. 465184. It writes other amounts the same way.
/// </summary>
public sealed partial class DisplayCurrency
{
    /// <summary>The game's own unit, for when no amount shows the screens' currency.</summary>
    public static readonly DisplayCurrency Pounds = new("£", ',', 1m);

    private readonly string _symbol;
    private readonly char _separator;

    private DisplayCurrency(string symbol, char separator, decimal rate)
    {
        _symbol = symbol;
        _separator = separator;
        Rate = rate;
    }

    /// <summary>Units of this currency per pound.</summary>
    public decimal Rate { get; }

    /// <summary>
    /// The currency of a text written in full ("R$3.233.473"), given the same amount in pounds; null for an abbreviated
    /// text ("R$3.5 MI"), which hides the digits a rate needs, or a zero amount.
    /// </summary>
    public static DisplayCurrency? From(string? text, long pounds)
    {
        if (text is null || pounds <= 0)
        {
            return null;
        }

        var match = FullAmount().Match(text.Trim());

        if (!match.Success)
        {
            return null;
        }

        var amount = decimal.Parse(match.Groups["digits"].Value.Where(char.IsDigit).ToArray(), CultureInfo.InvariantCulture);

        if (amount == 0)
        {
            return null;
        }

        var separator = match.Groups["separator"].Success ? match.Groups["separator"].Value[0] : '.';
        return new DisplayCurrency(match.Groups["symbol"].Value, separator, amount / pounds);
    }

    /// <summary>An amount in pounds written in this currency, to the nearest unit, e.g. "-R$54.842.512".</summary>
    public string Format(long pounds)
    {
        var amount = (long)Math.Round(pounds * Rate, MidpointRounding.AwayFromZero);
        var digits = Math.Abs(amount).ToString(CultureInfo.InvariantCulture);
        var text = new StringBuilder();

        for (var i = 0; i < digits.Length; i++)
        {
            if (i > 0 && (digits.Length - i) % 3 == 0)
            {
                text.Append(_separator);
            }

            text.Append(digits[i]);
        }

        return $"{(amount < 0 ? "-" : string.Empty)}{_symbol}{text}";
    }

    // A symbol, then the whole amount in groups of three digits and nothing after it.
    [GeneratedRegex(@"^(?<symbol>[^\d\s-]+)\s?(?<digits>\d{1,3}(?:(?<separator>[.,   ])\d{3})*)$")]
    private static partial Regex FullAmount();
}
