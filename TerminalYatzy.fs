module TerminalYatzy

open System

open State
open Game
open Player
open GreedyComputerPlayer

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
                        | PlayerType.Computer -> makeNaiveGreedyComputerPlayer name // TODO: Implement computer player.
                        | _ -> failwith $"Invalid value for player type: {playerType}"
                    makeLegalPlayer playerLogic 2]
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
                    let currentDice = List.concat [dice; newdice]
                    printfn "" // Prettier output
                    printfn $"Throw: {diceStr newdice}"
                    if nRemaining = 0 then
                        printfn $"Final dice: {diceStr currentDice}"
                        printfn "" // Prettier output
                        currentDice
                    else
                        printfn $"Current dice: {diceStr currentDice}"
                        printfn "" // Prettier output

                        let chosenDice = player.Act playerState currentDice nRemaining
                        diceThrow (nRemaining-1) chosenDice
                
                // Complete throws
                let dice = diceThrow 2 []
                
                let rec selectScore dice (playerState: PlayerStateDef) =
                    match playerState with
                    | PlayerState (remainingScoreTypes, scores) ->
                        let selectedScore = player.SelectScore playerState dice
                        selectedScore
                
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
