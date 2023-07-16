module Models

type ErrorViewModel =
    { RequestId: string }

    member this.ShowRequestId = not (System.String.IsNullOrEmpty(this.RequestId))

[<CLIMutable>]
type Envelope = { Number: uint16; Name: string; Amount: double }

[<CLIMutable>]
type EnvelopeName = { EnvelopeName: string }

[<CLIMutable>]
type Login = { Email: string; Password: string; }

type LoginSignUpErrors = { EmailError: string; PasswordError: string; ConfirmPasswordError: string }

[<CLIMutable>]
type PasswordConfig = { MiB: byte; Iterations: byte; DegreeOfParallism: byte; Salt: byte array }

[<CLIMutable>]
type SignUp = { Email: string; Password: string; ConfirmPassword: string; }

[<CLIMutable>]
type Transaction = { TransactionNumber: uint; Amount: double; Date: System.DateTime; Note: string; }

[<CLIMutable>]
type Transfer = { DestinationNumber: uint16; Amount: double; }

[<CLIMutable>]
type Sel_Transactions_Result = { NumberOfAllTransactions: uint; Transactions: Transaction list}

[<CLIMutable>]
type User = { Id: uint; PasswordHash: byte array; LockoutExpiry: System.DateTime; PasswordConfig: PasswordConfig; }

[<CLIMutable>]
type YesNo = { YesNo: string }
