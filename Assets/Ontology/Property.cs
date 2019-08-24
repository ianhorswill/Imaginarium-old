using System.Collections.Generic;
using CatSAT;
using CatSAT.NonBoolean.SMT.MenuVariables;

/// <summary>
/// Represents a property of an individual
/// </summary>
public class Property : Concept
{
    static Property()
    {
        Ontology.AllReferentTables.Add(AllProperties);
    }

    public Property(string[] name, VariableType type)
    {
        Name = name;
        AllProperties[name] = this;
        Type = type;
    }

    /// <summary>
    /// The CatSAT domain of this variable
    /// </summary>
    public readonly VariableType Type;

    public readonly List<MenuRule> MenuRules = new List<MenuRule>();

    private static readonly Dictionary<TokenString, Property> AllProperties = new Dictionary<TokenString, Property>();

    /// <summary>
    /// Return the property with the specified name, if any, otherwise null.
    /// </summary>
    public static Property Find(params string[] tokens) => AllProperties.LookupOrDefault(tokens);

    /// <summary>
    /// Token string used to refer to this property
    /// </summary>
    public readonly string[] Name;

    /// <inheritdoc />
    public override string[] StandardName => Name;

    /// <inheritdoc />
    public override bool IsNamed(string[] tokens) => Name.SameAs(tokens);

    public class MenuRule
    {
        public readonly MonadicConcept[] Conditions;
        public readonly Menu<string> Menu;

        public MenuRule(MonadicConcept[] conditions, Menu<string> menu)
        {
            Conditions = conditions;
            Menu = menu;
        }
    }
}
