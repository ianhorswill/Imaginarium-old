using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A unit test for generators
/// Consists of an NP and whether it should exist or not.
/// </summary>
public class Test
{
    public readonly CommonNoun Noun;
    public readonly MonadicConceptLiteral[] Modifiers;
    public readonly bool ShouldExist;
    public readonly string SucceedMessage;
    public readonly string FailMessage;

    public Test(CommonNoun noun, IEnumerable<MonadicConceptLiteral> modifiers, bool shouldExist, string succeedMessage, string failMessage)
    {
        Noun = noun;
        ShouldExist = shouldExist;
        SucceedMessage = succeedMessage;
        FailMessage = failMessage;
        Modifiers = modifiers.ToArray();
    }

    public (bool success, Invention example) Run()
    {
        var example = TestExistence(Noun, Modifiers);
        var success = ShouldExist == (example != null);
        return (success, example);
    }

    private Invention TestExistence(CommonNoun noun, IEnumerable<MonadicConceptLiteral> modifiers)
    {
        var g = new Generator(Noun, Modifiers, 1);
        try
        {
            return g.Solve();
        }
        catch (CatSAT.ContradictionException)
        {
            return null;
        }
        catch (CatSAT.TimeoutException)
        {
            return null;
        }
    }
}
