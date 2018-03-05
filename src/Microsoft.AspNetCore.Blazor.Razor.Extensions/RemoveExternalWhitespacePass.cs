// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    internal class RemoveExternalWhitespacePass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
    {
        public static void Register(IRazorEngineBuilder configuration)
        {
            configuration.Features.Add(new RemoveExternalWhitespacePass());
        }

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var method = documentNode.FindPrimaryMethod();
            var applicableChildren = method.Children.Where(c =>
                c is HtmlContentIntermediateNode
                || c is CSharpCodeIntermediateNode
                || c is CSharpExpressionIntermediateNode);

            var leadingHtml = applicableChildren.FirstOrDefault() as HtmlContentIntermediateNode;
            if (leadingHtml != null)
            {
                RemoveContiguousWhitespaceNodes(leadingHtml.Children, 0, reverse: false);
            }

            var trailingHtml = applicableChildren.LastOrDefault() as HtmlContentIntermediateNode;
            if (trailingHtml != null)
            {
                RemoveContiguousWhitespaceNodes(trailingHtml.Children, trailingHtml.Children.Count - 1, reverse: true);
            }
        }

        private void RemoveContiguousWhitespaceNodes(IntermediateNodeCollection nodes, int startIndex, bool reverse)
        {
            while (startIndex >= 0 && startIndex < nodes.Count)
            {
                var node = nodes[startIndex];
                if (IsWhitespace(node))
                {
                    nodes.RemoveAt(startIndex);
                    if (reverse)
                    {
                        startIndex--;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool IsWhitespace(IntermediateNode node)
            => node is IntermediateToken token
            && string.IsNullOrWhiteSpace(token.Content);
    }
}
