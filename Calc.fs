module Xcap.Calc

let calcWeights totalRow totalCol (ts: (('a * 'b) * decimal) array) =
    let denom =
        ts
        |> Array.find (fun ((row, col), _) -> row = totalRow && col = totalCol)
        |> snd

    ts
    |> Array.map (fun (id, cap) -> (id, cap / denom))

/// <summary>
/// Returns the consistent as-of dates or an error (with the set of documents) if empty or inconsistent
/// </summary>
let getConsistentAsOf ls =
    let dates =
        ls
        |> Array.choose (fun (_, _, maybeDate) -> maybeDate)

    if Array.isEmpty dates then
        Error ls
    else
        let max = Array.max dates

        if max = Array.min dates then
            Ok max
        else
            Error ls