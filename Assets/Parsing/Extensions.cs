#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Ian Horswill">
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

using System;
using System.Collections.Generic;

public static class Extensions
{
    /// <summary>
    /// Two token strings are equivalent
    /// </summary>
    public static bool SameAs(this string[] a, string[] b)
    {
        if (a == null || b == null)
            return false;

        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
            if (!string.Equals(a[i], b[i], StringComparison.OrdinalIgnoreCase))
                return false;

        return true;
    }

    /// <summary>
    /// Add an element, if it is not already present
    /// </summary>
    public static void AddNew<T>(this List<T> list, T newElement)
    {
        if (list.Contains(newElement)) return;
        list.Add(newElement);
    }

    /// <summary>
    /// Convert a token string back into its corresponding text string
    /// </summary>
    public static string Untokenize(this IEnumerable<string> tokens) => Tokenizer.Untokenize(tokens);

    /// <summary>
    /// Returns the item in dictionary or a default value if they key is not present
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <param name="d">Dictionary to check</param>
    /// <param name="key">Key to find the value of</param>
    /// <param name="def">Default value to use if the key is not present</param>
    /// <returns></returns>
    public static TValue LookupOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue def = null)
        where TValue : class =>
        d.TryGetValue(key, out TValue value) ? value : def;
}
