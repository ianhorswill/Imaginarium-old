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
    public TR Concept
    {
        get
        {
            if (CachedConcept == null)
                CachedConcept = GetConcept();
            return CachedConcept;
        }
    }

    /// <summary>
    /// Clears any stored state in this expression, for example the CachedConcept.
    /// </summary>
    public virtual void Reset()
    {
        CachedConcept = null;
    }

    /// <summary>
    /// Determine the reference of expression.
    /// Called only from the get method of Concept.
    /// </summary>
    protected abstract TR GetConcept();
}