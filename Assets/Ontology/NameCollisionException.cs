using System;

public class NameCollisionException : UserException
{
    public readonly string[] Name;
    public readonly Type OldType;
    public readonly Type NewType;

    public NameCollisionException(string[] name, Type oldType, Type newType)
        : base(
            $"Can't create a new {newType}, {name.Untokenize()}, because there is already {oldType} of the same name.",
            $"You appear to be using {name.Untokenize()} as if it were a {Concept.EnglishTypeName(newType)}, but I thought it was a {Concept.EnglishTypeName(oldType)}")
    {
        Name = name;
        OldType = oldType;
        NewType = newType;
    }
}
