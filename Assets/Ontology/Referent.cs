using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// An object within the ontology (e.g. a Concept or Individual)
/// </summary>
[DebuggerDisplay("{" + nameof(Text) + "}")]
public abstract class Referent
{
    /// <summary>
    /// True if this object's name matches the specified token string.
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public abstract bool IsNamed(string[] tokens);

    /// <summary>
    /// String form of standard name of this object
    /// </summary>
    public virtual string Text => Tokenizer.Untokenize(StandardName);

    /// <summary>
    /// Tokenized form of the standard name of this object
    /// </summary>
    public abstract string[] StandardName { get; }
    
    /// <inheritdoc />
    public override string ToString() => Text;
}