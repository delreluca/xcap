module Xcap.Calc

let calcWeights totalRow totalCol (ts: (('a * 'b) * decimal) array) =
    let denom =
        ts
        |> Array.find (fun ((row, col), _) -> row = totalRow && col = totalCol)
        |> snd

    ts
    |> Array.map (fun (id, cap) -> (id, cap / denom))

let getConsistentAsOf ls =
    let dates =
        ls
        |> Array.choose (fun (_, _, maybeDate) -> maybeDate)

    if Array.isEmpty dates then
        None
    else
        let max = Array.max dates

        if max = Array.min dates then
            Some max
        else
            None
