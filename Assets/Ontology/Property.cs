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
using CatSAT.NonBoolean.SMT.Float;
using CatSAT.NonBoolean.SMT.MenuVariables;

/// <summary>
/// Represents a property of an individual
/// </summary>
public class Property : Concept
{
    public Property(string[] name, VariableType type) : base(name)
    {
        Name = name;
        Ontology.AllProperties[name] = this;
        Type = type;
    }

    public override string Description
    {
        get
        {
            var baseDescription = base.Description;
            if (MenuRules != null && MenuRules.Count > 0)
            {
                switch (MenuRules.Count)
                {
                    case 1:
                        return $"{baseDescription} chosen from {MenuRules[0].Menu.Name}";
                    case 2:
                        return $"{baseDescription} chosen from {MenuRules[0].Menu.Name} or {MenuRules[1].Menu.Name}";
                    case 3:
                        return $"{baseDescription} chosen from {MenuRules[0].Menu.Name}, {MenuRules[1].Menu.Name}, or {MenuRules[2].Menu.Name}";
                    default:
                        return $"{baseDescription} chosen from a number of files";
                }
            }
            else if (Type is FloatDomain d)
                return $"{baseDescription} between {d.Bounds.Lower} and {d.Bounds.Upper}";
            else return baseDescription;
        }
    }

    /// <summary>
    /// The CatSAT domain of this variable
    /// </summary>
    public readonly VariableType Type;

    public readonly List<MenuRule> MenuRules = new List<MenuRule>();

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
