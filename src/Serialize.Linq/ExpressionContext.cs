#region Copyright
//  Copyright, Sascha Kiefer (esskar)
//  Released under LGPL License.
//  
//  License: https://raw.github.com/esskar/Serialize.Linq/master/LICENSE
//  Contributing: https://github.com/esskar/Serialize.Linq
#endregion

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyModel;
using Serialize.Linq.Nodes;

namespace Serialize.Linq
{
    public class ExpressionContext
    {
        private readonly ConcurrentDictionary<string, ParameterExpression> _parameterExpressions;
        private readonly ConcurrentDictionary<string, Type> _typeCache;

        public ExpressionContext()
        {
            _parameterExpressions = new ConcurrentDictionary<string, ParameterExpression>();
            _typeCache = new ConcurrentDictionary<string, Type>();
        }

        public bool AllowPrivateFieldAccess { get; set; }

        public virtual BindingFlags? GetBindingFlags()
        {
            if (!AllowPrivateFieldAccess)
                return null;

            return BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        }

        public virtual ParameterExpression GetParameterExpression(ParameterExpressionNode node)
        {
            if(node == null)
                throw new ArgumentNullException("node");
            var key = node.Type.Name + Environment.NewLine + node.Name;
            return _parameterExpressions.GetOrAdd(key, k => Expression.Parameter(node.Type.ToType(this), node.Name));
        }

        public virtual Type ResolveType(TypeNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (string.IsNullOrWhiteSpace(node.Name))
                return null;

            return _typeCache.GetOrAdd(node.Name, typeName =>
            {
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    return type;
                }

#if NET46
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
#else
                foreach (var lib in DependencyContext.Default.RuntimeLibraries)
                {
                    foreach (var assemblyName in lib.Assemblies)
                    {
                        type = Assembly
                            .Load(assemblyName.Name)
                            .GetType(typeName);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                }
#endif



                throw new InvalidOperationException($"Could not resolve type: {typeName}");
            });
        }
    }
}
