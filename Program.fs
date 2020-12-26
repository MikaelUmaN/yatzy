open System
open State
open Game

[<EntryPoint>]
let main argv =
    printfn $"{hLine}"
    printfn "Welcome to yatzy!"
    printfn $"{hLine}"

    let rec game appState gameState =
        match appState with
        | NrPlayers ->
            printf "Enter number of players: "
            let nPlayers = Console.ReadLine() |> int       
            printfn $"Starting a yatzy game with {nPlayers} players!"
            printfn "" // Prettier output
            game PlayerNames (Game (nPlayers, [], Map.empty))
        | PlayerNames ->
            match gameState with
            | Game (nPlayers, _, _) ->
                let playerNames = 
                    [for i in 1..nPlayers ->  
                        printf $"Enter name of player {i}: "
                        Console.ReadLine()]
                printfn "" // Prettier output
                
                let initialStates = 
                    playerNames 
                    |> List.map (fun p -> p, initPlayerState) 
                    |> Map.ofList
                game Playing (Game (nPlayers, playerNames, initialStates))
            | _ -> failwith "Inconsistent state"
        | Playing ->
            let playerTurn player playerState =
                printfn $"{hLine}"
                printfn $"Player: {player}"
                printfn $"{hLine}"

                let availableScoreTypes = 
                    match playerState with
                    | PlayerState (remainingScoreTypes, _) -> remainingStr remainingScoreTypes
                printfn ""
                printfn $"{availableScoreTypes}"
                printfn ""

                let rec diceThrow nRemaining (dice: list<DiceSide>) =
                    let nThrowDice = ndice - List.length dice
                    let newdice = castDice nThrowDice
                    let currentdice = List.concat [dice; newdice]
                    printfn "" // Prettier output
                    printfn $"Throw: {diceStr newdice}"
                    if nRemaining = 0 then
                        printfn $"Final dice: {diceStr currentdice}"
                        printfn "" // Prettier output
                        currentdice
                    else
                        printfn $"Current dice: {diceStr currentdice}"
                        printfn "" // Prettier output

                        let rec keepdiceChoice currentdice =
                            printf "Write values of dice to keep: "
                            let keepdice =
                                try
                                    List.map (int >> enum<DiceSide>) (Console.ReadLine().Split(' ') 
                                    |> Array.toList)
                                with
                                | _ -> []
                            let hasKeepdice =
                                keepdice
                                |> List.groupBy id
                                |> List.forall (fun (d, ds) -> 
                                    let groupedCurrentdice =
                                        currentdice 
                                        |> List.groupBy id
                                        |> Map.ofList
                                    groupedCurrentdice.ContainsKey d && groupedCurrentdice.[d].Length >= ds.Length)
                            if not hasKeepdice then
                                printfn "" // Prettier output
                                printfn "You do not have those dice!"
                                printfn "" // Prettier output
                                keepdiceChoice currentdice
                            else
                                keepdice
                        let chosendice = keepdiceChoice currentdice
                        diceThrow (nRemaining-1) chosendice
                
                // Complete throws
                let dice = diceThrow 2 []
                
                let rec selectScore dice (playerState: PlayerStateDef) =
                    match playerState with
                    | PlayerState (remainingScoreTypes, scores) ->
                        printfn "Select score by writing the name, available scores:"
                        printfn ""
                        printfn $"{availableScoreTypes}"
                        printfn ""
                        
                        printf "Score: "
                        let score = Console.ReadLine()
                        try
                            let selectedScore = Enum.Parse<ScoreType>(score)
                            let foundSelectedScore = 
                                remainingScoreTypes 
                                |> List.tryFind (fun s -> s = selectedScore)
                            match foundSelectedScore with
                            | Some(s) -> 
                                printfn ""
                                s
                            | None ->
                                printfn "That score type is not available"
                                printfn ""
                                selectScore dice playerState
                        with
                            | _ ->
                                printfn "Incorrect score type"
                                printfn ""
                                selectScore dice playerState
                
                // Select score and update state.
                let selectedScore = selectScore dice playerState
                let newState =
                    match playerState with
                    | PlayerState (remainingScoreTypes, scores) ->
                        let nowRemaining = remainingScoreTypes |> List.filter (fun s -> s <> selectedScore)
                        let newScores = scores.Add (selectedScore, scoreResult selectedScore dice)
                        PlayerState(nowRemaining, newScores)
                newState

            match gameState with
            | Game (nPlayers, players, playerStates) ->
                printfn $"{hLine}"
                printfn $"Starting new round"
                printfn $"{hLine}"
                printfn ""
                let newPlayerStates =
                    players 
                    |> List.map (fun p -> (p, playerTurn p playerStates.[p]))
                    |> Map.ofList
                let newGameState = Game(nPlayers, players, newPlayerStates)
                
                // Decide if the game is over or not.
                let gameFinished =
                    newPlayerStates
                    |> Map.forall (fun _ v -> 
                        match v with
                        | PlayerState (remainingScoreTypes, _) -> remainingScoreTypes.IsEmpty)
                if gameFinished then
                    game GameFinished newGameState
                else
                    game Playing newGameState
            | _ -> failwith "Inconsistent state"
        | GameFinished -> gameState

    printfn "" // Prettier output
    let resultingGameState = game NrPlayers Empty

    // Calculate final score for each player.
    let sortedPlayerScores =
        match resultingGameState with
        | Game (_, _, playerStates) ->
            playerStates
            |> Map.map (fun name playerState -> calculateScore playerState)
            |> Map.toList
            |> List.sortByDescending snd
        | _ -> failwith "Inconsistent state"

    // Display results.
    printResult sortedPlayerScores

    0 // return an integer exit code