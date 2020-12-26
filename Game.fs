module Game

open System

open State

let random = Random()

let minDieSide = DiceSide.One
let maxDieSide = DiceSide.Six
let ndice = 5

let castDice nDice =
    [for i in 1..nDice -> random.Next(int maxDieSide) + (int minDieSide) |> enum<DiceSide>]

let initPlayerState: PlayerStateDef =
    let remainingScoreTypes = Enum.GetValues<ScoreType>() |> Array.toList//Enum.GetValues(typeof<ScoreType>) |> Array.toList
    PlayerState(remainingScoreTypes, Map.empty)

/// Score

let countSide (d: DiceSide) (dice: list<DiceSide>) =
    dice |> List.filter (fun dd -> dd = d) |> List.length

let scoreCountSide (d: DiceSide) (dice: list<DiceSide>) =
    let cnt = countSide d dice
    Multiple(d, cnt |> enum<MultipleInstance>)

let countMultiple (m: MultipleInstance) (dice: list<DiceSide>) (d: DiceSide) =
    let cnt = countSide d dice
    let hasMultiple = cnt >= int(m)
    if hasMultiple then
        Multiple(d, m)
    else
        Miss

let scoreResult (scoreType: ScoreType) (dice: list<DiceSide>) =
    match scoreType with
    | ScoreType.Ones -> scoreCountSide DiceSide.One dice
    | ScoreType.Twos -> scoreCountSide DiceSide.Two dice
    | ScoreType.Threes -> scoreCountSide DiceSide.Three dice
    | ScoreType.Fours -> scoreCountSide DiceSide.Four dice
    | ScoreType.Fives -> scoreCountSide DiceSide.Five dice
    | ScoreType.Sixes -> scoreCountSide DiceSide.Six dice
    | ScoreType.Pair ->
        Enum.GetValues<DiceSide>() 
        |> Array.toList
        |> List.map (countMultiple MultipleInstance.Pair dice)
        |> List.maxBy (fun m -> 
            match m with
            | Miss -> 0
            | Multiple(s, i) -> int(s)
            | _ -> 0)
    | ScoreType.Triple ->
        Enum.GetValues<DiceSide>() 
        |> Array.toList
        |> List.map (countMultiple MultipleInstance.Triple dice)
        |> List.maxBy (fun m -> 
            match m with
            | Miss -> 0
            | Multiple(s, i) -> int(s)
            | _ -> 0)
    | ScoreType.Quadruple ->
        Enum.GetValues<DiceSide>() 
        |> Array.toList
        |> List.map (countMultiple MultipleInstance.Quadruple dice)
        |> List.maxBy (fun m -> 
            match m with
            | Miss -> 0
            | Multiple(s, i) -> int(s)
            | _ -> 0)
    | ScoreType.TwoPairs ->
        let pairs =
            Enum.GetValues<DiceSide>() 
            |> Array.toList
            |> List.map ((countMultiple MultipleInstance.Pair dice) >> (fun m -> 
                match m with
                | Miss -> None
                | Multiple(s, _) -> Some(s)
                | _ -> None))
            |> List.filter (Option.isSome)
            |> List.map (Option.get)
        if pairs.Length = 2 then
            TwoPairs(pairs.[0], pairs.[1])
        else
            Miss
    | ScoreType.FullHouse ->
        let triple =
            Enum.GetValues<DiceSide>() 
            |> Array.toList
            |> List.map ((countMultiple MultipleInstance.Triple dice) >> (fun m -> 
                match m with
                | Miss -> None
                | Multiple(s, _) -> Some(s)
                | _ -> None))
            |> List.filter (Option.isSome)
            |> List.map (Option.get)
        if triple.Length = 1 then
            let tripleSide = triple.[0]
            let pair =
                Enum.GetValues<DiceSide>()
                |> Array.toList
                |> List.filter (fun s -> s <> tripleSide)
                |> List.map ((countMultiple MultipleInstance.Pair dice) >> (fun m -> 
                    match m with
                    | Miss -> None
                    | Multiple(s, _) -> Some(s)
                    | _ -> None))
                |> List.filter (Option.isSome)
                |> List.map (Option.get)
            if pair.Length = 1 then
                let pairSide = pair.[0]
                FullHouse(tripleSide, pairSide)
            else
                Miss
        else
            Miss
    | ScoreType.MinorStraight ->
        let sortedDice =
            dice
            |> List.map int
            |> List.sortDescending
            
        let diffIsZero =
            List.zip (sortedDice |> List.take 4) (sortedDice |> List.skip 1)
            |> List.map (fun (x, y) -> x - y)
            |> List.forall (fun dx -> dx = 0)
        if diffIsZero && sortedDice.[0] = int(DiceSide.Five) && sortedDice.[^1] = int(DiceSide.One) then
            MinorStraight
        else
            Miss
    | ScoreType.MajorStraight ->
        let sortedDice =
            dice
            |> List.map int
            |> List.sortDescending
            
        let diffIsZero =
            List.zip (sortedDice |> List.take 4) (sortedDice |> List.skip 1)
            |> List.map (fun (x, y) -> x - y)
            |> List.forall (fun dx -> dx = 1)
        if diffIsZero && sortedDice.[0] = int(DiceSide.Six) && sortedDice.[^1] = int(DiceSide.Two) then
            MajorStraight
        else
            Miss
    | ScoreType.Mix ->
        Mix(dice.[0], dice.[1], dice.[2], dice.[3], dice.[4])
    | ScoreType.Yatzy ->
        let quintuple =
            Enum.GetValues<DiceSide>() 
            |> Array.toList
            |> List.map ((countMultiple MultipleInstance.Yatzy dice) >> (fun m -> 
                match m with
                | Miss -> None
                | Multiple(s, _) -> Some(s)
                | _ -> None))
            |> List.filter (Option.isSome)
            |> List.map (Option.get)
        if quintuple.Length = 1 then
            Yatzy // Special score.
        else
            Miss
    | _ -> failwith $"Unknown score type: {scoreType}"

