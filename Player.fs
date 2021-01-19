module Player

open System

open State
open Game

/// Wraps player logic into a player that obeys by the game rules.
/// If the inner logic makes illegal moves, it is given a chance to make
/// a legal move instead. A maximum of n repetitions are allowed.
let makeLegalPlayer (playerLogic: IPlayer) (allowedRepetitions: int) =
    { new IPlayer with 
        member _.Name = playerLogic.Name

        member _.Act playerState dice nRemaining =
            let rec act failures =
                if failures > allowedRepetitions then
                    failwith "Maximum allowed number of repetitions reached"
                else
                    try
                        let keptDice = playerLogic.Act playerState dice nRemaining 
                        let hasKeptdice =
                            keptDice
                            |> List.groupBy id
                            |> List.forall (fun (d, ds) -> 
                                let groupedCurrentdice =
                                    dice 
                                    |> List.groupBy id
                                    |> Map.ofList
                                groupedCurrentdice.ContainsKey d && groupedCurrentdice.[d].Length >= ds.Length)
                        if not hasKeptdice then
                            printfn "" // Prettier output
                            printfn "You do not have those dice!"
                            printfn "" // Prettier output

                            printfn ""
                            printfn $"Retries remaining: {allowedRepetitions - failures + 1}"
                            printfn ""

                            act (failures + 1)
                        else
                            keptDice
                    with
                    | e ->
                        printfn ""
                        printfn $"Exception: {e}"
                        printfn ""

                        printfn ""
                        printfn $"Retries remaining: {allowedRepetitions - failures + 1}"
                        printfn ""

                        act (failures + 1)
            act 0

        member _.SelectScore playerState dice =
            match playerState with
            | PlayerState (remainingScoreTypes, _) ->
                let rec selectScore failures =
                    if failures > allowedRepetitions then
                        failwith "Maximum allowed number of repetitions reached"
                    else
                        try
                            let selectedScore = playerLogic.SelectScore playerState dice
                            let foundSelectedScore = 
                                remainingScoreTypes 
                                |> List.tryFind (fun s -> s = selectedScore)
                            match foundSelectedScore with
                            | Some(s) -> s
                            | None ->
                                printfn ""
                                printfn "That score type is not available"
                                printfn ""
    
                                printfn ""
                                printfn $"Retries remaining: {allowedRepetitions - failures + 1}"
                                printfn ""

                                selectScore (failures + 1)
                        with
                        | e ->
                            printfn ""
                            printfn $"Exception: {e}"
                            printfn ""

                            printfn ""
                            printfn $"Retries remaining: {allowedRepetitions - failures + 1}"
                            printfn ""

                            selectScore (failures + 1)
                selectScore 0 }

/// Creates a human player acting through the terminal 
let makeHumanTerminalPlayer (name: string) =
    { new IPlayer with
        member _.Name = name

        member _.Act playerState dice nRemaining = 
            printf "Write values of dice to keep: "
            let keepdice =
                try
                    List.map (int >> enum<DiceSide>) (Console.ReadLine().Split(' ') 
                    |> Array.toList)
                with
                | _ -> []            
            keepdice

        member _.SelectScore playerState dice = 
            let availableScoreTypes = 
                match playerState with
                | PlayerState (remainingScoreTypes, _) -> remainingStr remainingScoreTypes
            
            printfn "Select score by writing the name, available scores:"
            printfn ""
            printfn $"{availableScoreTypes}"
            printfn ""
        
            printfn ""
            printfn $"Dice to select score for: {diceStr dice}"
            printfn "" //

            printf "Score: "
            let rec selectScore() =
                let score = Console.ReadLine()
                try
                    let selectedScore = Enum.Parse<ScoreType>(score)
                    selectedScore
                with
                | _ ->
                    printfn "Incorrect score type"
                    printfn ""
                    selectScore()
            selectScore() }
