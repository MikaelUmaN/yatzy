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
