using System.Collections.Generic;

public class ProperNoun : Noun
{
    public string[] Name;
    public readonly Individual Individual;

    public ProperNoun(string[] name)
    {
        Name = name;
        Individual = Individual.Permanent(new MonadicConcept[0], Name);
    }

    public override bool IsNamed(string[] tokens) => Name.SameAs(tokens);

    public override string[] StandardName => Name;

    public List<CommonNoun> Kinds => Individual.Kinds;
}
