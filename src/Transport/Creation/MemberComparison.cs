namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Reflection;

    static class MemberComparison
    {
        public static bool AllMembersAreEqual(this object left, object right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            var type = left.GetType();
            if (type != right.GetType())
                return false;

            if (left is ValueType)
            {
                // do a field comparison, or use the override if Equals is implemented:
                return left.Equals(right);
            }

            // check for override:
            if (type != typeof(object)
                && type == type.GetMethod("Equals").DeclaringType)
            {
                // the Equals method is overridden, use it:
                return left.Equals(right);
            }

            // all Arrays, Lists, IEnumerable<> etc implement IEnumerable
            if (left is IEnumerable)
            {
                var rightEnumerator = (right as IEnumerable).GetEnumerator();
                rightEnumerator.Reset();
                foreach (object leftItem in left as IEnumerable)
                {
                    // unequal amount of items
                    if (!rightEnumerator.MoveNext())
                        return false;
                    else
                    {
                        if (!AllMembersAreEqual(leftItem, rightEnumerator.Current))
                            return false;
                    }
                }
            }
            else
            {
                // compare each property
                foreach (PropertyInfo info in type.GetProperties(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.GetProperty))
                {
                    // TODO: need to special-case indexable properties
                    if (!AllMembersAreEqual(info.GetValue(left, null), info.GetValue(right, null)))
                        return false;
                }

                // compare each field
                foreach (FieldInfo info in type.GetFields(
                    BindingFlags.GetField |
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance))
                {
                    if (!AllMembersAreEqual(info.GetValue(left), info.GetValue(right)))
                        return false;
                }
            }
            return true;
        }
    }
}