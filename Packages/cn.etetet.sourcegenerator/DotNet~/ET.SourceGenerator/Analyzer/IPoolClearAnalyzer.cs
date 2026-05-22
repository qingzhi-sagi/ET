using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class IPoolClearAnalyzer: DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(IPoolClearAnalyzerRule.Rule);

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
            if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzeAssembly.All))
            {
                analysisContext.RegisterSymbolAction(this.Analyze, SymbolKind.NamedType);
            }
        });
    }

    private void Analyze(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        if (typeSymbol.TypeKind != TypeKind.Class || typeSymbol.IsAbstract)
        {
            return;
        }

        if (!IPoolClearGenerator.ImplementsIPoolDirectly(typeSymbol))
        {
            return;
        }

        if (IPoolClearGenerator.HasExplicitIPoolClear(typeSymbol))
        {
            return;
        }

        foreach (SyntaxReference declaringSyntaxReference in typeSymbol.DeclaringSyntaxReferences)
        {
            if (declaringSyntaxReference.GetSyntax(context.CancellationToken) is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                continue;
            }

            if (classDeclarationSyntax.IsPartial() && typeSymbol.ContainingType == null &&
                !IPoolClearGenerator.NeedsManualClear(typeSymbol))
            {
                return;
            }

            Diagnostic diagnostic = Diagnostic.Create(
                IPoolClearAnalyzerRule.Rule,
                classDeclarationSyntax.Identifier.GetLocation(),
                typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }
    }
}
