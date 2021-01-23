module TerminalYatzy

open System

open State
open Game
open Player

let makeTerminalYatzyImpl =
    { new IYatzy with

        override _.DeterminePlayers: List<IPlayer> = 
            printf "Enter number of players: "
            let nPlayers = Console.ReadLine() |> int       
            printfn $"Starting a yatzy game with {nPlayers} players!"
            printfn "" // Prettier output

            let players = 
                [for i in 1..nPlayers ->  
                    printf $"Enter name of player {i}: "
                    let name = Console.ReadLine()
                    printfn ""
                    printf $"Enter type of player {name} (Human, Computer): "
                    let playerType = Enum.Parse<PlayerType>(Console.ReadLine())
                    let playerLogic =
                        match playerType with
                        | PlayerType.Human -> makeHumanTerminalPlayer name
                        | PlayerType.Computer -> makeHumanTerminalPlayer name // TODO: Implement computer player.
                        | _ -> failwith $"Invalid value for player type: {playerType}"
                    playerLogic]
            players

        override _.PlayerTurn player playerState: PlayerStateDef = 
            let playerTurn (player: IPlayer) playerState =
                printfn $"{hLine}"
                printfn $"Player: {player.Name}"
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

            playerTurn player playerState

        override _.GameFinished(arg1: GameState): GameState = 
            failwith "Not Implemented"
    }
