using CatSAT;
using System;
using System.Collections.Generic;
using System.Linq;
using CatSAT.NonBoolean.SMT.MenuVariables;
using static CatSAT.Language;

/// <summary>
/// Generates a specified type of object based on the information in the ontology
/// </summary>
public class Generator
{
    /// <summary>
    /// The output from the "imagine" command that created this Generator
    /// </summary>
    public static Generator Current;

    #region Instance variables
    /// <summary>
    /// The object whose attributes are to be generated
    /// </summary>
    public List<Individual> Individuals = new List<Individual>();

    /// <summary>
    /// The CatSAT problem that will generate the values for the Creation's attributes
    /// </summary>
    public Problem Problem;

    /// <summary>
    /// How many objects the user requested
    /// </summary>
    public int Count;
    #endregion

    /// <summary>
    /// Creates a generator for objects of the specified types
    /// </summary>
    /// <param name="noun">Base common noun for the object</param>
    /// <param name="concepts">Other monadic concepts that must be true of the object</param>
    public Generator(CommonNoun noun, params MonadicConcept[] concepts) : this(noun, (IEnumerable<MonadicConcept>)concepts)
    {  }

    /// <summary>
    /// Creates a generator for objects of the specified types
    /// </summary>
    /// <param name="noun">Base common noun for the object</param>
    /// <param name="concepts">Other monadic concepts that must be true of the object</param>
    /// <param name="count">Number of objects of the specified type to include</param>
    public Generator(CommonNoun noun, IEnumerable<MonadicConcept> concepts, int count = 1)
    {
        Count = count;
        var ca = concepts.ToArray();
        for (var i = 0; i < Count; i++)
            Individuals.Add(Individual.Ephemeral(ca.Append(noun), noun.SingularForm.Append(i.ToString()).ToArray()));

        Rebuild();
    }

    /// <summary>
    /// Rebuild and re-solve the CatSAT problem
    /// </summary>
    public void Rebuild()
    {
        // Do this first so that Problem.Current gets set.
        Problem = new Problem("invention");
        ResetPredicateTables();

        foreach (var i in Individuals)
            AddFormalization(i);

        var verbs = Verb.AllVerbs.ToArray();

        foreach (var i1 in Individuals)
        foreach (var i2 in Individuals)
        foreach (var v in verbs)
            // v can't hold of i1,i2 unless they're both in the v's domain
            if (CanBeA(i1, v.SubjectKind) && CanBeA(i2, v.ObjectKind))
            {
                var proposition = Holds(v, i1, i2);
                proposition.InitialProbability = v.Density;
                var relationDoesNotHold = Not(proposition);
                AddImplication(relationDoesNotHold, Not(IsA(i1, v.SubjectKind)));
                AddImplication(relationDoesNotHold, Not(IsA(i2, v.ObjectKind)));
            }

        foreach (var v in verbs)
        {
            if (v.IsFunction || v.IsTotal)
            {
                var min = v.IsTotal ? 1 : 0;
                var max = v.IsFunction ? 1 : Individuals.Count;
                foreach (var i1 in Individuals)
                    if (CanBeA(i1, v.SubjectKind))
                        Problem.Quantify(min, max, Individuals.Where(i2=>CanBeA(i2, v.ObjectKind)).Select(i2 => Holds(v, i1, i2)));
            }

            if (v.IsAntiReflexive)
                // No individuals can self-relate
            {
                foreach (var i in Individuals)
                    if (CanBeA(i, v.SubjectKind))
                        Problem.Assert(Not(Holds(v, i, i)));
            }

            if (v.IsReflexive)
            {
                // All eligible individuals must self-relate
                foreach (var i in Individuals)
                    if (CanBeA(i, v.SubjectKind))
                        Problem.Assert(Holds(v, i, i));
            }

            if (v.Generalizations.Count > 0 || v.MutualExclusions.Count > 0)
                foreach (var (s, o) in Domain(v))
                {
                    foreach (var g in v.Generalizations)
                        AddImplication(Holds(g, s, o), Holds(v, s, o));
                    foreach (var e in v.MutualExclusions)
                        Problem.AtMost(1, Holds(v, s, o), Holds(e, s, o));
                }
        }
    }

