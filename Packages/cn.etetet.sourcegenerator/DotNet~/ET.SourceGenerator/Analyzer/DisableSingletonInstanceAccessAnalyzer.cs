using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DisableSingletonInstanceAccessAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
                ImmutableArray.Create(DisableSingletonInstanceAccessAnalyzerRule.Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(analysisContext =>
            {
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzeAssembly.AllModelHotfix))
                {
                    analysisContext.RegisterSemanticModelAction(this.AnalyzeSemanticModel);
                }
            });
        }

        private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            foreach (MemberAccessExpressionSyntax memberAccessExpressionSyntax in
                     context.SemanticModel.SyntaxTree.GetRoot().DescendantNodes<MemberAccessExpressionSyntax>())
            {
                this.AnalyzeMemberAccess(context, memberAccessExpressionSyntax);
            }
        }

        private void AnalyzeMemberAccess(SemanticModelAnalysisContext context, MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            if (memberAccessExpressionSyntax.Name.Identifier.Text != "Instance")
            {
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol is not IPropertySymbol propertySymbol)
            {
                return;
            }

            if (!propertySymbol.IsStatic || propertySymbol.Name != "Instance")
            {
                return;
            }

            if (context.SemanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression).Type is not INamedTypeSymbol targetTypeSymbol)
            {
                return;
            }

            if (!IsSingletonType(targetTypeSymbol))
            {
                return;
            }

            if (targetTypeSymbol.HasAttributeInTypeAndBaseTyes(Definition.AllowInstanceAttribute))
            {
                return;
            }

            Diagnostic diagnostic = Diagnostic.Create(
                DisableSingletonInstanceAccessAnalyzerRule.Rule,
                memberAccessExpressionSyntax.Name.Identifier.GetLocation(),
                targetTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsSingletonType(INamedTypeSymbol typeSymbol)
        {
            for (INamedTypeSymbol? current = typeSymbol; current != null; current = current.BaseType)
            {
                if (!current.IsGenericType)
                {
                    continue;
                }

                string originalType = current.OriginalDefinition.ToDisplayString();
                if (originalType == "ET.Singleton<T>" || originalType == "ET.EnumSingleton<T>")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
