module Xcap.Requests

open System.Net.Http
open System.Net
open System.IO
open Xcap.Heuristics
open UglyToad.PdfPig

let client = new HttpClient()

let knownUrls =
    [ (("ACWI", "All Cap"), "https://www.msci.com/resources/factsheets/index_fact_sheet/msci-acwi-all-cap.pdf")
      (("ACWI", "IMI"), "https://www.msci.com/documents/10199/b93d88ef-632f-4bdb-9069-d7c5aecd9d6d")
      (("ACWI", "Standard"), "https://www.msci.com/resources/factsheets/index_fact_sheet/msci-acwi.pdf")
      (("ACWI", "Small Cap"), "https://www.msci.com/documents/10199/2cdf9672-e1b2-4197-a951-9605fce4772f")
      (("EM", "Standard"), "https://www.msci.com/documents/10199/c0db0a48-01f2-4ba9-ad01-226fd5678111")
      (("EM", "IMI"), "https://www.msci.com/documents/10199/97e25eb7-9bd0-4204-bea9-077095acf1d3")
      (("EM", "Small Cap"), "https://www.msci.com/documents/10199/4a397ecf-5cd5-4c49-a611-61db92de980c")
      (("DM", "Standard"), "https://www.msci.com/documents/10199/149ed7bc-316e-4b4c-8ea4-43fcb5bd6523")
      (("DM", "Small Cap"), "https://www.msci.com/documents/10199/a67b0d43-0289-4bce-8499-0c102eaa8399")
      (("DM", "IMI"), "https://www.msci.com/documents/10199/cc93af9f-9373-4f3b-87bb-1c523ca7431e")
      (("DM", "All Cap"), "https://www.msci.com/documents/10199/4479c942-4690-4f31-b0e0-7094f511b7a4") ]

let extractFromUrl (pdfUrl: string) =
    async {
        let! response = client.GetAsync(pdfUrl) |> Async.AwaitTask
        // Manual redirection needed as some URLs redirect https -> http (or http -> https -> http)
        use! stream =
            match response.StatusCode with
            | HttpStatusCode.Moved ->
                client.GetStreamAsync(response.Headers.Location)
                |> Async.AwaitTask
            | _ ->
                response.Content.ReadAsStreamAsync()
                |> Async.AwaitTask

        use ms = new MemoryStream()
        do! stream.CopyToAsync(ms) |> Async.AwaitTask
        ms.Seek(0L, SeekOrigin.Begin) |> ignore
        use pdf = PdfDocument.Open(ms, ParsingOptions())
        let pg = pdf.GetPage(2)
        return pg.Text
    }

let guessFromUrl idUrl =
    async {
        let id, url = idUrl
        let! soup = extractFromUrl url
        let cap, asOf = guess soup
        return (id, cap, asOf)
    }
