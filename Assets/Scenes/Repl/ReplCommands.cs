using System.Collections.Generic;
using CatSAT;
using UnityEngine;

public static class ReplCommands
{
    public static IEnumerable<SentencePattern> Commands(Parser p, UIDriver repl)
    {
        yield return new SentencePattern(p, "debug")
            .Action(() => SentencePattern.LogAllParsing = !SentencePattern.LogAllParsing)
            .Documentation("Toggles debugging of input parsing")
            .Command();

        yield return new SentencePattern(p, "help")
            .Action(() =>
            {
                foreach (var r in p.SentencePatterns)
                    Driver.AppendResponseLine(r.HelpDescription);
            })
            .Documentation("Prints this list of commands")
            .Command();

        yield return new SentencePattern(p, "quit")
            .Action(Application.Quit)
            .Documentation("Ends the application")
            .Command();

        yield return new SentencePattern(p, "exit")
            .Action(Application.Quit)
            .Documentation("Ends the application")
            .Command();

        yield return new SentencePattern(p, "imagine", "!", p.Object)
            .Action(() =>
            {
                var countRequest = p.Object.ExplicitCount;
                var count = countRequest ?? (p.Object.Number == Parser.Number.Plural ? 9 : 1);
                try
                {
                    Generator.Current = new Generator(p.Object.CommonNoun, p.Object.Modifiers, count);
                }
                catch (ContradictionException e)
                {
                    Driver.AppendResponseLine($"<color=red><b>Contradiction found.</b></color>");
                    Driver.AppendResponseLine($"Internal error message: {e.Message}");
                }
            })
            .Command()
            .Documentation(
                "Generates one or more Objects.  For example, 'imagine a cat' or 'imagine 10 long-haired cats'.");

        yield return new SentencePattern(p, "undo")
            .Action(() => repl.History.Undo())
            .Command()
            .Documentation("Undoes the last change to the ontology.");

        yield return new SentencePattern(p, "start", "over")
            .Action(() =>
            {
                repl.History.Clear();
                Driver.AppendResponseLine("Knowledge-base erased.  I don't know anything.");
            })
            .Command()
            .Documentation("Tells the system to forget everything you've told it about the world.");

        yield return new SentencePattern(p, "save", p.ListName)
            .Action(() => { repl.History.Save(p.DefinitionFilePath(p.ListName.Text.Untokenize())); })
            .Command()
            .Documentation("Saves assertions to a file.");

        yield return new SentencePattern(p, "test")
            .Action(() =>
            {
                var total = 0;
                var failed = 0;
                foreach (var (test, success, example) in p.Ontology.TestResults())
                {
                    total++;
                    if (!success) failed++;
                    Driver.AppendResponseLine(
                        success
                            ? $"<B><color=green>{test.SucceedMessage}</color></b>"
                            : $"<b><color=red>{test.FailMessage}</color></b>"
                    );
                    if (example != null)
                        Driver.AppendResponseLine($"Example: {example.Description(example.Individuals[0])}");
                }

                if (total > 0)
                {
                    if (failed == 0)
                        Driver.PrependResponseLine(
                            $"<color=green><b>All {total} tests passed.</b></color>\n\n");
                    else
                        Driver.PrependResponseLine(
                            $"<color=red><b>{failed} of {total} tests failed.</b></color>\n\n");
                }
                else
                    Driver.AppendResponseLine("No tests have been defined.");
            })
            .Documentation("Run all tests currently defined")
            .Command();

        yield return new SentencePattern(p, "grade", p.Text)
            .Action(() => { Driver.StartCoroutine(AutoGrader.GradeAssignment(p.Text.Text.Untokenize())); })
            .Documentation("Run all tests currently defined")
            .Command();

        yield return new SentencePattern(p, "decompile")
            .Action(() =>
            {
                var g = Generator.Current;
                Driver.AppendResponseLine(g != null
                    ? g.Problem.Decompiled
                    : "Please type an imagine command first");
            })
            .Documentation("Dump the clauses of the compiled SAT problem")
            .Command();

        yield return new SentencePattern(p, "stats")
            .Action(() =>
            {
                var g = Generator.Current;
                if (g == null)
                    Driver.AppendResponseLine("Please type an imagine command first.");
                else
                {
                    Driver.AppendResponseLine(g.Problem.Stats);
                    Driver.AppendResponseLine(g.Problem.PerformanceStatistics);
                }
            })
            .Documentation("Dump the clauses of the compiled SAT problem")
            .Command();
    }
}