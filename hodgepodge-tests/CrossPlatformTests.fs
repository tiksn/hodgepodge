module TIKSN.hodgepodge.CrossPlatformTests

open Expecto

[<Tests>]
let makeNullAsNoneTests =
    testList
        "CrossPlatform.makeNullAsNone"
        [ testCase "Given NULL string, should return None"
          <| fun _ ->
              let parameter: string = null
              let parameterOption = Some parameter
              let subject = CrossPlatform.makeNullAsNone parameterOption
              Expect.isTrue subject.IsNone "NULL string option should return None" ]

[<Tests>]
let setRootProcessParentToNoneTests =
    testList
        "CrossPlatform.setRootProcessParentToNone"
        [ testCase "Given None parent, should return None parent"
          <| fun _ ->
              let subject =
                  { ProcessId = 12u
                    ParentProcessId = None
                    Path = None }

              let result = CrossPlatform.setRootProcessParentToNone subject
              Expect.equal subject.ParentProcessId result.ParentProcessId "Result should be the same as input"

          testCase "Given Same Parent as own PID, should return None parent"
          <| fun _ ->
              let subject =
                  { ProcessId = 12u
                    ParentProcessId = Some 12u
                    Path = None }

              let result = CrossPlatform.setRootProcessParentToNone subject
              Expect.notEqual subject.ParentProcessId result.ParentProcessId "Result should not be the same as input"
              Expect.isTrue result.ParentProcessId.IsNone "Result should be None"

          testCase "Given Different Parent as own PID, should return unchanged"
          <| fun _ ->
              let subject =
                  { ProcessId = 12u
                    ParentProcessId = Some 14u
                    Path = None }

              let result = CrossPlatform.setRootProcessParentToNone subject
              Expect.equal subject.ParentProcessId result.ParentProcessId "Result should be the same as input"
              Expect.isTrue result.ParentProcessId.IsSome "Result should be Some"
              Expect.equal (Some 14u) result.ParentProcessId "Result should be Some 14" ]
