module FsLib.ShaderWorld

open Godot

let mutable running: bool = true

let mutable texture_updated: bool = false

// let mutable texture: ImageTexture option = None

let mutable outputViewport: Viewport option = None

let mutable simulationTexture: Sprite2D option = None

let updateTexture () : unit =
    outputViewport
    |> Option.iter (fun viewport ->

        let texture =
            ImageTexture.CreateFromImage(viewport.GetTexture().GetImage())

        simulationTexture <-
            simulationTexture
            |> Option.map (fun sprite ->
                sprite.Texture <- texture
                sprite)

    )

// simulationTexture <-
//     simulationTexture
//     |> Option.map (fun sprite ->
//         sprite.Texture <- texture
//         sprite))

// |> Option.iter (fun sprite ->
//     sprite.Texture <- texture.Value
//     texture_updated <- true))

let _process (delta: float) : unit =
    if running && not texture_updated then
        updateTexture ()

    if texture_updated then
        texture_updated <- false
