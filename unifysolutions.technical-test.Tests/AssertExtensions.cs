using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using unifysolutions.technical_test.Client;

namespace unifysolutions.technical_test.Tests
{
    public static class AssertExtensions
    {
        /// <summary>
        /// Confirm that collection is sorted as expected for a single property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assert"></param>
        /// <param name="actualCollection">Collection to be tested</param>
        /// <param name="propertyLambda">Expected order by property</param>
        /// <param name="expectedSortOrder">Expected sort order of the collection</param>
        /// <exception cref="ArgumentException">Thrown if the expression passed is not a valid property of type string</exception>
        public static void CollectionIsInExpectedOrderBasedOnStringProperty<T>(this Assert assert, IEnumerable<T> actualCollection,
            Expression<Func<T, object>> propertyLambda, SortOrder expectedSortOrder)
        {
            
            if (actualCollection == null || !actualCollection.Any())
            {
                return;
            }

            var propertyInfo = GetPropertyInfo(propertyLambda);
            if (propertyInfo.PropertyType != typeof(string))
            {
                throw new ArgumentException("The lambda expression 'propertyLambda' should point to a property of type string");
            }
            else
            {
                var previousValue = (string)propertyInfo.GetValue(actualCollection.First(), null);
                foreach (var nextItem in actualCollection.Skip(1))
                {
                    var nextValue = (string)propertyInfo.GetValue(nextItem, null);
                    AssertItemOrder(expectedSortOrder, nextValue, previousValue);
                    previousValue = nextValue;
                }
            }
        }

        private static void AssertItemOrder(SortOrder expectedSortOrder, string? nextValue, string? previousValue)
        {
            if (expectedSortOrder == SortOrder.Descending)
            {
                Assert.IsTrue(nextValue.CompareTo(previousValue) <= 0);
            }
            else
            {
                Assert.IsTrue(nextValue.CompareTo(previousValue) >= 0);
            }
        }

        private static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> propertyLambda)
        {
            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                var unaryExpression = propertyLambda.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    member = unaryExpression.Operand as MemberExpression;
                }
            }

            if (member == null)
            {
                throw new ArgumentException("The lambda expression 'propertyLambda' should point to a valid property");
            }

            return member.Member as PropertyInfo;
        }
    }
}