    IEnumerable<(Individual, Individual)> Domain(Verb v)
    {
        foreach (var i1 in Individuals)
            if (CanBeA(i1, v.SubjectKind))
                foreach (var i2 in Individuals)
                    if (CanBeA(i1, v.ObjectKind))
                        yield return (i1, i2);
    }

    /// <summary>
    /// Make a new Model
    /// </summary>
    public Invention Solve()
    {
        return new Invention(this, Problem.Solve());
    }

    /// <summary>
    /// Add all clauses and variables relevant to the individual
    /// </summary>
    private void AddFormalization(Individual ind)
    {
        // We know that i IS of kind k, so assert that and its implications
        bool AssertKind(Individual i, CommonNoun k)
        {
            var isK = IsA(i, k);
            if (MaybeAssert(isK))
            {
                // We haven't already processed the constraints for i being a k)
                foreach (var super in k.Superkinds)
                    AssertKind(i, super);
                MaybeFormalizeKindInstance(i, k);

                foreach (var p in k.Properties)
                {
                    if (!i.Properties.ContainsKey(p))
                        // Create SMT variable
                    {
                        var v = p.Type == null ? new MenuVariable<string>(p.Text, null, Problem, isK) : p.Type.Instantiate(p.Text, Problem, isK);
                        i.Properties[p] = v;
                        foreach (var r in p.MenuRules)
                        {
                            AddImplication(i, r.Conditions.Append(k), ((MenuVariable<string>)v).In(r.Menu));
                            //AddClause(r.Conditions.Select(c => Not(MembershipProposition(i, c))).Append(Not(isK)).Append(((MenuVariable<string>)v).In(r.Menu)));
                        }
                    }
                }

                return true;
            }

            return false;
        }

        // We know that i MIGHT BE of kind k so add clauses stating that if it is, i
        void SolveForSubclass(Individual i, CommonNoun k)
        {
            MaybeFormalizeKindInstance(i, k);
            if (k.Subkinds.Count == 0)
                return;
            Problem.Unique(k.Subkinds.Select(sub => IsA(i, sub)).Append(Not(IsA(i, k))));
            foreach (var sub in k.Subkinds)
                SolveForSubclass(i, sub);
        }

        // Add clauses for implications that follow from i being of kind k
        void MaybeFormalizeKindInstance(Individual i, CommonNoun k)
        {
            if (kindsFormalized.Contains(new Tuple<Individual, CommonNoun>(i, k)))
                return;

            foreach (var a in k.ImpliedAdjectives)
            {
                AddImplication(i, a.Conditions.Append(k), a.Adjective);
                //AddClause(a.Conditions.Select(c => Not(MembershipProposition(i, c))).Append(Not(MembershipProposition(i, k)))
                //        .Append(MembershipProposition(i, a.Adjective)));
            }

            foreach (var adj in k.RelevantAdjectives)
                // Force the creation of the Proposition representing the possibility of the adjective
                // being true of the individual
                IsA(i, adj);
            foreach (var set in k.AlternativeSets)
            {
                var clause = set.Alternatives.Select(a => IsA(i, a)).Append(Not(IsA(i, k)));
                if (set.IsRequired)
                    Problem.Unique(clause);
                else
                    Problem.AtMost(1, clause);
            }

            kindsFormalized.Add(new Tuple<Individual, CommonNoun>(i, k));
        }

        ind.Properties.Clear();
        foreach (var k in ind.Kinds)
        {
            if (AssertKind(ind, k))
                SolveForSubclass(ind, k);
        }

        foreach (var a in ind.Adjectives) 
            MaybeAssert(IsA(ind, a));
    }
    
    #region Predicate and Proposition tracking
    private void ResetPredicateTables()
    {
        predicates.Clear();
        asserted.Clear();
        kindsFormalized.Clear();
    }

    /// <summary>
    /// Assert p is true, unless we've already asserted it
    /// </summary>
    /// <param name="p">Proposition to assert</param>
    /// <returns>True if it had not already been asserted</returns>
    private bool MaybeAssert(Proposition p)
    {
        if (asserted.Contains(p))
            return false;
        Problem.Assert(p);
        asserted.Add(p);
        return true;
    }

