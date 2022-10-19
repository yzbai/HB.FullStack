// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;
using CommunityToolkit.Mvvm.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models;

/// <summary>
/// A model representing an attribute declaration.
/// </summary>
internal sealed record AttributeInfo(
    string TypeName,
    ImmutableArray<TypedConstantInfo> ConstructorArgumentInfo,
    ImmutableArray<(string Name, TypedConstantInfo Value)> NamedArgumentInfo)
{
    /// <summary>
    /// Creates a new <see cref="AttributeInfo"/> instance from a given <see cref="AttributeData"/> value.
    /// </summary>
    /// <param name="attributeData">The input <see cref="AttributeData"/> value.</param>
    /// <returns>A <see cref="AttributeInfo"/> instance representing <paramref name="attributeData"/>.</returns>
    public static AttributeInfo From(AttributeData attributeData)
    {
        string typeName = attributeData.AttributeClass!.GetFullyQualifiedName();

        // Get the constructor arguments
        ImmutableArray<TypedConstantInfo> constructorArguments =
            attributeData.ConstructorArguments
            .Select(TypedConstantInfo.From)
            .ToImmutableArray();

        // Get the named arguments
        ImmutableArray<(string, TypedConstantInfo)>.Builder namedArguments = ImmutableArray.CreateBuilder<(string, TypedConstantInfo)>();

        foreach (KeyValuePair<string, TypedConstant> arg in attributeData.NamedArguments)
        {
            namedArguments.Add((arg.Key, TypedConstantInfo.From(arg.Value)));
        }

        return new(
            typeName,
            constructorArguments,
            namedArguments.ToImmutable());
    }

    /// <summary>
    /// Gets an <see cref="AttributeSyntax"/> instance representing the current value.
    /// </summary>
    /// <returns>The <see cref="ExpressionSyntax"/> instance representing the current value.</returns>
    public AttributeSyntax GetSyntax()
    {
        // Gather the constructor arguments
        IEnumerable<AttributeArgumentSyntax> arguments =
            ConstructorArgumentInfo
            .Select(static arg => AttributeArgument(arg.GetSyntax()));

        // Gather the named arguments
        IEnumerable<AttributeArgumentSyntax> namedArguments =
            NamedArgumentInfo.Select(static arg =>
                AttributeArgument(arg.Value.GetSyntax())
                .WithNameEquals(NameEquals(IdentifierName(arg.Name))));

        return Attribute(IdentifierName(TypeName), AttributeArgumentList(SeparatedList(arguments.Concat(namedArguments))));

    }

    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="AttributeInfo"/>.
    /// </summary>
    public sealed class Comparer : Comparer<AttributeInfo, Comparer>
    {
        /// <inheritdoc/>
        protected override void AddToHashCode(ref HashCode hashCode, AttributeInfo obj)
        {
            hashCode.Add(obj.TypeName);
            hashCode.AddRange(obj.ConstructorArgumentInfo, TypedConstantInfo.Comparer.Default);

            foreach ((string key, TypedConstantInfo value) in obj.NamedArgumentInfo)
            {
                hashCode.Add(key);
                hashCode.Add(value, TypedConstantInfo.Comparer.Default);
            }
        }

        /// <inheritdoc/>
        protected override bool AreEqual(AttributeInfo x, AttributeInfo y)
        {
            if (x.TypeName != y.TypeName ||
                !x.ConstructorArgumentInfo.SequenceEqual(y.ConstructorArgumentInfo, TypedConstantInfo.Comparer.Default) ||
                x.NamedArgumentInfo.Length != y.NamedArgumentInfo.Length)
            {
                return false;
            }

            for (int i = 0; i < x.NamedArgumentInfo.Length; i++)
            {
                (string Name, TypedConstantInfo Value) left = x.NamedArgumentInfo[i];
                (string Name, TypedConstantInfo Value) right = y.NamedArgumentInfo[i];

                if (left.Name != right.Name ||
                    !TypedConstantInfo.Comparer.Default.Equals(left.Value, right.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
