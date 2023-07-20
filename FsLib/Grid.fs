module FsLib.Grid

type position = { x: int; y: int }



// Cell stuff, maybe break out later?

type cellStatus =
    | Alive
    | Dead

type cellBehavior =
    | Normal
    | VBarrier

type cell =
    { status: cellStatus
      position: position
      behavior: cellBehavior }

let makeCell
    (status: cellStatus)
    (position: position)
    (behavior: cellBehavior)
    : cell =
    { status = status
      position = position
      behavior = behavior }



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

    let markDead (pos: position) (Cells(cells): Cells) : Cells =
        Cells(
            cells.Change(
                pos,
                function
                | Some(x) -> Some({ x with status = Dead })
                | None ->
                    Some(
                        { status = Dead
                          position = pos
                          behavior = Normal }
                    )
            )
        )

    let updateBehavior
        (pos: position)
        (behavior: cellBehavior)
        (Cells(cells): Cells)
        =
        // { cell with behavior = behavior }
        Cells(
            cells.Change(
                pos,
                function
                | Some(x) -> Some({ x with behavior = behavior })
                | None -> failwith "Shouldn't call this on a non-existent cell"
            )
        )

    let getCellNextStatusNormal
        (pos: position)
        (cells: _CellsType)
        : cellStatus * position list =
        // Returning the list of neighbors at the end gives us a 2nd pass
        //   list to check for spontaneous generation of new cells around
        //   borders of currently alive cells
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

    let getCellNextStatusVBarrier
        (pos: position)
        (cells: _CellsType)
        : cellStatus * position list =

        // Break things up into 3 groups for barrier calculations:
        let abovePositions =
            [ { x = pos.x - 1; y = pos.y - 1 }
              { x = pos.x - 1; y = pos.y }
              { x = pos.x - 1; y = pos.y + 1 } ]

        let sidePositions =
            [ { x = pos.x; y = pos.y - 1 }; { x = pos.x; y = pos.y + 1 } ]

        let belowPositions =
            [ { x = pos.x + 1; y = pos.y - 1 }
              { x = pos.x + 1; y = pos.y }
              { x = pos.x + 1; y = pos.y + 1 } ]

        let positions = [ abovePositions; sidePositions; belowPositions ]

        positions
        |> List.map (
            List.map (fun p -> Map.tryFind p cells)
            >> List.choose id
            >> List.filter (fun c -> c.status = Alive)
            >> List.length
        )
        |> (function
        | [ above; sides; below ] -> (above, sides, below)
        | _ -> failwith "Unexpected number of lists in positions")
        |> (function
        | (_, _, 3) -> Dead
        | (2, _, 1) -> Dead
        | (a, b, c) when a + b + c = 2 ->
            if cells.ContainsKey(pos) then cells[pos].status else Dead
        | (a, b, c) when a + b + c = 3 -> Alive
        | _ -> Dead)
        |> (fun x -> (x, positions |> List.concat))

    let getCellNextStatus (pos: position) (cells: _CellsType) =
        match cells[pos].behavior with
        | Normal -> getCellNextStatusNormal pos cells
        | VBarrier -> getCellNextStatusVBarrier pos cells

    let getCellsNextStatus (Cells(cells): Cells) =
        let aliveCells = cells |> Map.filter (fun _ c -> c.status = Alive)

        // Get updated cells and 2nd pass list
        let (updatedCells, extrasToCheck) =
            aliveCells
            |> Map.toList
            |> List.map (fun (p, c) ->
                let (status, positions) =
                    getCellNextStatus c.position aliveCells

                ((p, { c with status = status }), positions))
            |> List.unzip

        // Run through 2nd pass list,
        //   filter out already checked,
        //   then check for spontaneous generation
        let spontaneousCells =
            extrasToCheck
            |> List.concat
            |> List.distinct
            |> List.filter (fun p -> not (Map.containsKey p aliveCells))
            |> List.map (fun p ->
                let (status, _) = getCellNextStatus p aliveCells

                (p,
                 { status = status
                   position = p
                   behavior = Normal }))
            |> List.filter (fun (_, c) -> c.status = Alive)

        // Combine the 2 lists and return
        (updatedCells @ spontaneousCells)
        |> Map.ofList
        |> Cells
        |> (fun x -> (x, spontaneousCells |> List.map fst))

    let positions (Cells(cells): Cells) : position list =
        cells |> Map.toList |> List.map fst

    let cells (Cells(cells): Cells) : cell list =
        cells |> Map.toList |> List.map snd
