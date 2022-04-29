#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoGrader.cs" company="Ian Horswill">
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

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Imaginarium.Driver;
using Imaginarium.Ontology;
using Imaginarium.Parsing;
using static System.IO.Path;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class AutoGrader
{
    public static IEnumerator GradeAssignment(string assignmentName)
    {
        var dir = Combine(ConfigurationFiles.GradingDirectory, assignmentName);

        // Get the grades spreadsheet from Canvas and find the column header for the grades for this assignment
        var canvasGrades = new Spreadsheet(Combine(dir,"canvas grades.csv"), "ID");
        var assignmentColumn = 0;
        foreach (var cName in canvasGrades.Header)
            if (cName.StartsWith(assignmentName + " ("))
                break;
            else 
                assignmentColumn++;

        if (assignmentColumn == canvasGrades.Header.Length)
        {
            Driver.SetOutputWindow($"No column found in canvas grades spreadsheet starting with '{assignmentName}'; grading aborted.");
        }

        var assignmentColumnName = canvasGrades.Header[assignmentColumn];

        Debug.Log($"Grades are in column {assignmentColumnName}");

        Debug.Log($"Starting grading of assignments in {dir}");
        yield return null;
        using (var results = new StreamWriter(Combine(dir, "Scores.csv")))
        {
            results.WriteLine("Student,Score,Errors");
            // If the submissions were plain .gen files, make directories for them
            foreach (var bareSubmission in Directory.GetFiles(dir))
            {
                var sub = GetFileNameWithoutExtension(bareSubmission);
                if (GetExtension(bareSubmission) == ".gen" && sub.Split('_').Length > 2)
                {
                    var subDir = Combine(dir, sub);
                    Directory.CreateDirectory(subDir);
                    File.Move(bareSubmission, Combine(subDir, Path.GetFileName(bareSubmission)));
                }
            }
            
            // Process the directories
            foreach (var submission in Directory.GetDirectories(dir))
            {
                var submissionName = GetFileNameWithoutExtension(submission);
                System.Diagnostics.Debug.Assert(submissionName != null, nameof(submissionName) + " != null");
                var firstUnderscore = submissionName.IndexOf('_');
                var secondUnderscore = submissionName.IndexOf('_', firstUnderscore+1);
                var studentName = submissionName.Substring(0, firstUnderscore);
                var studentId = submissionName.Substring(firstUnderscore + 1, secondUnderscore - (firstUnderscore + 1));
                Debug.Log(studentName);
                Driver.SetOutputWindow($"Starting grading of {studentName}");
                yield return null;
                var generator = FindGeneratorDirectory(submission);
                if (generator != null)
                    yield return Object.FindObjectOfType<UIDriver>().StartCoroutine(GradeSubmission(studentName, generator, dir, results, canvasGrades, studentId, assignmentColumnName));
                else
                {
                    results.WriteLine($"{studentName},0,No .gen files found");
                    Debug.Log("No generator found");
                    Driver.SetOutputWindow($"{studentName}: no .gen files found");
                }

                results.FlushAsync();
                yield return null;
            }
        }

        canvasGrades.Save();
        Debug.Log("Grading finished");
        Driver.SetOutputWindow("Grading complete");
    }

    private static IEnumerator GradeSubmission(string studentName, string generator, string assignmentPath, TextWriter results, Spreadsheet grades, string studentId, string scoreColumnName)
    {
        var count = 0;
        var passed = 0;
        var errors = new StringBuilder();
        var ontology = new Ontology(studentName, generator);
        try
        {
            var testLoadErrors = ontology.Parser.LoadDefinitions(Combine(assignmentPath, "tests.gen"), false);

            count = testLoadErrors.Count;
            foreach (var e in testLoadErrors)
                errors.Append($"{e.Message}; ");
        }
        catch (Exception e)
        {
            results.WriteLine($"{studentName},0,\"{e.Message.Replace("\"","\"\"")}\"");
        }

        // ReSharper disable once UnusedVariable
        foreach (var (test, success, example) in ontology.TestResults())
        {
            if (success)
                passed++;
            else
            {
                errors.Append(test.FailMessage);
                errors.Append("; ");
            }

            count++;
            Driver.SetOutputWindow($"{studentName}: {count} tests, {passed} passed; {100*passed/count}%");
            yield return null;
        }

        var score = (count>0)?(100 * passed) / count : 0;
        results.WriteLine($"{studentName},{score},{errors}");
        if (!grades.ContainsKey(studentId))
            Debug.Log($"Student does not appear in spreadsheet: {studentName} {studentId}");
        else
            grades[studentId, scoreColumnName] = score;
        Debug.Log($"{studentName}: {score}% passed");
        Driver.SetOutputWindow($"{studentName}: {score}% passed");

    }

    private static string FindGeneratorDirectory(string submission)
    {
        if (ContainsGenFile(submission))
            return submission;
        return Directory.GetDirectories(submission).FirstOrDefault(
            d =>
            {
                var fileName = GetFileName(d);
                System.Diagnostics.Debug.Assert(fileName != null, nameof(fileName) + " != null");
                return !fileName.StartsWith("_") && !fileName.StartsWith(".") && ContainsGenFile(d);
            });
    }

    private static bool ContainsGenFile(string submission)
    {
        return Directory.GetFiles(submission).Any(p => HasExtension(".gen"));
    }
}
