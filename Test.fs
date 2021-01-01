module ``Score Calculation Tests``
    open Xunit
    open FsUnit.Xunit

    open State
    open Game

    [<Fact>]
    let ``When scoring four 4s as Fours the score is 16`` () =
        let r = Number(DiceSide.Four, DiceSideInstances.FourDie)
        let s = calculateSingleScore r
        s |> should equal 16

    [<Fact>]
    let ``When scoring two pairs of 2 and 6 the score is 16`` () =
        let r = TwoPairs(DiceSide.Two, DiceSide.Six)
        let s = calculateSingleScore r
        s |> should equal 16

    [<Fact>]
    let ``When scoring a triple of threes the score is 9`` () =
        let r = Multiple(DiceSide.Three, MultipleInstance.Triple)
        let s = calculateSingleScore r
        s |> should equal 9

    [<Fact>]
    let ``When scoring a pair of threes the score is 6`` () =
        let r = Multiple(DiceSide.Three, MultipleInstance.Pair)
        let s = calculateSingleScore r
        s |> should equal 6

    [<Fact>]
    let ``When scoring a full house sixes full of ones the score is 20`` () =
        let r = FullHouse(DiceSide.Six, DiceSide.One)
        let s = calculateSingleScore r
        s |> should equal 20