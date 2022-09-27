// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HB.FullStack.Common.SourceGenerators.PropertyTrackable;

using Microsoft.CodeAnalysis;

namespace CommunityToolkit.Mvvm.SourceGenerators.Diagnostics;

/// <summary>
/// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
/// </summary>
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor DuplicateIPropertyTrackableObjectInterfaceForPropertyTrackableObjectAttributeError = new DiagnosticDescriptor(
        id: "PT0001",
        title: $"Duplicate IPropertyTrackableObject definition",
        messageFormat: $"Cannot apply [PropertyTrackableObject] to type {{0}}, as it already declares the IPropertyTrackableObject interface",
        category: typeof(PropertyTrackableObjectGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: $"Cannot apply [PropertyTrackableObject] to a type that already declares the IPropertyTrackableObject interface.",
        helpLinkUri: "https://aka.ms/mvvmtoolkit/errors/mvvmtk0001");

    public static readonly DiagnosticDescriptor InvalidContainingTypeForTrackPropertyFieldError = new DiagnosticDescriptor(
        id: "PT0002",
        title: "Invalid containing type for [TrackProperty] field",
        messageFormat: "The field {0}.{1} cannot be used to generate an TrackProperty property, as its containing type doesn't inherit from PropertyTrackableObject, nor does it use [PropertyTrackableObject]",
        category: typeof(TrackPropertyGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Fields annotated with [TrackProperty] must be contained in a type that inherits from PropertyTrackableObject or that is annotated with [PropertyTrackableObject].",
        helpLinkUri: "https://aka.ms/mvvmtoolkit/errors/mvvmtk0019");

    public static readonly DiagnosticDescriptor TrackPropertyNameCollisionError = new DiagnosticDescriptor(
        id: "PT0003",
        title: "Name collision for generated property",
        messageFormat: "The field {0}.{1} cannot be used to generate an trackproperty property, as its name would collide with the field name (instance fields should use the \"lowerCamel\", \"_lowerCamel\" or \"m_lowerCamel\" pattern)",
        category: typeof(TrackPropertyGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The name of fields annotated with [TrackProperty] should use \"lowerCamel\", \"_lowerCamel\" or \"m_lowerCamel\" pattern to avoid collisions with the generated properties.",
        helpLinkUri: "https://aka.ms/mvvmtoolkit/errors/mvvmtk0014");

    public static readonly DiagnosticDescriptor InvalidTrackPropertyError = new DiagnosticDescriptor(
       id: "PT0004",
       title: "Invalid generated property declaration",
       messageFormat: "The field {0}.{1} cannot be used to generate an trackproperty property, as it should be value-type or string, or inherit INotifyPropertyChanging and INotifyPropertyChanged",
       category: typeof(TrackPropertyGenerator).FullName,
       defaultSeverity: DiagnosticSeverity.Error,
       isEnabledByDefault: true,
       description: "The fields be either valuetype or string or any type inherint INotifyPropertyChanging and INotifyPropertyChanged.",
       helpLinkUri: "https://aka.ms/mvvmtoolkit/errors/mvvmtk0024");

}