using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ibliskavka.Common
{
    /// <summary>
    /// These update methods are useful for dealing with objects that track their own IsDirty flag such as EntityFramework or webservice APIs for SharePoint or Dynamics CRM.
    /// 
    /// Example: Doing a data load into SharePoint you only want to update a value if it is different or else you will force an update on the ModifiedOn and ModifiedBy field.
    /// 
    /// By using a non-short-circuiting OR chain you can get a pretty concise update operation.
    /// if (
    ///     Update(target, x => x.Line1, source.Line1)
    ///     | Update(target, x => x.Line2, source.Line2)
    ///     | Update(target, x => x.City, source.City)
    ///     | Update(target, x => x.State, source.State)
    ///     | Update(target, x => x.Zip, source.Zip))
    /// {
    ///     context.UpdateObject(target);
    /// }

    /// </summary>
    public static class EntityUpdaters
    {
        /// <summary>
        /// This is the default updater. It works well when they types are the same.
        /// </summary>
        public static bool Update<T, U>(T target, Expression<Func<T, U>> outExpr, U newValue)
        {
            var expr = (MemberExpression)outExpr.Body;
            var prop = (PropertyInfo)expr.Member;

            U oldValue = (U)prop.GetValue(target, null);

            if (!EqualityComparer<U>.Default.Equals(oldValue, newValue))          //Only update if changed.
            {
                prop.SetValue(target, newValue, null);
                return true;
            }
            return false;
        }

        /// <summary>
        /// This is a updater overide that implements special handling for strings.
        /// </summary>
        public static bool Update<T>(T target, Expression<Func<T, string>> outExpr, string newValue)
        {
            // Clean up the input or perform any common operations.
            if (newValue != null) newValue = newValue.Trim();

            var expr = (MemberExpression)outExpr.Body;
            var prop = (PropertyInfo)expr.Member;

            string oldValue = (string)prop.GetValue(target, null);

            if (oldValue != newValue)          //Only update if changed.
            {
                prop.SetValue(target, newValue, null);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Parsing updater. Very convenient when the input is strings, such as XML or a file, etc. and the output is more specific.
        /// </summary>
        public static bool ParseUpdate<T, U>(T target, Expression<Func<T, U>> outExpr, string strValue)
        {
            U newValue;

            if (strValue.TryParseGeneric<U>(out newValue))
            {
                var expr = (MemberExpression)outExpr.Body;
                var prop = (PropertyInfo)expr.Member;

                U oldValue = (U)prop.GetValue(target, null);

                if (!EqualityComparer<U>.Default.Equals(oldValue, newValue))
                {
                    prop.SetValue(target, newValue, null);
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Generic parse method. Converter must exist for the type. I only tested this for primitive types. You might need to test this further.
        /// </summary>
        public static bool TryParseGeneric<T>(this string input, out T result)
        {
            //Special handling for bool? types.
            if (typeof(T) == typeof(bool?))
            {
                bool? boolVal = (string.IsNullOrWhiteSpace(input) || input == "0")
                                ? false
                                : true;

                result = (T)(object)boolVal;
                return true;
            }

            var converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

            if (converter != null)
            {
                try
                {
                    result = (T)converter.ConvertFromString(input);
                    return true;
                }
                catch (Exception)
                {
                    //"Parsing" failed. Fall through to default.
                }
            }
            result = default(T);
            return false;

        }
    }
}
