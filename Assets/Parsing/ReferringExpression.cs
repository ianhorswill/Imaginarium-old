#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferringExpression.cs" company="Ian Horswill">
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

/// <summary>
/// Base class of Segments that denote Referents
/// </summary>
/// <typeparam name="TR">Type of Referent of this expression class</typeparam>
public abstract class ReferringExpression<TR> : Segment
    where TR : Referent
{
    /// <summary>
    /// Internal field for storing the Referent of this expression
    /// </summary>
    protected TR CachedConcept;

    /// <summary>
    /// Gets the Referent of this expression
    /// </summary>
    public TR Concept => CachedConcept ?? (CachedConcept = GetConcept());

    /// <summary>
    /// Clears any stored state in this expression, for example the CachedConcept.
    /// </summary>
    public override void Reset()
    {
        CachedConcept = null;
    }

    /// <summary>
    /// Determine the reference of expression.
    /// Called only from the get method of Concept.
    /// </summary>
    protected abstract TR GetConcept();

    protected ReferringExpression(Parser parser) : base(parser)
    { }
}