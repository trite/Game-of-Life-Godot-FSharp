module FsLib.Grid

type position = { x: int; y: int }

// Cell stuff, maybe break out later?
type cellStatus =
    | Alive
    | Dead

type cell =
    { status: cellStatus
      position: position }




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

    let addCell (cell: cell) (Cells(cells): Cells) : Cells =
        Cells(Map.add cell.position cell cells)

    let removeCell (cell: cell) (Cells(cells): Cells) : Cells =
        Cells(cells.Remove(cell.position))

    let getCellNextStatus (pos: position) (Cells(cells): Cells) : cellStatus =
        let neighbors =
            [ { x = pos.x - 1; y = pos.y - 1 }
              { x = pos.x - 1; y = pos.y }
              { x = pos.x - 1; y = pos.y + 1 }
              { x = pos.x; y = pos.y - 1 }
              { x = pos.x; y = pos.y + 1 }
              { x = pos.x + 1; y = pos.y - 1 }
              { x = pos.x + 1; y = pos.y }
              { x = pos.x + 1; y = pos.y + 1 } ]
            |> List.map (fun p -> Map.tryFind p cells)
            |> List.choose id
            |> List.filter (fun c -> c.status = Alive)
            |> List.length

        match neighbors with
        | 2 -> cells[pos].status
        | 3 -> Alive
        | _ -> Dead

    let getCellsNextStatus (Cells(cells): Cells) : Cells =
        Cells(
            cells
            |> Map.map (fun _ c ->
                { c with
                    status = getCellNextStatus c.position (Cells(cells)) })
        )

// let addCell (cell: cell) (cells: Cells) : Cells = cells.Add(cell.position, cell)