let calculateScore playerState =
    let (PlayerState (_, scores)) = playerState
    
    let sumScores =
        scores
        |> Map.map (fun k v ->
            match v with
            | Miss -> 0
            | Number(s, i) -> int(s) * int(i)
            | Multiple(s, i) -> int(s) * int(i)
            | TwoPairs(s1, s2) -> 2 * int(s1) + 2 * int(s2) 
            | FullHouse(s1, s2) -> 3 * int(s1) + 2 * int(s2)
            | MinorStraight -> 15
            | MajorStraight -> 20
            | Mix(s1, s2, s3, s4, s5) -> int(s1) + int(s2) + int(s3) + int(s4) + int(s5)
            | Yatzy -> 50)
        |> Map.toList
        |> List.sumBy snd

    let bonus =
        let numberScore =
            scores
            |> Map.map (fun k v ->
                match v with
                | Number(s, i) -> int(s) * int(i)
                | _ -> 0)
            |> Map.toList
            |> List.sumBy snd
        if numberScore >= 63 then
            50
        else
            0
    
    sumScores + bonus

/// Printers

let diceStr (dice: seq<DiceSide>) =
    dice
    |> Seq.map (int >> string)
    |> String.concat " "

let hLine = "-----------------------------------------------"

let remainingStr (remainingScoreTypes: seq<ScoreType>) =
    remainingScoreTypes
    |> Seq.map (string)
    |> String.concat "\n"

let printResult (sortedPlayerScores: list<string * int>) =
    printfn "Yatzy Results!"
    printfn ""
    sortedPlayerScores
    |> List.iter (fun (player, score) -> 
        printfn $"{hLine}"
        printfn $"Player: {player}"
        printfn $"Score: {score}"
        printfn $"{hLine}"
        printfn ""
    )