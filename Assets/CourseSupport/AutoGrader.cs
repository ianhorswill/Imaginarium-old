using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static System.IO.Path;

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
            foreach (var submission in Directory.GetDirectories(dir))
            {
                var submissionName = GetFileNameWithoutExtension(submission);
                var firstUnderscore = submissionName.IndexOf('_');
                var secondUnderscore = submissionName.IndexOf('_', firstUnderscore+1);
                var studentName = submissionName.Substring(0, firstUnderscore);
                var studentId = submissionName.Substring(firstUnderscore + 1, secondUnderscore - (firstUnderscore + 1));
                Debug.Log(studentName);
                Driver.SetOutputWindow($"Starting grading of {studentName}");
                yield return null;
                var generator = FindGeneratorDirectory(submission);
                if (generator != null)
                    yield return Driver.StartCoroutine(GradeSubmission(studentName, generator, dir, results, canvasGrades, studentId, assignmentColumnName));
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
        try
        {
            //Parser.DefinitionsDirectory = generator;
            //Driver.Ontology.ClearTests();
            //var testLoadErrors = Parser.LoadDefinitions(Combine(assignmentPath, "tests.gen"), false);

            //count = testLoadErrors.Count;
            //foreach (var e in testLoadErrors)
            //    errors.Append($"{e.Message}; ");
        }
        catch (Exception e)
        {
            results.WriteLine($"{studentName},0,\"{e.Message.Replace("\"","\"\"")}\"");
        }

        foreach (var (test, success, example) in Driver.Ontology.TestResults())
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
            d => !GetFileName(d).StartsWith("_") && !GetFileName(d).StartsWith(".") && ContainsGenFile(d));
    }

    private static bool ContainsGenFile(string submission)
    {
        return Directory.GetFiles(submission).Any(p => HasExtension(".gen"));
    }
}
