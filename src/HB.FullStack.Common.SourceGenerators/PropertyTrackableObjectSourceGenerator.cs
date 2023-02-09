using System.Collections.Immutable;
using System.Linq;

using CommunityToolkit.Mvvm.SourceGenerators;
using CommunityToolkit.Mvvm.SourceGenerators.Diagnostics;
using CommunityToolkit.Mvvm.SourceGenerators.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HB.FullStack.Common.SourceGenerators.PropertyTrackable
{
    [Generator(LanguageNames.CSharp)]

    public class PropertyTrackableObjectGenerator : TransitiveMembersGenerator<object?>
    {
        public PropertyTrackableObjectGenerator() : base("global::HB.FullStack.Common.PropertyTrackable.PropertyTrackableObjectAttribute") { }

        protected override object? ValidateTargetTypeAndGetInfo(INamedTypeSymbol typeSymbol, AttributeData? attributeData, Compilation compilation, out ImmutableArray<Diagnostic> diagnostics)
        {
            ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();

            // Check if the type already implements INotifyPropertyChanged...
            if (typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedName("global::HB.FullStack.Common.PropertyTrackable.IPropertyTrackableObject")))
            {
                builder.Add(DiagnosticDescriptors.DuplicateIPropertyTrackableObjectInterfaceForPropertyTrackableObjectAttributeError, typeSymbol, typeSymbol);

                goto End;
            }

        End:
            diagnostics = builder.ToImmutable();

            return null;
        }

        protected override ImmutableArray<MemberDeclarationSyntax> FilterDeclaredMembers(object? info, ImmutableArray<MemberDeclarationSyntax> memberDeclarations)
        {
            return memberDeclarations;
        }
    }
}
