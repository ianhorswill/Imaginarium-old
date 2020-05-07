/// <summary>
/// Represents a monadic concept or its negation
/// </summary>
public class MonadicConceptLiteral
{
    /// <summary>
    /// Concept referred to by this literal
    /// </summary>
    public readonly MonadicConcept Concept;
    /// <summary>
    /// Polarity of the literal.
    /// If true, then this means Concept, else !Concept.
    /// </summary>
    public readonly bool IsPositive;

    public MonadicConceptLiteral(MonadicConcept concept, bool isPositive = true)
    {
        Concept = concept;
        IsPositive = isPositive;
    }

    public static implicit operator MonadicConceptLiteral(MonadicConcept c)
    {
        return new MonadicConceptLiteral(c);
    }

    public MonadicConceptLiteral Inverse()
    {
        return new MonadicConceptLiteral(Concept, !IsPositive);
    }
}