    /// <summary>
    /// The proposition representing that concept k applies to individual i
    /// </summary>
    public Proposition IsA(Individual i, MonadicConcept k)
    {
        if (k is CommonNoun n && !CanBeA(i, n))
            return false;

        return PredicateOf(k)(i);
    }

    public bool CanBeA(Individual i, CommonNoun kind)
    {
        bool SearchUp(CommonNoun k)
        {
            if (k == kind)
                return true;
            foreach (var super in k.Superkinds)
                if (SearchUp(super))
                    return true;
            return false;
        }

        bool SearchDown(CommonNoun k)
        {
            if (k == kind)
                return true;
            foreach (var sub in k.Subkinds)
                if (SearchDown(sub))
                    return true;
            return false;
        }

        foreach (var k in i.Kinds)
            if (SearchUp(k) || SearchDown(k))
                return true;

        return false;
    }

    public Proposition Holds(Verb v, Individual i1, Individual i2) => PredicateOf(v)(i1, i2);

    /// <summary>
    /// The predicate used to represent concept in the CatSAT problem
    /// </summary>
    private Func<Individual,Proposition> PredicateOf(MonadicConcept c)
    {
        if (predicates.TryGetValue(c, out Func<Individual,Proposition> p))
            return p;
        return predicates[c] = Predicate<Individual>(c.StandardName.Untokenize());
    }

    private Func<Individual, Individual, Proposition> PredicateOf(Verb v)
    {
        if (relations.TryGetValue(v, out Func<Individual, Individual, Proposition> p))
            return p;
        var name = v.StandardName.Untokenize();
        return relations[v] = v.IsSymmetric?SymmetricPredicate<Individual>(name):
            Predicate<Individual, Individual>(name);
    }

    /// <summary>
    /// Which individual/kind pairs we've already generated clauses for
    /// </summary>
    private readonly HashSet<Tuple<Individual, CommonNoun>> kindsFormalized =
        new HashSet<Tuple<Individual, CommonNoun>>();

    /// <summary>
    /// Propositions already asserted in Problem
    /// </summary>
    private readonly HashSet<Proposition> asserted = new HashSet<Proposition>();

    /// <summary>
    /// Predicates created within Problem
    /// </summary>
    private readonly Dictionary<object, Func<Individual,Proposition>> predicates = new Dictionary<object, Func<Individual,Proposition>>();
    
    private readonly Dictionary<Verb, Func<Individual,Individual, Proposition>> relations = new Dictionary<Verb, Func<Individual,Individual, Proposition>>();

    #endregion

    #region Clause generation
    /// <summary>
    /// Add clause to Problem stating that consequent(i) follow from antecedent(i)
    /// </summary>
    /// <param name="i">Individual for which this implication holds</param>
    /// <param name="antecedents">A set of conditions on i</param>
    /// <param name="consequent">A concept that must be true of i when the antecedents are true.</param>
    void AddImplication(Individual i, IEnumerable<MonadicConcept> antecedents, MonadicConcept consequent)
    {
        AddClause(antecedents.Select(a => Not(IsA(i, a))).Append(IsA(i, consequent)));
    }

    /// <summary>
    /// Add clause to Problem stating that consequent(i) follow from antecedent(i)
    /// </summary>
    /// <param name="i">Individual for which this implication holds</param>
    /// <param name="antecedents">A set of conditions on i</param>
    /// <param name="consequent">A proposition that must follow from the antecedent applying to i.</param>
    void AddImplication(Individual i, IEnumerable<MonadicConcept> antecedents, Literal consequent)
    {
        AddClause(antecedents.Select(a => Not(IsA(i, a))).Append(consequent));
    }

    /// <summary>
    /// Assert that all antecedents being true implies consequent
    /// </summary>
    void AddImplication(Literal consequent, params Literal[] antecedents)
    {
        AddClause(antecedents.Select(Not).Append(consequent));
    }

    /// <summary>
    /// Add a CNF clause to the problem.  This states that at least one the literals must be true.
    /// </summary>
    /// <param name="literals"></param>
    void AddClause(IEnumerable<Literal> literals)
    {
        Problem.AtLeast(1, literals);
    }
    #endregion
}
