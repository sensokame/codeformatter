﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under MIT. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    [Export(typeof(IFormattingRule))]
    internal sealed class HasNoNewLineBeforeEndBraceFormattingRule : IFormattingRule
    {
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken) as CSharpSyntaxNode;
            if (syntaxRoot == null)
                return document;

            var closeBraceTokens = syntaxRoot.DescendantTokens().Where((token) => token.CSharpKind() == SyntaxKind.CloseBraceToken);
            Func<SyntaxToken, SyntaxToken, SyntaxToken> replacementForTokens = (token, dummy) =>
            {
                var triviaItem = token.LeadingTrivia.First();
                int elementsToRemove = 1;
                if (token.LeadingTrivia.Count > 1)
                {
                    while (triviaItem == token.LeadingTrivia.ElementAt(elementsToRemove))
                        elementsToRemove++;
                }

                var newToken = token.WithLeadingTrivia(token.LeadingTrivia.Skip(elementsToRemove));
                return newToken;
            };

            var tokensToReplace = closeBraceTokens.Where((token) => token.HasLeadingTrivia && token.LeadingTrivia.First().CSharpKind() == SyntaxKind.EndOfLineTrivia);

            return document.WithSyntaxRoot(syntaxRoot.ReplaceTokens(tokensToReplace, replacementForTokens));
        }
    }
}
