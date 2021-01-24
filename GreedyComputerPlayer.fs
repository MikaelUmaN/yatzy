module GreedyComputerPlayer

open State
open Game

let selectMaxScore remainingScoreTypes dice =
    let maxScore =
        (List.map ((fun sc -> (sc, scoreResult sc dice)) >> (fun (sc, sr) -> (sc, calculateSingleScore sr))) remainingScoreTypes)
        |> List.maxBy snd
    maxScore

/// Returns a greedy computer player that just goes for the maximum score still left available to him,
/// which the dice allow.
let makeNaiveGreedyComputerPlayer name =
    { new IPlayer with

        override _.Name: string = name

        override _.Act(playerState: PlayerStateDef) (dice: DiceSide list) (nRemaining: int): DiceSide list = 
            let (PlayerState (remaining, _)) = playerState
            let maxScoreType = selectMaxScore remaining dice
            if snd maxScoreType > 0 then dice else []

        override _.SelectScore(playerState: PlayerStateDef) (dice: DiceSide list): ScoreType = 
            let (PlayerState (remaining, _)) = playerState
            fst <| selectMaxScore remaining dice
    }