// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using System.Text;

using CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;
using CommunityToolkit.Mvvm.SourceGenerators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HB.FullStack.Common.SourceGenerators.PropertyTrackable;

/// <summary>
/// A source generator for the <c>ObservablePropertyAttribute</c> type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class TrackPropertyGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        //#if DEBUG
        //        if (!Debugger.IsAttached)
        //        {
        //            Debugger.Launch();
        //        }
        //#endif

        // Gather info for all annotated fields
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, Result<TrackPropertyInfo?> Info)> propertyInfoWithErrors =
            context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax { Parent: ClassDeclarationSyntax or RecordDeclarationSyntax, AttributeLists.Count: > 0 } } },
                static (context, token) =>
                {
                    if (!context.SemanticModel.Compilation.HasLanguageVersionAtLeastEqualTo(LanguageVersion.CSharp8))
                    {
                        return default;
                    }

                    IFieldSymbol fieldSymbol = (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node, token)!;

                    // Filter the fields using [TrackProperty]
                    if (!fieldSymbol.HasAttributeWithFullyQualifiedName("global::HB.FullStack.Common.PropertyTrackable.TrackPropertyAttribute"))
                    {
                        return default;
                    }

                    // Produce the incremental models
                    HierarchyInfo hierarchy = HierarchyInfo.From(fieldSymbol.ContainingType);
                    TrackPropertyInfo? propertyInfo = Execute.TryGetPropertyInfo(fieldSymbol, out ImmutableArray<Diagnostic> diagnostics);

                    return (Hierarchy: hierarchy, new Result<TrackPropertyInfo?>(propertyInfo, diagnostics));
                })
            .Where(static item => item.Hierarchy is not null);

        // Output the diagnostics
        context.ReportDiagnostics(propertyInfoWithErrors.Select(static (item, _) => item.Info.Errors));

        // Get the filtered sequence to enable caching
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, TrackPropertyInfo Info)> propertyInfo =
            propertyInfoWithErrors
            .Select(static (item, _) => (item.Hierarchy, Info: item.Info.Value))
            .Where(static item => item.Info is not null)!
            .WithComparers(HierarchyInfo.Comparer.Default, TrackPropertyInfo.Comparer.Default);

        // Split and group by containing type
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, ImmutableArray<TrackPropertyInfo> Properties)> groupedPropertyInfo =
            propertyInfo
            .GroupBy(HierarchyInfo.Comparer.Default)
            .WithComparers(HierarchyInfo.Comparer.Default, TrackPropertyInfo.Comparer.Default.ForImmutableArray());

        // Generate the requested properties and methods
        context.RegisterSourceOutput(groupedPropertyInfo, static (context, item) =>
        {
            // Generate all member declarations for the current type
            ImmutableArray<MemberDeclarationSyntax> memberDeclarations =
                item.Properties
                .Select(Execute.GeneratePropertySyntax)
                .Concat(item.Properties.Select(Execute.GetTrackProperyTwoMethodsSyntax).SelectMany(static l => l))
                .ToImmutableArray();

            // Insert all members into the same partial type declaration
            CompilationUnitSyntax compilationUnit = item.Hierarchy.GetCompilationUnit(memberDeclarations);

            context.AddSource($"{item.Hierarchy.FilenameHint}.properties.g.cs", compilationUnit.GetText(Encoding.UTF8));
        });
    }
}
