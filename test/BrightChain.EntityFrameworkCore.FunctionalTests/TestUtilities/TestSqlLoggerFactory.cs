// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightChain.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BrightChain.EntityFrameworkCore.TestUtilities
{
    public class TestSqlLoggerFactory : ListLoggerFactory
    {
        private const string FileNewLine = @"
";

        private static readonly string _eol = Environment.NewLine;

        public TestSqlLoggerFactory()
            : this(_ => true)
        {
        }

        public TestSqlLoggerFactory(Func<string, bool> shouldLogCategory)
            : base(c => shouldLogCategory(c) || c == DbLoggerCategory.Database.Command.Name)
        {
            this.Logger = new TestSqlLogger(shouldLogCategory(DbLoggerCategory.Database.Command.Name));
        }

        public IReadOnlyList<string> SqlStatements
            => ((TestSqlLogger)this.Logger).SqlStatements;

        public IReadOnlyList<string> Parameters
            => ((TestSqlLogger)this.Logger).Parameters;

        public string Sql
            => string.Join(_eol + _eol, this.SqlStatements);

        public void AssertBaseline(string[] expected, bool assertOrder = true)
        {
            try
            {
                if (assertOrder)
                {
                    for (var i = 0; i < expected.Length; i++)
                    {
                        Assert.Equal(expected[i], this.SqlStatements[i], ignoreLineEndingDifferences: true);
                    }

                    Assert.Empty(this.SqlStatements.Skip(expected.Length));
                }
                else
                {
                    foreach (var expectedFragment in expected)
                    {
                        var normalizedExpectedFragment = expectedFragment.Replace("\r", string.Empty).Replace("\n", _eol);
                        Assert.Contains(
                            normalizedExpectedFragment,
                            this.SqlStatements);
                    }
                }
            }
            catch
            {
                var methodCallLine = Environment.StackTrace.Split(
                    new[] { _eol },
                    StringSplitOptions.RemoveEmptyEntries)[3].Substring(6);

                var indexMethodEnding = methodCallLine.IndexOf(')') + 1;
                var testName = methodCallLine.Substring(0, indexMethodEnding);
                var parts = methodCallLine[indexMethodEnding..].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var fileName = parts[1][..^5];
                var lineNumber = int.Parse(parts[2]);

                var currentDirectory = Directory.GetCurrentDirectory();
                var logFile = currentDirectory.Substring(
                        0,
                        currentDirectory.LastIndexOf("\\artifacts\\", StringComparison.Ordinal) + 1)
                    + "QueryBaseline.txt";

                var testInfo = testName + " : " + lineNumber + FileNewLine;
                const string indent = FileNewLine + "                ";

                var newBaseLine = $@"            AssertSql(
                {string.Join("," + indent + "//" + indent, this.SqlStatements.Take(9).Select(sql => "@\"" + sql.Replace("\"", "\"\"") + "\""))});

";

                if (this.SqlStatements.Count > 9)
                {
                    newBaseLine += "Output truncated.";
                }

                this.Logger.TestOutputHelper?.WriteLine("---- New Baseline -------------------------------------------------------------------");
                this.Logger.TestOutputHelper?.WriteLine(newBaseLine);

                var contents = testInfo + newBaseLine + FileNewLine + "--------------------" + FileNewLine;

                File.AppendAllText(logFile, contents);

                throw;
            }
        }

        protected class TestSqlLogger : ListLogger
        {
            private readonly bool _shouldLogCommands;

            public TestSqlLogger(bool shouldLogCommands)
            {
                this._shouldLogCommands = shouldLogCommands;
            }

            public List<string> SqlStatements { get; } = new();
            public List<string> Parameters { get; } = new();

            protected override void UnsafeClear()
            {
                base.UnsafeClear();

                this.SqlStatements.Clear();
                this.Parameters.Clear();
            }

            protected override void UnsafeLog<TState>(
                LogLevel logLevel,
                EventId eventId,
                string message,
                TState state,
                Exception exception)
            {
                if (eventId.Id == BrightChainEventId.ExecutingSqlQuery)
                {
                    if (this._shouldLogCommands)
                    {
                        base.UnsafeLog(logLevel, eventId, message, state, exception);
                    }

                    if (message != null)
                    {
                        var structure = (IReadOnlyList<KeyValuePair<string, object>>)state;

                        var parameters = structure.Where(i => i.Key == "parameters").Select(i => (string)i.Value).First();
                        var commandText = structure.Where(i => i.Key == "commandText").Select(i => (string)i.Value).First();

                        if (!string.IsNullOrWhiteSpace(parameters))
                        {
                            this.Parameters.Add(parameters);
                            parameters = parameters.Replace(", ", _eol) + _eol + _eol;
                        }

                        this.SqlStatements.Add(parameters + commandText);
                    }
                }

                if (eventId.Id == BrightChainEventId.ExecutingReadItem)
                {
                    if (this._shouldLogCommands)
                    {
                        base.UnsafeLog(logLevel, eventId, message, state, exception);
                    }

                    if (message != null)
                    {
                        var structure = (IReadOnlyList<KeyValuePair<string, object>>)state;

                        var partitionKey = structure.Where(i => i.Key == "partitionKey").Select(i => (string)i.Value).First();
                        var resourceId = structure.Where(i => i.Key == "resourceId").Select(i => (string)i.Value).First();

                        this.SqlStatements.Add($"ReadItem({partitionKey}, {resourceId})");
                    }
                }
                else
                {
                    base.UnsafeLog(logLevel, eventId, message, state, exception);
                }
            }
        }
    }
}
