module Xcap.Heuristics

open System
open System.Globalization

let guess (soup: string) =
    // expected text extraction, starting from page 2
    // APR 30, 2021...
    // Mkt Cap ( USD Millions)...
    // Index[]123,456,789.00 ...
    let rowText = "Index"
    let headerText = "Mkt Cap ( USD Millions)"

    let headerIdx =
        soup.IndexOf headerText + headerText.Length

    let capIdx =
        (soup.IndexOf(rowText, headerIdx))
        + rowText.Length

    let capEndIdx = (soup.IndexOf(" ", capIdx)) - 1
    let textual = soup.[capIdx..capEndIdx]
    let mutable capResult = 0.0M
    let mutable dateResult = DateTime()
    let textualDate = soup.[0..11]

    let amt =
        if
            Decimal.TryParse
                (
                    textual,
                    NumberStyles.AllowTrailingWhite
                    ||| NumberStyles.AllowLeadingWhite
                    ||| NumberStyles.AllowDecimalPoint
                    ||| NumberStyles.AllowThousands,
                    NumberFormatInfo.InvariantInfo,
                    &capResult
                )
        then
            Some capResult
        else
            None

    let date =
        if
            DateTime.TryParseExact
                (
                    textualDate,
                    "MMM dd, yyyy",
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.AllowLeadingWhite
                    ||| DateTimeStyles.AllowTrailingWhite
                    ||| DateTimeStyles.AllowWhiteSpaces,
                    &dateResult
                )
        then
            Some dateResult
        else
            None

    (amt, date)
