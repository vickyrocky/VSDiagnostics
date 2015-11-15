﻿using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using VSDiagnostics.Utilities;

namespace VSDiagnostics.Diagnostics.Async.SyncMethodWithSyncSuffix
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SyncMethodWithAsyncSuffixAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = VSDiagnosticsResources.AsyncCategory;
        private static readonly string Message = VSDiagnosticsResources.SyncMethodWithSyncSuffixAnalyzerMessage;
        private static readonly string Title = VSDiagnosticsResources.SyncMethodWithSyncSuffixAnalyzerTitle;

        internal static DiagnosticDescriptor Rule
            =>
                new DiagnosticDescriptor(DiagnosticId.SyncMethodWithSyncSuffix, Title, Message, Category, Severity,
                    isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);
        }
        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var method = context.Node as MethodDeclarationSyntax;
            if (method == null)
            {
                return;
            }

            var returnType = context.SemanticModel.GetTypeInfo(method.ReturnType);
            if (returnType.Type == null)
            {
                return;
            }

            if (!(method.Modifiers.Any(SyntaxKind.AsyncKeyword) ||
                returnType.Type.MetadataName == typeof(Task).Name ||
                returnType.Type.MetadataName == typeof(Task<>).Name))
            {
                if (method.Identifier.Text.EndsWith("Async"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(),
                        method.Identifier.Text));
                }
            }
        }
    }
}
