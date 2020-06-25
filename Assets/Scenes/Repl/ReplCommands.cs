#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReplCommands.cs" company="Ian Horswill">
// Copyright (C) 2019, 2020 Ian Horswill
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
using Imaginarium.Driver;
using Imaginarium.Generator;
using Imaginarium.Parsing;
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
                    Driver.AppendResponseLine("<color=red><b>Contradiction found.</b></color>");
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
                    Driver.PrependResponseLine(
                        failed == 0
                            ? $"<color=green><b>All {total} tests passed.</b></color>\n\n"
                            : $"<color=red><b>{failed} of {total} tests failed.</b></color>\n\n");
                }
                else
                    Driver.AppendResponseLine("No tests have been defined.");
            })
            .Documentation("Run all tests currently defined")
            .Command();

        yield return new SentencePattern(p, "grade", p.Text)
            .Action(() => { Object.FindObjectOfType<UIDriver>().StartCoroutine(AutoGrader.GradeAssignment(p.Text.Text.Untokenize())); })
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