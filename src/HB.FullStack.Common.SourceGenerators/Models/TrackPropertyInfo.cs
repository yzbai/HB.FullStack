// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using CommunityToolkit.Mvvm.SourceGenerators.Extensions;
using CommunityToolkit.Mvvm.SourceGenerators.Helpers;

namespace CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models;

/// <summary>
/// A model representing an generated property
/// </summary>
/// <param name="TypeNameWithNullabilityAnnotations">The type name for the generated property, including nullability annotations.</param>
/// <param name="FieldName">The field name.</param>
/// <param name="PropertyName">The generated property name.</param>
/// <param name="PropertyChangingNames">The sequence of property changing properties to notify.</param>
/// <param name="PropertyChangedNames">The sequence of property changed properties to notify.</param>
/// <param name="NotifiedCommandNames">The sequence of commands to notify.</param>
/// <param name="NotifyPropertyChangedRecipients">Whether or not the generated property also broadcasts changes.</param>
/// <param name="NotifyDataErrorInfo">Whether or not the generated property also validates its value.</param>
/// <param name="ForwardedAttributes">The sequence of forwarded attributes for the generated property.</param>
internal sealed record TrackPropertyInfo(
    string TypeNameWithNullabilityAnnotations,
    string FieldName,
    string PropertyName,
    bool IsNotified,
    ImmutableArray<AttributeInfo> ForwardedAttributes
    )
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="TrackPropertyInfo"/>.
    /// </summary>
    public sealed class Comparer : Comparer<TrackPropertyInfo, Comparer>
    {
        /// <inheritdoc/>
        protected override void AddToHashCode(ref HashCode hashCode, TrackPropertyInfo obj)
        {
            hashCode.Add(obj.TypeNameWithNullabilityAnnotations);
            hashCode.Add(obj.FieldName);
            hashCode.Add(obj.PropertyName);
            hashCode.Add(obj.IsNotified);
            hashCode.AddRange(obj.ForwardedAttributes, AttributeInfo.Comparer.Default);
        }

        /// <inheritdoc/>
        protected override bool AreEqual(TrackPropertyInfo x, TrackPropertyInfo y)
        {
            return
                x.TypeNameWithNullabilityAnnotations == y.TypeNameWithNullabilityAnnotations &&
                x.FieldName == y.FieldName &&
                x.PropertyName == y.PropertyName &&
                x.IsNotified == y.IsNotified &&
                x.ForwardedAttributes.SequenceEqual(y.ForwardedAttributes, AttributeInfo.Comparer.Default);
        }
    }
}
