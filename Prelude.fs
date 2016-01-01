namespace FSharpx.Cmdlet

module Prelude =
    /// Active pattern to allow `Choice1Of2` and `Choice2Of2` to be matched as `Success` and `Failure`, respectively.
    let (|Success|Failure|) = function
        | Choice1Of2 x -> Success x
        | Choice2Of2 x -> Failure x
    
    /// Convenience constructor function for creating a `Choice1Of2`.
    let inline Success x = Choice1Of2 x
    /// Convenience constructor function for creating a `Choice2Of2`.
    let inline Failure x = Choice2Of2 x
