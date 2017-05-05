#region Copyright
//  Copyright, Sascha Kiefer (esskar)
//  Released under LGPL License.
//  
//  License: https://raw.github.com/esskar/Serialize.Linq/master/LICENSE
//  Contributing: https://github.com/esskar/Serialize.Linq
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serialize.Linq.Extensions;
using Serialize.Linq.Tests.Internals;

namespace Serialize.Linq.Tests
{
    [TestClass]
    public class ExpressionSerializerTests
    {
        public TestContext TestContext { get; set; }
        

        [TestMethod]
        public void SerializeDeserializeTextTest()
        {
            foreach (var expected in SerializerTestData.TestExpressions)
            {
                var text = expected.ToJson();

                this.TestContext.WriteLine("{0} serializes to text with length {1}: {2}", expected, text.Length, text);

                var actual = text.ToExpression();

                if (expected == null)
                {
                    Assert.IsNull(actual, "Input expression was null, but output is {0}", actual);
                    continue;
                }
                Assert.IsNotNull(actual, "Input expression was {0}, but output is null", expected);
                ExpressionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void SerializeDeserializeComplexExpressionWithCompileTest()
        {
            var expected = (Expression<Func<Bar, bool>>)(p => p.LastName == "Miller" && p.FirstName.StartsWith("M"));
            expected.Compile();

            var json = expected.ToJson();

            var actual = (Expression<Func<Bar, bool>>)json.ToExpression();
            Assert.IsNotNull(actual, "Input expression was {0}, but output is null", expected);
            ExpressionAssert.AreEqual(expected, actual);

            actual.Compile();
        }

        [TestMethod]
        public void NullableDecimalTest()
        {
            var expected = Expression.Constant(0m, typeof(Decimal?));

            var text = expected.ToJson();

            this.TestContext.WriteLine("{0} serializes to text with length {1}: {2}", expected, text.Length, text);

            var actual = text.ToExpression();
            Assert.IsNotNull(actual, "Input expression was {0}, but output is null for '{1}'", expected);
            ExpressionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SerializeNewObjWithoutParameters()
        {
            Expression<Func<List<int>, List<int>>> exp = l => new List<int>();

            var result = exp.ToJson();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SerializeFuncExpressionsWithoutParameters()
        {
            Expression<Func<bool>> exp = () => false;

            var result = exp.ToJson();
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void SerializeDeserializeGuidValueAsJson()
        {
            SerializeDeserializeExpressionAsText(CreateGuidExpression());
        }
        
        [TestMethod]
        public void ExpressionWithConstantDateTimeAsJson()
        {
            SerializeDeserializeExpressionAsText(CreateConstantDateTimeExpression());
        }
        
        [TestMethod]
        public void ExpressionWithConstantTypeAsJson()
        {
            SerializeDeserializeExpressionAsText(CreateConstantTypeExpression());
        }
        
        private static ConstantExpression CreateConstantDateTimeExpression()
        {
            return Expression.Constant(DateTime.Today);
        }

        private static Expression<Func<Guid>> CreateGuidExpression()
        {
            var guidValue = Guid.NewGuid();
            return () => guidValue;
        }

        private static ConstantExpression CreateConstantTypeExpression()
        {
            return Expression.Constant(typeof(string));
        }

        private static Expression SerializeDeserializeExpressionAsText(Expression expression)
        {
            return expression.ToJson().ToExpression();
        }
        
        
    }
}
