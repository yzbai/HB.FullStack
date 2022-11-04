// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

using CommunityToolkit.Mvvm.SourceGenerators.ComponentModel.Models;
using CommunityToolkit.Mvvm.SourceGenerators.Diagnostics;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HB.FullStack.Common.SourceGenerators.PropertyTrackable;

/// <inheritdoc/>
partial class TrackPropertyGenerator
{
    /// <summary>
    /// A container for all the logic for <see cref="TrackPropertyGenerator"/>.
    /// </summary>
    internal static class Execute
    {

        /// <summary>
        /// Processes a given field.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
        /// <param name="diagnostics">The resulting diagnostics from the processing operation.</param>
        /// <returns>The resulting <see cref="TrackPropertyInfo"/> instance for <paramref name="fieldSymbol"/>, if successful.</returns>
        public static TrackPropertyInfo? TryGetPropertyInfo(IFieldSymbol fieldSymbol, out ImmutableArray<Diagnostic> diagnostics)
        {
            ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();

            // Validate the target type
            if (!IsTargetTypeValid(fieldSymbol))
            {
                builder.Add(
                    DiagnosticDescriptors.InvalidContainingTypeForTrackPropertyFieldError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);

                diagnostics = builder.ToImmutable();

                return null;
            }

            // Get the property type and name
            string typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
            string fieldName = fieldSymbol.Name;
            string propertyName = GetGeneratedPropertyName(fieldSymbol);

            // Check for name collisions
            if (fieldName == propertyName)
            {
                builder.Add(
                    DiagnosticDescriptors.TrackPropertyNameCollisionError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);

                diagnostics = builder.ToImmutable();

                // If the generated property would collide, skip generating it entirely. This makes sure that
                // users only get the helpful diagnostic about the collision, and not the normal compiler error
                // about a definition for "Property" already existing on the target type, which might be confusing.
                return null;
            }

            // Check for special cases that are explicitly not allowed
            if (!IsImmutableProperty(propertyName, fieldSymbol.Type))
            {
                builder.Add(
                    DiagnosticDescriptors.InvalidTrackPropertyError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);

                diagnostics = builder.ToImmutable();

                return null;
            }

            diagnostics = builder.ToImmutable();

            bool hasNotified = fieldSymbol.Type.HasInterfaceWithFullyQualifiedName("global::System.ComponentModel.INotifyPropertyChanging");

            ImmutableArray<AttributeInfo>.Builder forwardedAttributes = ImmutableArray.CreateBuilder<AttributeInfo>();

            // Gather attributes info
            foreach (AttributeData attributeData in fieldSymbol.GetAttributes())
            {

                if (attributeData.AttributeClass?.HasFullyQualifiedName("global::HB.FullStack.Common.PropertyTrackable.TrackPropertyAttribute") == true)
                {
                    continue;
                }

                forwardedAttributes.Add(AttributeInfo.From(attributeData));
            }

            return new(
                typeNameWithNullabilityAnnotations,
                fieldName,
                propertyName,
                hasNotified,
                forwardedAttributes.ToImmutable());
        }

        /// <summary>
        /// Validates the containing type for a given field being annotated.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
        /// <param name="shouldInvokeOnPropertyChanging">Whether or not property changing events should also be raised.</param>
        /// <returns>Whether or not the containing type for <paramref name="fieldSymbol"/> is valid.</returns>
        private static bool IsTargetTypeValid(IFieldSymbol fieldSymbol)
        {

            bool isObservableObject = fieldSymbol.ContainingType.InheritsFromFullyQualifiedName("global::HB.FullStack.Common.PropertyTrackable.IPropertyTrackableObject");
            bool hasObservableObjectAttribute = fieldSymbol.ContainingType.HasOrInheritsAttributeWithFullyQualifiedName("global::HB.FullStack.Common.PropertyTrackable.PropertyTrackableObjectAttribute");

            return isObservableObject || hasObservableObjectAttribute;
        }

