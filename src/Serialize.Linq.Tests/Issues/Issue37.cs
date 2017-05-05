using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Serialize.Linq.Extensions;
using Serialize.Linq.Tests.Internals;

namespace Serialize.Linq.Tests.Issues
{
    [TestClass]
    public class Issue37
    {
        [TestMethod]
        public void DynamicsTests()
        {
            var expressions = new List<Expression>();

            Expression<Func<Item, dynamic>> objectExp = item => new {item.Name, item.ProductId};
            Expression<Func<string, dynamic>> stringExp = str => new { Text = str };

            expressions.Add(objectExp);
            expressions.Add(stringExp);

            foreach (var expected in expressions)
            {
                var actual = expected.ToJson().ToExpression();

                ExpressionAssert.AreEqual(expected, actual);
            }
        }

        public class Item
        {
            public string Name { get; set; }

            public string ProductId { get; set; }
        }
    }
}
