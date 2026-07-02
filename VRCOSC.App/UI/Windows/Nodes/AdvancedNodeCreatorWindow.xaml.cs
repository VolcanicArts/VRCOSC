// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fastenshtein;
using VRCOSC.App.Nodes;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Windows.Nodes;

public partial class AdvancedNodeCreatorWindow : IManagedWindow
{
    public Type? ConstructedType { get; private set; }

    private List<(double Score, Type Type)> suggestedTypes = [];

    public AdvancedNodeCreatorWindow()
    {
        InitializeComponent();

        updateText(null);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        TypeTextBox.Focus();
    }

    private void TypeTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        updateText(TypeTextBox.Text);
    }

    private void updateText(string? text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            Type? constructedType = null;

            suggestedTypes = GetTypesSimilarToName(text).OrderBy(t => t.Score).ToList();
            SuggestedTypes.Text = string.Join("\n", suggestedTypes.Select(t => t.Type.GetFriendlyName().Replace("Node", "")));

            var checkText = insertNodeIntoTypeName(text);
            TypeResolver.TryConstruct(checkText, out constructedType);

            if (constructedType is not null)
            {
                FormedTypeText.Text = constructedType.GetFriendlyName().Replace("Node", "");
                FormedTypeText.FontStyle = FontStyles.Normal;
                ConstructedType = constructedType;
                return;
            }
        }
        else
        {
            SuggestedTypes.Text = null;
        }

        FormedTypeText.Text = "null";
        FormedTypeText.FontStyle = FontStyles.Italic;
        ConstructedType = null;
    }

    public static IEnumerable<(double Score, Type Type)> GetTypesSimilarToName(string friendlyName)
    {
        var friendlyLower = friendlyName.ToLowerInvariant();

        // Try strict matching first with 50%
        var minMatchLength = Math.Min(
            Math.Max(2, (int)Math.Ceiling(friendlyLower.Length * 0.5)),
            friendlyLower.Length
        );

        var strictResults = FilterAndScore(minMatchLength);

        // If nothing found, relax to 30% and include Levenshtein
        if (!strictResults.Any())
        {
            minMatchLength = Math.Max(1, (int)Math.Ceiling(friendlyLower.Length * 0.3));
            return FilterAndScore(minMatchLength).OrderBy(r => r.Score);
        }

        return strictResults.OrderBy(r => r.Score);

        List<(double Score, Type Type)> FilterAndScore(int minLength)
        {
            var filtered = new List<(double Score, Type Type)>();
            var queryPrefix = friendlyLower.Substring(0, Math.Min(minLength, friendlyLower.Length));

            foreach (var type in NodeTypeManager.Data.SelectMany(p => p.Value.LinkedTypes))
            {
                var typeName = type.GetFriendlyName(includeGenerics: false).Replace("Node", "").ToLowerInvariant();

                // More lenient: check if prefix appears anywhere OR characters appear in order
                var hasMatch = typeName.Contains(queryPrefix) ||
                               CharactersInOrder(friendlyLower, typeName);

                if (!hasMatch)
                    continue;

                double score = CalculateScore(typeName);
                filtered.Add((score, type));
            }

            return filtered;
        }

        double CalculateScore(string baseTypeName)
        {
            if (baseTypeName == friendlyLower) return 0;
            if (baseTypeName.StartsWith(friendlyLower)) return baseTypeName.Length - friendlyLower.Length;

            var levenshtein = new Levenshtein(friendlyLower);
            var distance = levenshtein.DistanceFrom(baseTypeName);
            var positionIndex = baseTypeName.IndexOf(friendlyLower[0]);
            var penalty = positionIndex < 0 ? friendlyLower.Length : positionIndex * 0.5;

            return distance + penalty;
        }
    }

    // Check if query characters appear in order in target (e.g., "paramsource" in "parametersource")
    private static bool CharactersInOrder(string query, string target)
    {
        int queryIdx = 0;

        foreach (char c in target)
        {
            if (queryIdx < query.Length && c == query[queryIdx])
                queryIdx++;
        }

        return queryIdx >= Math.Ceiling(query.Length * 0.6); // At least 60% of characters match
    }

    private void TypeTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var firstSuggestionType = suggestedTypes.FirstOrDefault().Type;

            if (ConstructedType is null && TypeResolver.TryConstruct(firstSuggestionType.GetFriendlyName(), out _))
                ConstructedType = firstSuggestionType;

            if (ConstructedType is not null)
                Close();
        }

        if (e.Key == Key.Tab && suggestedTypes.Any())
        {
            var type = suggestedTypes.First().Type;
            var name = Regex.Replace(type.GetFriendlyName().Replace("Node", ""), "<[^<>]*>", "<>");
            TypeTextBox.Text = name;
            TypeTextBox.CaretIndex = type.IsGenericType ? name.IndexOf('<') + 1 : name.Length;
        }

        if (e.Key == Key.Escape)
        {
            ConstructedType = null;
            Close();
        }
    }

    private static string insertNodeIntoTypeName(string typeName)
    {
        var match = InsertNodeIntoTypeNameRegex().Match(typeName);
        return match.Success ? $"{match.Groups[1].Value}Node{match.Groups[2].Value}" : typeName + "Node";
    }

    public object GetComparer() => new();

    [GeneratedRegex("^([A-Za-z_][A-Za-z0-9_]*)(.*>)$")]
    private static partial Regex InsertNodeIntoTypeNameRegex();
}