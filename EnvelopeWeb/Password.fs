module Password

open Models
open System.Linq

let SaltSize = 16;
let HashOutputSize = 32;

let makeDefaultPasswordConfig() = { MiB = 19uy; Iterations = 2uy; DegreeOfParallism = 1uy; Salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(SaltSize) }

let private passwordToBytes (password:string) =
    System.Text.Encoding.UTF8.GetBytes(password.Trim())

let generateHash passwordConfig (password:string) =
    let passwordBytes = passwordToBytes password
    let argon = new Konscious.Security.Cryptography.Argon2id(passwordBytes)
    argon.MemorySize <- 1024 * System.Convert.ToInt32(passwordConfig.MiB)
    argon.Iterations <- passwordConfig.Iterations |> System.Convert.ToInt32
    argon.DegreeOfParallelism <- passwordConfig.DegreeOfParallism |> System.Convert.ToInt32
    argon.Salt <- passwordConfig.Salt
    argon.GetBytes(HashOutputSize)

let verify passwordConfig password = (generateHash passwordConfig password).SequenceEqual
