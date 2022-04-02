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

open System
open System.Globalization
[<EntryPoint>]
let main args =
    let data = guesses |> Async.RunSynchronously

    let totalRow, totalCol =
        match args with
        | [| totalRow; totalCol |] -> (totalRow, totalCol)
        | _ -> ("ACWI", "IMI")

    match getConsistentAsOf data with
    | Ok asOf ->
        printfn "Data is as of %s" (asOf.ToString("yyyy-MM-dd"))
        printfn "100%% = %s %s" totalRow totalCol

        let weights =
            data
            |> Array.choose (fun (id, maybeCap, _) -> maybeCap |> Option.map (fun cap -> (id, cap)))
            |> calcWeights totalRow totalCol

        printfn "%s" (table weights)
        0
    | Error ls ->
        printfn "Either the fact sheets have inconsistent as-of dates (can happen between months) or no date could be parsed."
        let printErrorLn ((region, cap), (marketValue: decimal option), (asOf:DateTime option)) =
            let valFormatted = marketValue |> Option.map (fun d -> d.ToString(NumberFormatInfo.InvariantInfo)) |> Option.defaultValue "?"
            let datFormatted = asOf |> Option.map (fun d -> d.ToString("yyyy-MM-dd")) |> Option.defaultValue "?"
            printfn $"\tFact sheet for {region,-5} {cap,-10} has market value {valFormatted,15} USD and is as of {datFormatted,11}"

        ls |> Array.iter printErrorLn
        -1
