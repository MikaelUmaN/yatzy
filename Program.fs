open System

open State
open Game
open Player
open TerminalYatzy

[<EntryPoint>]
let main argv =
    let rec game (yatzy: IYatzy) appState gameState =
        match appState with
        | PlayerSetup ->
            let players = yatzy.DeterminePlayers
            
            let initialStates = 
                players 
                |> List.map (fun p -> p.Name, initPlayerState) 
                |> Map.ofList
            
            game yatzy Playing (Game (players, initialStates))
        | Playing ->
            match gameState with
            | Game (players, playerStates) ->
                printfn $"{hLine}"
                printfn $"Starting new round"
                printfn $"{hLine}"
                printfn ""

                let newPlayerStates =
                    players 
                    |> List.map (fun p -> (p.Name, (yatzy.PlayerTurn p playerStates.[p.Name])))
                    |> Map.ofList
                let newGameState = Game(players, newPlayerStates)
                
                // Decide if the game is over or not.
                let gameFinished =
                    newPlayerStates
                    |> Map.forall (fun _ v -> 
                        match v with
                        | PlayerState (remainingScoreTypes, _) -> remainingScoreTypes.IsEmpty)
                if gameFinished then
                    game yatzy GameFinished newGameState
                else
                    game yatzy Playing newGameState
            | _ -> failwith "Inconsistent state"
        | GameFinished -> yatzy.GameFinished gameState

    printfn $"{hLine}"
    printfn "Welcome to yatzy!"
    printfn $"{hLine}"

    let yatzyImpl = makeTerminalYatzyImpl
    let resultingGameState = game yatzyImpl PlayerSetup (Game(List.empty, Map.empty))

    // Calculate final score for each player.
    let sortedPlayerScores =
        match resultingGameState with
        | Game (_, playerStates) ->
            playerStates
            |> Map.map (fun name playerState -> calculateScore playerState)
            |> Map.toList
            |> List.sortByDescending snd
        | _ -> failwith "Inconsistent state"

    // Display results.
    printResult sortedPlayerScores

    0 // return an integer exit code