        /// <summary>
        /// 必须是immutable才行
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="propertyType">The property type.</param>
        /// <returns>Whether the generated property is invalid.</returns>
        private static bool IsImmutableProperty(string propertyName, ITypeSymbol propertyType)
        {
            // If the generated property name is called "Property" and the type is either object or it is PropertyChangedEventArgs or
            // PropertyChangingEventArgs (or a type derived from either of those two types), consider it invalid. This is needed because
            // if such a property was generated, the partial On<PROPERTY_NAME>Changing and OnPropertyChanging(PropertyChangingEventArgs)
            // methods, as well as the partial On<PROPERTY_NAME>Changed and OnPropertyChanged(PropertyChangedEventArgs) methods.

            if(propertyType.IsReadOnly)
            {
                return true;
            }

            //1. ValueType or String
            if (propertyType.IsValueType || propertyType.SpecialType == SpecialType.System_String)
            {
                return true;
            }

            //2. Immutable Collection
            if (propertyType.HasInterfaceWithFullyQualifiedName("global::System.Collections.IEnumerable"))
            {
                return propertyType.HasUnderNamespace("System.Collections.Immutable");
                //return propertyType.HasInterfaceWithFullyQualifiedNameWithoutGenericParameter("global::System.Collections.Generic.IReadOnlyCollection");
            }

            //3. all properties are init-only or record
            //TODO: 这里看不到由其他SourceGeneration生成的属性。
            //所以考察所有属性都是init-only不成行。该考察是record

            return propertyType.IsRecord;
            //var memebers = propertyType.GetMembers();
            //var setters = propertyType.GetMembers().Where(m => m is IMethodSymbol mm && mm.MethodKind == MethodKind.PropertySet);


            //if (setters.All(m => m is IMethodSymbol mm && mm.IsInitOnly))
            //{
            //    return true;
            //}

            //if (propertyType.HasInterfaceWithFullyQualifiedName("global::System.ComponentModel.INotifyPropertyChanging")
            //    && propertyType.HasInterfaceWithFullyQualifiedName("global::System.ComponentModel.INotifyPropertyChanged"))
            //{
            //    return true;
            //}

            //return false;
        }

        /// <summary>
        /// Gets the <see cref="MemberDeclarationSyntax"/> instance for the input field.
        /// </summary>
        /// <param name="propertyInfo">The input <see cref="TrackPropertyInfo"/> instance to process.</param>
        /// <returns>The generated <see cref="MemberDeclarationSyntax"/> instance for <paramref name="propertyInfo"/>.</returns>
        public static MemberDeclarationSyntax GeneratePropertySyntax(TrackPropertyInfo propertyInfo)
        {
            ImmutableArray<StatementSyntax>.Builder setterStatements = ImmutableArray.CreateBuilder<StatementSyntax>();

            // Get the property type syntax
            TypeSyntax propertyType = IdentifierName(propertyInfo.TypeNameWithNullabilityAnnotations);

            // In case the backing field is exactly named "value", we need to add the "this." prefix to ensure that comparisons and assignments
            // with it in the generated setter body are executed correctly and without conflicts with the implicit value parameter.
            ExpressionSyntax fieldExpression = propertyInfo.FieldName switch
            {
                "value" => MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName("value")),
                string name => IdentifierName(name)
            };

