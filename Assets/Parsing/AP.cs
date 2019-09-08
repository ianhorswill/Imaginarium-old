/// <summary>
/// Represents a phrase denoting an adjective
/// </summary>
public class AP : ReferringExpression<Adjective>
{
    public Adjective Adjective => Concept;

    protected override Adjective GetConcept() => Adjective.Find(Text) ?? new Adjective(Text);

    public override bool ValidBeginning(string firstToken) => firstToken != "a" && firstToken != "an";
}