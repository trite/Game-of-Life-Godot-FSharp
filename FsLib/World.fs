namespace FsLib

open Godot



module World =
    let cell: Sprite2D option = None

    let _ready: unit -> unit = fun () -> GD.Print("Hello from FSharp!!!!!")