            if (propertyInfo.IsNotified)
            {
                //if (<FieldName> is INotifyPropertyChanging oldChanging)
                //{
                //    oldChanging.PropertyChanging -= <PropertyName>_PropertyChanging;
                //}
                ImmutableArray<StatementSyntax>.Builder if1TrueStatement = ImmutableArray.CreateBuilder<StatementSyntax>();

                if1TrueStatement.Add(ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SubtractAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("oldChanging"), IdentifierName("PropertyChanging")),
                        IdentifierName($"{propertyInfo.PropertyName}_PropertyChanging"))));

                IfStatementSyntax if1 = IfStatement(
                    Token(SyntaxKind.IfKeyword),
                    Token(SyntaxKind.OpenParenToken),
                    IsPatternExpression(
                        fieldExpression,
                        Token(SyntaxKind.IsKeyword),
                        DeclarationPattern(IdentifierName("global::System.ComponentModel.INotifyPropertyChanging"), SingleVariableDesignation(Identifier("oldChanging")))),
                    Token(SyntaxKind.CloseParenToken),
                    Block(if1TrueStatement), null);

                //if (<FieldName> is INotifyPropertyChanged oldChanged)
                //{
                //    oldChanged.PropertyChanged -= <PropertyName>_PropertyChanged;
                //}
                ImmutableArray<StatementSyntax>.Builder if2TrueStatement = ImmutableArray.CreateBuilder<StatementSyntax>();

                if2TrueStatement.Add(ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SubtractAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("oldChanged"), IdentifierName("PropertyChanged")),
                        IdentifierName($"{propertyInfo.PropertyName}_PropertyChanged"))));

                IfStatementSyntax if2 = IfStatement(
                    Token(SyntaxKind.IfKeyword),
                    Token(SyntaxKind.OpenParenToken),
                    IsPatternExpression(
                        fieldExpression,
                        Token(SyntaxKind.IsKeyword),
                        DeclarationPattern(IdentifierName("global::System.ComponentModel.INotifyPropertyChanged"), SingleVariableDesignation(Identifier("oldChanged")))),
                    Token(SyntaxKind.CloseParenToken),
                    Block(if2TrueStatement), null);

                //if (value is INotifyPropertyChanging newChanging)
                //{
                //    newChanging.PropertyChanging += <PropertyName>_PropertyChanging;
                //}
                ImmutableArray<StatementSyntax>.Builder if3TrueStatement = ImmutableArray.CreateBuilder<StatementSyntax>();

                if3TrueStatement.Add(ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.AddAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("newChanging"), IdentifierName("PropertyChanging")),
                        IdentifierName($"{propertyInfo.PropertyName}_PropertyChanging"))));

                IfStatementSyntax if3 = IfStatement(
                    Token(SyntaxKind.IfKeyword),
                    Token(SyntaxKind.OpenParenToken),
                    IsPatternExpression(
                        IdentifierName("value"),
                        Token(SyntaxKind.IsKeyword),
                        DeclarationPattern(IdentifierName("global::System.ComponentModel.INotifyPropertyChanging"), SingleVariableDesignation(Identifier("newChanging")))),
                    Token(SyntaxKind.CloseParenToken),
                    Block(if3TrueStatement), null);

                //if (value is INotifyPropertyChanged newChanged)
                //{
                //    newChanged.PropertyChanged += <PropertyName>_PropertyChanged;
                //}

                ImmutableArray<StatementSyntax>.Builder if4TrueStatement = ImmutableArray.CreateBuilder<StatementSyntax>();

                if4TrueStatement.Add(ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.AddAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("newChanged"), IdentifierName("PropertyChanged")),
                        IdentifierName($"{propertyInfo.PropertyName}_PropertyChanged"))));

                IfStatementSyntax if4 = IfStatement(
                    Token(SyntaxKind.IfKeyword),
                    Token(SyntaxKind.OpenParenToken),
                    IsPatternExpression(
                        IdentifierName("value"),
                        Token(SyntaxKind.IsKeyword),
                        DeclarationPattern(IdentifierName("global::System.ComponentModel.INotifyPropertyChanged"), SingleVariableDesignation(Identifier("newChanged")))),
                    Token(SyntaxKind.CloseParenToken),
                    Block(if4TrueStatement), null);

                setterStatements.AddRange(if1, if2, if3, if4);
            }

            setterStatements.Add(
             ExpressionStatement(
                 InvocationExpression(IdentifierName("SetAndTrackProperty"))
                 .AddArgumentListArguments(
                     Argument(null, Token(SyntaxKind.RefKeyword), fieldExpression),
                     Argument(IdentifierName("value")))));

            // Prepare the forwarded attributes, if any
            ImmutableArray<AttributeListSyntax> forwardedAttributes =
                propertyInfo.ForwardedAttributes
                .Select(static a => AttributeList(SingletonSeparatedList(a.GetSyntax())))
                .ToImmutableArray();

            // Construct the generated property as follows:
            //
            // /// <inheritdoc cref="<FIELD_NAME>"/>
            // [global::System.CodeDom.Compiler.GeneratedCode("...", "...")]
            // [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
            // <FORWARDED_ATTRIBUTES>
            // public <FIELD_TYPE><NULLABLE_ANNOTATION?> <PROPERTY_NAME>
            // {
            //     get => <FIELD_NAME>;
            //     set
            //     {
            //         <BODY>
            //     }
            // }
            return
                PropertyDeclaration(propertyType, Identifier(propertyInfo.PropertyName))
                .AddAttributeLists(
                    AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName("global::System.CodeDom.Compiler.GeneratedCode"))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(TrackPropertyGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(TrackPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                    .WithOpenBracketToken(Token(TriviaList(Comment($"/// <inheritdoc cref=\"{propertyInfo.FieldName}\"/>")), SyntaxKind.OpenBracketToken, TriviaList())),
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage")))))
                .AddAttributeLists(forwardedAttributes.ToArray())
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithExpressionBody(ArrowExpressionClause(IdentifierName(propertyInfo.FieldName)))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(Block(setterStatements)));
        }

        /// <summary>
        /// Gets the <see cref="MemberDeclarationSyntax"/> instances for the <c>OnPropertyChanging</c> and <c>OnPropertyChanged</c> methods for the input field.
        /// </summary>
        /// <param name="propertyInfo">The input <see cref="TrackPropertyInfo"/> instance to process.</param>
        /// <returns>The generated <see cref="MemberDeclarationSyntax"/> instances for the <c>OnPropertyChanging</c> and <c>OnPropertyChanged</c> methods.</returns>
        public static ImmutableArray<MemberDeclarationSyntax> GetTrackProperyTwoMethodsSyntax(TrackPropertyInfo propertyInfo)
        {
            if (!propertyInfo.IsNotified)
            {
                return ImmutableArray<MemberDeclarationSyntax>.Empty;
            }

            // Get the property type syntax
            TypeSyntax parameterType = IdentifierName(propertyInfo.TypeNameWithNullabilityAnnotations);

            TypeSyntax propertyChangingEventArgsType = IdentifierName("global::System.ComponentModel.PropertyChangingEventArgs");
            TypeSyntax propertyChangedEventArgsType = IdentifierName("global::System.ComponentModel.PropertyChangedEventArgs");
            // Construct the generated method as follows:
            //
            // /// <summary>Executes the logic for when <see cref="<PROPERTY_NAME>"/> is changing.</summary>
            // [global::System.CodeDom.Compiler.GeneratedCode("...", "...")]
            // partial void On<PROPERTY_NAME>Changing(<PROPERTY_TYPE> value);

            ImmutableArray<StatementSyntax>.Builder propertyChangingBody = ImmutableArray.CreateBuilder<StatementSyntax>();

            propertyChangingBody.Add(ExpressionStatement(
                InvocationExpression(IdentifierName("TrackOldValue"))
                .AddArgumentListArguments(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"{propertyInfo.PropertyName}"))),
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("e"), IdentifierName("PropertyName"))),
                    Argument(IdentifierName($"{propertyInfo.FieldName}"))
                    )));

            MemberDeclarationSyntax propertyChangingDeclaration =
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier($"{propertyInfo.PropertyName}_PropertyChanging"))
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("sender")).WithType(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))),
                    Parameter(Identifier("e")).WithType(propertyChangingEventArgsType))
                .AddAttributeLists(
                    AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName("global::System.CodeDom.Compiler.GeneratedCode"))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(TrackPropertyGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(TrackPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                    .WithOpenBracketToken(Token(TriviaList(Comment($"/// <summary>Executes the logic for when <see cref=\"{propertyInfo.PropertyName}\"/> is changing.</summary>")), SyntaxKind.OpenBracketToken, TriviaList())))
                .WithBody(Block(propertyChangingBody));

            // Construct the generated method as follows:
            //
            // /// <summary>Executes the logic for when <see cref="<PROPERTY_NAME>"/> ust changed.</summary>
            // [global::System.CodeDom.Compiler.GeneratedCode("...", "...")]
            // partial void On<PROPERTY_NAME>Changed(<PROPERTY_TYPE> value);

            ImmutableArray<StatementSyntax>.Builder propertyChangedBody = ImmutableArray.CreateBuilder<StatementSyntax>();

            propertyChangedBody.Add(ExpressionStatement(
                InvocationExpression(IdentifierName("TrackNewValue"))
                .AddArgumentListArguments(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"{propertyInfo.PropertyName}"))),
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("e"), IdentifierName("PropertyName"))),
                    Argument(IdentifierName($"{propertyInfo.FieldName}"))
                    )));

            MemberDeclarationSyntax propertyChangedDeclaration =
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier($"{propertyInfo.PropertyName}_PropertyChanged"))
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("sender")).WithType(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))),
                    Parameter(Identifier("e")).WithType(propertyChangedEventArgsType))
                .AddAttributeLists(
                    AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName("global::System.CodeDom.Compiler.GeneratedCode"))
                        .AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(TrackPropertyGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(TrackPropertyGenerator).Assembly.GetName().Version.ToString()))))))
                    .WithOpenBracketToken(Token(TriviaList(Comment($"/// <summary>Executes the logic for when <see cref=\"{propertyInfo.PropertyName}\"/> just changed.</summary>")), SyntaxKind.OpenBracketToken, TriviaList())))
                .WithBody(Block(propertyChangedBody));

            return ImmutableArray.Create(propertyChangingDeclaration, propertyChangedDeclaration);
        }

        /// <summary>
        /// Get the generated property name for an input field.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IFieldSymbol"/> instance to process.</param>
        /// <returns>The generated property name for <paramref name="fieldSymbol"/>.</returns>
        public static string GetGeneratedPropertyName(IFieldSymbol fieldSymbol)
        {
            string propertyName = fieldSymbol.Name;

            if (propertyName.StartsWith("m_"))
            {
                propertyName = propertyName.Substring(2);
            }
            else if (propertyName.StartsWith("_"))
            {
                propertyName = propertyName.TrimStart('_');
            }

            return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";
        }
    }
}
