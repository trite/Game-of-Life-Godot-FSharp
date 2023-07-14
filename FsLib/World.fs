namespace FsLib

open Godot
open System




module World =
    let mutable cell: Sprite2D option = None

    let mutable aliveColor: Color option = None

    let mutable cells: Collections.Dictionary<Vector2, Sprite2D> =
        Collections.Dictionary<Vector2, Sprite2D>()

    let mutable zoom = 1.0f

    let zoomStep = 0.1f

    let cellSize = 32.0f

    // let mutable thisNode: Node2D option = None


    let NullableToOption (n: Nullable<_>) =
        if n.HasValue then Some n.Value else None

    let getGridPosition (pos: Vector2) : Vector2 =
        pos.Snapped(new Vector2(cellSize, cellSize)) / cellSize

    let addNewCell (this: Node2D) (gridPosition: Vector2) : unit =
        match cell with
        | None -> failwith "Could not find the `Cell` node!"
        | Some(c) ->

            if (cells.ContainsKey(gridPosition)) then
                failwith "Cell already exists at this position!"
            else

                let mutable newCell = c.Duplicate() :?> Sprite2D
                newCell.Position <- gridPosition * cellSize
                this.AddChild(newCell)
                newCell.Show()
                cells.Add(gridPosition, newCell)

    let changeZoom (this: Node2D) (delta: float32) : unit =
        zoom <- Mathf.Clamp(zoom + delta, 0.1f, 8.0f)

        this.GetNode<Camera2D>("Camera").Zoom <- new Vector2(zoom, zoom)

    let removeCell (this: Node2D) (pos: Vector2) : unit =
        let gridPosition = pos |> getGridPosition

        if cells.ContainsKey(gridPosition) then
            cells[gridPosition].QueueFree()
            cells.Remove(gridPosition) |> ignore
        else
            failwith "No cell exists at this position!"

    let _ready (this: Node2D) : unit =
        cell <- Some(this.GetNode<Sprite2D>("Cell"))

        match cell with
        | None -> failwith "Could not find the `Cell` node!"
        | Some(c) ->
            c.Hide()
            aliveColor <- Some(c.Modulate)

    let (|IsActionPressed|_|) (action: string) (event: InputEvent) =
        if event.IsActionPressed(action) then Some() else None

    let placeCell (this: Node2D) (pos: Vector2) =
        pos |> getGridPosition |> addNewCell this

    let _unhandledInput (this: Node2D) (event: InputEvent) : unit =
        match event with
        | :? InputEventMouseButton as e ->
            match e.ButtonIndex, e.Pressed with
            | MouseButton.Left, true -> this.GetGlobalMousePosition() |> placeCell this
            | MouseButton.Right, true -> this.GetGlobalMousePosition() |> removeCell this
            | MouseButton.WheelDown, _ -> changeZoom this zoomStep
            | MouseButton.WheelUp, _ -> changeZoom this -zoomStep
            | _ -> ()
        // | IsActionPressed "ui_accept" ->
        | _ -> ()

    let _process (this: Node2D) (delta: double) : unit =

        let mutable debug = this.GetNode<Label>("Wall/DebugInfo")

        let pos = this.GetGlobalMousePosition()
        let gridPos = pos |> getGridPosition

        debug.Text <-
            sprintf
                "MousePosition: %A
GridPosition: %A
Zoom: %f"
                pos
                gridPos
                zoom
