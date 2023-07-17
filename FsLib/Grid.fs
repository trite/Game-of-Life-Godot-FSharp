module FsLib.Grid

type position = { x: int; y: int }



// Cell stuff, maybe break out later?

type cellStatus =
    | Alive
    | Dead

type cell =
    { status: cellStatus
      position: position }

let makeCell (status: cellStatus) (position: position) : cell =
    { status = status; position = position }



// Grid stuff

type _CellsType = Map<position, cell>
type Cells = private Cells of _CellsType

module Cells =
    let empty = Cells Map.empty

    let private map
        (f: _CellsType -> _CellsType)
        (Cells(cells): Cells)
        : Cells =
        Cells(f cells)

    let upsertCell (cell: cell) (cells: Cells) : Cells =
        cells
        |> map (fun cells ->
            cells.Change(
                cell.position,
                function
                | Some(x) -> Some({ x with status = cell.status })
                | None -> Some(cell)
            ))

    let markDead (pos: position) (cells: Cells) : Cells =
        cells
        |> map (fun cells ->
            cells.Change(
                pos,
                function
                | Some(x) -> Some({ x with status = Dead })
                | None -> Some({ status = Dead; position = pos })
            ))

    let getCellNextStatus (pos: position) (cells) =
        let positions =
            [ { x = pos.x - 1; y = pos.y - 1 }
              { x = pos.x - 1; y = pos.y }
              { x = pos.x - 1; y = pos.y + 1 }
              { x = pos.x; y = pos.y - 1 }
              { x = pos.x; y = pos.y + 1 }
              { x = pos.x + 1; y = pos.y - 1 }
              { x = pos.x + 1; y = pos.y }
              { x = pos.x + 1; y = pos.y + 1 } ]

        positions
        |> List.map (fun p -> Map.tryFind p cells)
        |> List.choose id
        |> List.filter (fun c -> c.status = Alive)
        |> List.length
        |> (function
        | 2 -> if cells.ContainsKey(pos) then cells[pos].status else Dead
        | 3 -> Alive
        | _ -> Dead)
        |> (fun x -> (x, positions))

    let getCellsNextStatus (Cells(cells): Cells) =
        let aliveCells = cells |> Map.filter (fun _ c -> c.status = Alive)

        let (updatedCells, extrasToCheck) =
            aliveCells
            |> Map.toList
            |> List.map (fun (p, c) ->
                let (status, positions) =
                    getCellNextStatus c.position aliveCells

                ((p, { c with status = status }), positions))
            |> List.unzip

        let extras =
            extrasToCheck
            |> List.concat
            |> List.distinct
            |> List.filter (fun p -> not (Map.containsKey p aliveCells))
            |> List.map (fun p ->
                let (status, _) = getCellNextStatus p aliveCells
                (p, { status = status; position = p }))
            |> List.filter (fun (_, c) -> c.status = Alive)

        (updatedCells @ extras)
        |> Map.ofList
        |> Cells
        |> (fun x -> (x, extras |> List.map fst))

    let positions (Cells(cells): Cells) : position list =
        cells |> Map.toList |> List.map fst

    let cells (Cells(cells): Cells) : cell list =
        cells |> Map.toList |> List.map snd
