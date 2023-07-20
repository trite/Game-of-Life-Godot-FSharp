module FsLib.World

open Godot
open System

let mutable cell: Sprite2D option = None

let mutable aliveColor: Color option = None

[<Export(PropertyHint.None, "blah")>]
let mutable deadColor: Color option = Some(new Color(32u))


let mutable vBarrierColor: Color option =
    Some(new Color(0.82745f, 0.47058f, 0.16470f, 1.0f))

let mutable godotCells: Map<Grid.position, Sprite2D> = Map.empty

let mutable cells: Grid.Cells = Grid.Cells.empty

let mutable zoom = 1.0f

let zoomStep = 0.1f

let cellSize = 32.0f

// let mutable running: bool = false

type runMode =
    | Running
    | SingleFrame
    | Paused

let mutable running: runMode = Paused

let NullableToOption (n: Nullable<_>) =
    if n.HasValue then Some n.Value else None

let vectorToPosition (pos: Vector2) : Grid.position =
    let newPos = pos.Snapped(new Vector2(cellSize, cellSize)) / cellSize

    { Grid.x = int newPos.X
      Grid.y = int newPos.Y }

let positionToVector (pos: Grid.position) : Vector2 =
    new Vector2(float32 pos.x, float32 pos.y) * cellSize

let reconcileGodotCells (this: Node2D) : unit =
    let statusToColor (behavior: Grid.cellBehavior) =
        function
        | Grid.Alive ->
            match behavior with
            | Grid.Normal -> aliveColor.Value
            | Grid.VBarrier -> vBarrierColor.Value
        | Grid.Dead -> deadColor.Value

    cells
    |> Grid.Cells.cells
    // TODO: Mutating after each change might be heavy
    //         Could we batch the changes and reduce the number of mutations?
    |> List.iter
        (fun
            { Grid.position = pos
              Grid.status = status
              Grid.behavior = behavior } ->
            godotCells <-
                godotCells.Change(
                    pos,
                    fun c ->
                        match c with
                        | Some(c) ->
                            c.Position <- positionToVector pos
                            c.Modulate <- statusToColor behavior status
                            Some(c)

                        | None ->
                            let newCell = cell.Value.Duplicate() :?> Sprite2D
                            newCell.Position <- positionToVector pos
                            newCell.Modulate <- statusToColor behavior status
                            this.AddChild(newCell)
                            newCell.Show()
                            Some(newCell)
                ))

let upsertCell
    (this: Node2D)
    (behavior: Grid.cellBehavior)
    (gridPosition: Vector2)
    : unit =
    let cell =
        Grid.makeCell Grid.Alive (gridPosition |> vectorToPosition) behavior

    cells <- Grid.Cells.upsertCell cell cells
    reconcileGodotCells this

let changeZoom (this: Node2D) (delta: float32) : unit =
    zoom <- Mathf.Clamp(zoom + delta, 0.1f, 8.0f)

    this.GetNode<Camera2D>("Camera").Zoom <- new Vector2(zoom, zoom)

let markCellDead (this: Node2D) (pos: Vector2) : unit =
    cells <- Grid.Cells.markDead (vectorToPosition pos) cells
    reconcileGodotCells this

let markCellVBarrier (this: Node2D) (pos: Vector2) : unit =
    // cells <- Grid.Cells.markDead (vectorToPosition pos) cells
    cells <-
        Grid.Cells.updateBehavior (vectorToPosition pos) Grid.VBarrier cells

    reconcileGodotCells this

let _ready (this: Node2D) : unit =
    cell <- Some(this.GetNode<Sprite2D>("Cell"))

    running <- Paused

    match cell with
    | None -> failwith "Could not find the `Cell` node!"
    | Some(c) ->
        c.Hide()
        aliveColor <- Some(c.Modulate)

let (|IsActionPressed|_|) (action: string) (event: InputEvent) =
    if event.IsActionPressed(action) then Some() else None

let _unhandledInput (this: Node2D) (event: InputEvent) : unit =
    match event with
    | :? InputEventMouseButton as e ->
        match e.ButtonIndex, e.IsPressed() with
        | MouseButton.Left, true ->
            this.GetGlobalMousePosition() |> upsertCell this Grid.Normal
        | MouseButton.Right, true ->
            this.GetGlobalMousePosition() |> markCellDead this
        | MouseButton.WheelDown, _ -> changeZoom this zoomStep
        | MouseButton.WheelUp, _ -> changeZoom this -zoomStep
        | _ -> ()
    | :? InputEventMouseMotion as e ->
        match e.ButtonMask with
        | MouseButtonMask.Left ->
            this.GetGlobalMousePosition() |> upsertCell this Grid.Normal
        | MouseButtonMask.Right ->
            this.GetGlobalMousePosition() |> markCellDead this
        | MouseButtonMask.Middle ->
            let mutable cameraNode = this.GetNode<Camera2D>("Camera")
            cameraNode.Offset <- cameraNode.Offset - e.Relative
        | _ -> ()
    | IsActionPressed "ui_accept" ->
        running <-
            match running with
            | Running -> Paused
            | Paused -> Running
            | SingleFrame -> Paused
    | IsActionPressed "run_step" ->
        running <-
            match running with
            | Running -> Paused
            | Paused -> SingleFrame
            | SingleFrame -> Paused
    | IsActionPressed "mark_vbarrier" ->
        match vBarrierColor with
        | Some(color) ->
            // TODO: There's plenty still here to take care of but I'm sleepy
            this.GetGlobalMousePosition() |> upsertCell this Grid.VBarrier
        // c.Modulate <- color
        // c.Show()
        | _ ->
            failwith
                "Could not find one or both of the cell/vBarrierColor nodes"
    | _ -> ()

let _runSimulationStep (this: Node2D) : unit =
    if running = Running || running = SingleFrame then
        // Need to get the list of updated cells and then add any new ones
        //   that spontaneously came into existence
        let (updatedCells, newCells) = cells |> Grid.Cells.getCellsNextStatus
        cells <- updatedCells

        newCells
        |> List.iter (fun c -> upsertCell this Grid.Normal (positionToVector c))

        reconcileGodotCells this

    if running = SingleFrame then
        running <- Paused

let _process (this: Node2D) (delta: double) : unit =
    let mutable debug = this.GetNode<Label>("Wall/DebugInfo")

    let pos = this.GetGlobalMousePosition()
    let gridPos = pos |> vectorToPosition

    debug.Text <-
        sprintf
            "MousePosition: %A
GridPosition: %A
Zoom: %f
Running: %A"
            pos
            gridPos
            zoom
            running

    _runSimulationStep this
