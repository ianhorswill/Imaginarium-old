using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An Individual that is an intrinsic component of another individual
/// </summary>
public class Part : Concept
{
    static Part()
    {
        Ontology.AllReferentTables.Add(AllParts);
    }

    public Part(string[] name, CommonNoun kind, IEnumerable<MonadicConceptLiteral> modifiers) : base(name)
    {
        Name = name;
        AllParts[name] = this;
        Kind = kind;
        Modifiers = modifiers.ToArray();
    }

    /// <summary>
    /// The CatSAT domain of this variable
    /// </summary>
    public readonly CommonNoun Kind;

    /// <summary>
    /// Modifiers attached to the Kind
    /// </summary>
    public readonly MonadicConceptLiteral[] Modifiers;

    /// <summary>
    /// All Monadic concepts (Kind and Modifiers) attached to this Part.
    /// </summary>
    public IEnumerable<MonadicConceptLiteral> MonadicConcepts => Modifiers.Append(new MonadicConceptLiteral(Kind));

    private static readonly Dictionary<TokenString, Part> AllParts = new Dictionary<TokenString, Part>();

    /// <summary>
    /// Return the property with the specified name, if any, otherwise null.
    /// </summary>
    public static Part Find(params string[] tokens) => AllParts.LookupOrDefault(tokens);

    /// <summary>
    /// Token string used to refer to this property
    /// </summary>
    public readonly string[] Name;

    /// <inheritdoc />
    public override string[] StandardName => Name;

    /// <inheritdoc />
    public override bool IsNamed(string[] tokens) => Name.SameAs(tokens);
}