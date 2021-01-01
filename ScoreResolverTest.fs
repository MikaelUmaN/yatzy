module ``Score Resolution Tests``
    open Xunit
    open FsUnit.Xunit

    open State
    open Game

    [<Fact>]
    let ``When scoring four 4s as Fours the score result is a number score`` () =
        let r = scoreResult ScoreType.Fours [DiceSide.Four; DiceSide.One; DiceSide.Four; DiceSide.Four; DiceSide.Four]
        r |> should equal (Number(DiceSide.Four, DiceSideInstances.FourDie))

    [<Fact>]
    let ``When scoring no 4s as Fours the score is 0`` () =
        let r = scoreResult ScoreType.Fours [DiceSide.One; DiceSide.One; DiceSide.Five; DiceSide.Three; DiceSide.Two]
        r |> should equal (Miss)

    [<Fact>]
    let ``When scoring two pairs of 2 and 6 the score is two pairs`` () =
        let r = scoreResult ScoreType.TwoPairs [DiceSide.Two; DiceSide.One; DiceSide.Two; DiceSide.Six; DiceSide.Six]
        r |> should equal (TwoPairs(DiceSide.Two, DiceSide.Six))

    [<Fact>]
    let ``When scoring a triple of threes the score is a multiple`` () =
        let r = scoreResult ScoreType.Triple [DiceSide.Three; DiceSide.Three; DiceSide.Three; DiceSide.Six; DiceSide.One]
        r |> should equal (Multiple(DiceSide.Three, MultipleInstance.Triple))

    [<Fact>]
    let ``When scoring a pair of threes while dices also contain twos the threes are used`` () =
        let r = scoreResult ScoreType.Pair [DiceSide.Two; DiceSide.Two; DiceSide.Three; DiceSide.Three; DiceSide.Six]
        r |> should equal (Multiple(DiceSide.Three, MultipleInstance.Pair))

    [<Fact>]
    let ``When scoring a full house sixes full of ones the score is a full house`` () =
        let r = scoreResult ScoreType.FullHouse [DiceSide.Six; DiceSide.One; DiceSide.Six; DiceSide.One; DiceSide.Six]
        r |> should equal (FullHouse(DiceSide.Six, DiceSide.One))

    [<Fact>]
    let ``When scoring a mix as a straight we record a miss`` () =
        let r = scoreResult ScoreType.MajorStraight [DiceSide.Six; DiceSide.One; DiceSide.Three; DiceSide.Four; DiceSide.Six]
        r |> should equal (Miss)

    [<Fact>]
    let ``When scoring an actual stragight we receive the score`` () =
        let r = scoreResult ScoreType.MajorStraight [DiceSide.Six; DiceSide.Five; DiceSide.Four; DiceSide.Three; DiceSide.Two]
        r |> should equal (MajorStraight)

    [<Fact>]
    let ``When scoring an actual minor stragight we receive the score`` () =
        let r = scoreResult ScoreType.MinorStraight [DiceSide.One; DiceSide.Five; DiceSide.Four; DiceSide.Three; DiceSide.Two]
        r |> should equal (MinorStraight)

    [<Fact>]
    let ``When scoring a mix the result is just a mix`` () =
        let r = scoreResult ScoreType.Mix [DiceSide.One; DiceSide.Five; DiceSide.Four; DiceSide.Three; DiceSide.One]
        r |> should equal (Mix(DiceSide.One, DiceSide.Five, DiceSide.Four, DiceSide.Three, DiceSide.One))

    [<Fact>]
    let ``When scoring a yatzy that is actual the result is always yatzy`` () =
        let r = scoreResult ScoreType.Yatzy [DiceSide.One; DiceSide.One; DiceSide.One; DiceSide.One; DiceSide.One]
        r |> should equal (Yatzy)

    [<Fact>]
    let ``When scoring a yatzy that is not really a yatzy the result is always a Miss`` () =
        let r = scoreResult ScoreType.Yatzy [DiceSide.Two; DiceSide.One; DiceSide.One; DiceSide.One; DiceSide.One]
        r |> should equal (Miss)