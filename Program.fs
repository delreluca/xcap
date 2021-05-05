open System.Linq
open Xcap.Requests
open Xcap.Calc
open ConsoleTables

let guesses =
    knownUrls
    |> List.map guessFromUrl
    |> Async.Parallel

let table ls =
    let columnHeaders1 =
        ls |> Array.map (fst >> snd) |> Array.distinct

    let columnHeaders =
        "Region" :: (columnHeaders1 |> Array.toList)

    let groupToRow (key, items: (('a * string) * decimal) array) =
        let dict =
            items.ToDictionary((fun ((_, id2), _) -> id2), (fun (_, w) -> w))

        key
        :: (columnHeaders1
            |> Array.map
                (fun col ->
                    if dict.ContainsKey(col) then
                        sprintf "%2.2f%%" (dict.[col] * 100m) :> obj
                    else
                        "?" :> obj)
            |> Array.toList)

    let rows =
        ls
        |> Array.groupBy (fst >> fst)
        |> Array.map groupToRow
        |> Array.map List.toArray

    let tblSeed =
        ConsoleTable(columnHeaders |> List.toArray)
            .Configure(fun o ->
                o.EnableCount <- false
                o.NumberAlignment <- Alignment.Right)

    let tbl =
        rows
        |> Array.fold (fun (accum: ConsoleTable) cs -> accum.AddRow(cs)) tblSeed

    tbl.ToString()

[<EntryPoint>]
let main args =
    let data = guesses |> Async.RunSynchronously

    let totalRow, totalCol =
        match args with
        | [| totalRow; totalCol |] -> (totalRow, totalCol)
        | _ -> ("ACWI", "IMI")

    match getConsistentAsOf data with
    | Some asOf ->
        printfn "Data is as of %s" (asOf.ToString("yyyy-MM-dd"))
        printfn "100%% = %s %s" totalRow totalCol

        let weights =
            data
            |> Array.choose (fun (id, maybeCap, _) -> maybeCap |> Option.map (fun cap -> (id, cap)))
            |> calcWeights totalRow totalCol

        printfn "%s" (table weights)
        0
    | None ->
        printfn "Either you were so lucky to get different as-of dates or no date could be parsed."
        -1
