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