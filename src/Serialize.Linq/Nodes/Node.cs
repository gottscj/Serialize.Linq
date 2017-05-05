#region Copyright
//  Copyright, Sascha Kiefer (esskar)
//  Released under LGPL License.
//  
//  License: https://raw.github.com/esskar/Serialize.Linq/master/LICENSE
//  Contributing: https://github.com/esskar/Serialize.Linq
#endregion

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Serialize.Linq.Interfaces;

namespace Serialize.Linq.Nodes
{
    /// <summary>
    /// 
    /// </summary>
    #region DataContract
    [DataContract]
    #endregion
    public abstract class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        protected Node() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <exception cref="System.ArgumentNullException">factory</exception>
        protected Node(INodeFactory factory)
        {
            if(factory == null)
                throw new ArgumentNullException("factory");

            Factory = factory;
        }
        
        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        [IgnoreDataMember]
        public readonly INodeFactory Factory;        
    }
}
