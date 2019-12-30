#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Property.cs" company="Ian Horswill">
// Copyright (C) 2019 Ian Horswill
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

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

    public Property(string[] name, VariableType type) : base(name)
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
        public readonly MonadicConceptLiteral[] Conditions;
        public readonly Menu<string> Menu;

        public MenuRule(MonadicConceptLiteral[] conditions, Menu<string> menu)
        {
            Conditions = conditions;
            Menu = menu;
        }
    }
}
