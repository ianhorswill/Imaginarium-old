using System;

public class GrammaticalError : Exception
{
    public GrammaticalError(string message) : base(message) { }
    public GrammaticalError(string problemDescription, params string[] tokens) 
        : base($"{problemDescription} in \"{Tokenizer.Untokenize(tokens)}\"")
    { }
}
