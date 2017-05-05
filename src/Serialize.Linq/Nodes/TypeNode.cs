#region Copyright
//  Copyright, Sascha Kiefer (esskar)
//  Released under LGPL License.
//  
//  License: https://raw.github.com/esskar/Serialize.Linq/master/LICENSE
//  Contributing: https://github.com/esskar/Serialize.Linq
#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Serialize.Linq.Interfaces;

namespace Serialize.Linq.Nodes
{
    #region DataContract
#if !SERIALIZE_LINQ_OPTIMIZE_SIZE
    [DataContract]
#else
    [DataContract(Name = "T")]
#endif
    #endregion
    public class TypeNode : Node
    {        
        public TypeNode() { }

        public TypeNode(INodeFactory factory, Type type)
            : base(factory)
        {
            Initialize(type);
        }

        private void Initialize(Type type)
        {
            if (type == null)
                return;

            if (type.GetTypeInfo().IsGenericType)
            {
                GenericArguments = type.GetTypeInfo().GetGenericArguments().Select(t => new TypeNode(Factory, t)).ToArray();
                var typeDefinition = type.GetGenericTypeDefinition();
                Name = typeDefinition.AssemblyQualifiedName;
            }
            else
            {
                Name = type.AssemblyQualifiedName;
            }            
        }

        #region DataMember
#if !SERIALIZE_LINQ_OPTIMIZE_SIZE
        [DataMember(EmitDefaultValue = false)]
#else
        [DataMember(EmitDefaultValue = false, Name = "N")]        
#endif
        #endregion
        public string Name { get; set; }

        #region DataMember
#if !SERIALIZE_LINQ_OPTIMIZE_SIZE
        [DataMember(EmitDefaultValue = false)]
#else
        [DataMember(EmitDefaultValue = false, Name = "G")]        
#endif
        #endregion
        public TypeNode[] GenericArguments { get; set; }
        
        public Type ToType(ExpressionContext context)
        {
            var type = context.ResolveType(this);
            if (type == null)
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return null;
                throw new SerializationException(string.Format("Failed to serialize '{0}' to a type object.", Name));
            }

            if (GenericArguments != null)            
                type = type.MakeGenericType(GenericArguments.Select(t => t.ToType(context)).ToArray());
            
            return type;
        }
    }
}