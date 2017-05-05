#region Copyright
//  Copyright, Sascha Kiefer (esskar)
//  Released under LGPL License.
//  
//  License: https://raw.github.com/esskar/Serialize.Linq/master/LICENSE
//  Contributing: https://github.com/esskar/Serialize.Linq
#endregion
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Serialize.Linq.Factories;
using Serialize.Linq.Interfaces;
using Serialize.Linq.Nodes;

namespace Serialize.Linq.Extensions
{
    /// <summary>
    /// Expression externsions methods.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Converts an expression to an expression node.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="factorySettings">The factory settings to use.</param>
        /// <returns></returns>
        public static ExpressionNode ToExpressionNode(this Expression expression, FactorySettings factorySettings = null)
        {
            var factory = CreateFactory(expression, factorySettings);
            return factory.Create(expression);
        }
        
        public static INodeFactory CreateFactory(this Expression expression, FactorySettings factorySettings)
        {
            var lambda = expression as LambdaExpression;
            if (lambda != null)
                return new DefaultNodeFactory(lambda.Parameters.Select(p => p.Type), factorySettings);
            return new NodeFactory(factorySettings);
        }

        /// <summary>
        /// Converts an expression to an json encoded string.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="factorySettings">The factory settings to use.</param>
        /// <returns></returns>
        public static string ToJson(this Expression expression, FactorySettings factorySettings = null)
        {
            var expressionNode = expression.ToExpressionNode(factorySettings);
            return JsonConvert.SerializeObject(expressionNode, new ExpressionNodeJsonConverter());
        }
        
        /// <summary>
        /// Converts expression as json encoded string to Expression
        /// </summary>
        /// <param name="json"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Expression ToExpression(this string json, ExpressionContext context = null)
        {
            var expressionNode = JsonConvert.DeserializeObject<ExpressionNode>(json, new ExpressionNodeJsonConverter());
            return expressionNode?.ToExpression(context ?? new ExpressionContext());
        }
        
        /// <summary>
        /// Gets the default factory.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="factorySettings">The factory settings to use.</param>
        /// <returns></returns>
        internal static INodeFactory GetDefaultFactory(this Expression expression, FactorySettings factorySettings)
        {
            var lambda = expression as LambdaExpression;
            if(lambda != null)
                return  new DefaultNodeFactory(lambda.Parameters.Select(p => p.Type), factorySettings);
            return new NodeFactory(factorySettings);
        }

        /// <summary>
        /// Gets the link nodes of an expression tree.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        internal static IEnumerable<Expression> GetLinkNodes(this Expression expression)
        {
            if (expression is LambdaExpression)
            {
                var lambdaExpression = (LambdaExpression)expression;

                yield return lambdaExpression.Body;
                foreach (var parameter in lambdaExpression.Parameters)
                    yield return parameter;
            }
            else if (expression is BinaryExpression)
            {
                var binaryExpression = (BinaryExpression)expression;

                yield return binaryExpression.Left;
                yield return binaryExpression.Right;
            }
            else if (expression is ConditionalExpression)
            {
                var conditionalExpression = (ConditionalExpression) expression;

                yield return conditionalExpression.IfTrue;
                yield return conditionalExpression.IfFalse;
                yield return conditionalExpression.Test;
            }
            else if (expression is InvocationExpression)
            {
                var invocationExpression = (InvocationExpression)expression;
                yield return invocationExpression.Expression;
                foreach (var argument in invocationExpression.Arguments)
                    yield return argument;                
            }
            else if (expression is ListInitExpression)
            {
                yield return ((ListInitExpression) expression).NewExpression;
            }
            else if (expression is MemberExpression)
            {
                yield return ((MemberExpression) expression).Expression;
            }
            else if (expression is MemberInitExpression)
            {
                yield return ((MemberInitExpression) expression).NewExpression;
            }
            else if (expression is MethodCallExpression)
            {
                var methodCallExpression = (MethodCallExpression)expression;
                foreach (var argument in methodCallExpression.Arguments)
                    yield return argument;
                if (methodCallExpression.Object != null)                
                    yield return methodCallExpression.Object;                
            }
            else if (expression is NewArrayExpression)
            {
                foreach (var item in ((NewArrayExpression) expression).Expressions)
                    yield return item;
            }
            else if (expression is NewExpression)
            {
                foreach (var item in ((NewExpression) expression).Arguments)
                    yield return item;
            }
            else if (expression is TypeBinaryExpression)
            {
                yield return ((TypeBinaryExpression) expression).Expression;
            }
            else if (expression is UnaryExpression)
            {
                yield return ((UnaryExpression) expression).Operand;
            }
        }

        /// <summary>
        /// Gets the nodes of an expression tree.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        internal static IEnumerable<Expression> GetNodes(this Expression expression)
        {
            foreach (var node in expression.GetLinkNodes())
            {
                foreach (var subNode in node.GetNodes())
                    yield return subNode;
            }
            yield return expression;
        }

        /// <summary>
        /// Gets the nodes of an expression tree of given expression type.
        /// </summary>
        /// <typeparam name="TExpression">The type of the expression.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        internal static IEnumerable<TExpression> GetNodes<TExpression>(this Expression expression) where TExpression : Expression
        {
            return expression.GetNodes().OfType<TExpression>();
        }
    }
}