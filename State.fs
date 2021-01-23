module State

type ScoreType =
    | Ones = 1
    | Twos = 2
    | Threes = 3
    | Fours = 4
    | Fives = 5
    | Sixes = 6
    | Pair = 7
    | Triple = 8
    | Quadruple = 9
    | TwoPairs = 10
    | FullHouse = 11
    | MinorStraight = 12
    | MajorStraight = 13
    | Mix = 14
    | Yatzy = 15

type DiceSide =
    | One = 1
    | Two = 2
    | Three = 3
    | Four = 4
    | Five = 5
    | Six = 6

type DiceSideInstances =
    | OneDie = 1
    | TwoDie = 2
    | ThreeDie = 3
    | FourDie = 4
    | FiveDie = 5

type MultipleInstance =
    | Pair = 2
    | Triple = 3
    | Quadruple = 4
    | Yatzy = 5

type ScoreResult =
    | Miss
    | Number of Side: DiceSide * Instances: DiceSideInstances
    | Multiple of Side: DiceSide * Instance: MultipleInstance
    | TwoPairs of PairSideOne: DiceSide * PairSideTwo: DiceSide 
    | FullHouse of TripleSide: DiceSide * PairSide: DiceSide
    | MinorStraight
    | MajorStraight
    | Mix of SideOne: DiceSide * SideTwo: DiceSide * SideThree: DiceSide * SideFour: DiceSide * SideFive: DiceSide
    | Yatzy

type PlayerStateDef = PlayerState of RemainingScoreTypes: List<ScoreType> * Scores: Map<ScoreType, ScoreResult>

/// A player is either a human or a computer
type PlayerType =
    | Human = 1
    | Computer = 2

/// A player takes decisions on what dice to keep and what score to assign.
type IPlayer =

    /// The name of the player
    abstract member Name: string

    /// Given state, remaining number of throws and current dices, returns the set of dices to keep.
    /// May be equal to or less than the input dices - but never more.
    abstract member Act: playerState:PlayerStateDef -> dice:list<DiceSide> -> nRemaining:int -> list<DiceSide>

    /// Given state, and dice result, selects a score type
    abstract member SelectScore: playerState:PlayerStateDef -> dice:list<DiceSide> -> ScoreType

type GameState =
    | Empty
    | Game of Players: List<IPlayer> * PlayerStates: Map<string, PlayerStateDef>

/// State of the application.
type ApplicationState =
| PlayerSetup
| Playing
| GameFinished

/// Abstraction for the game of yatzy, from player setup to game finish.
type IYatzy =

    /// Determines players and player logic.
    abstract member DeterminePlayers: List<IPlayer>

    /// Plays a player turn of the game.
    abstract member PlayerTurn: IPlayer -> PlayerStateDef -> PlayerStateDef

    /// Convenience function to perform any kind of clean-up or analysis 
    /// when the game is over.
    abstract member GameFinished: GameState -> GameState
