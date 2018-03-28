using System;
using System.Collections.Generic;
using Mapsui.VectorTiles.MapboxGLStyler.Filter;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Converter
{
    /// <summary>
    /// Create filters from given filter array in Mapbpx GL style layer
    /// </summary>
    public class FilterConverter
    {
        public static bool IsExpressionFilter(JArray filter)
        {
            if (filter == null || filter.Count == 0)
                return false;

            if (filter[0].Type != JTokenType.String)
                return false;

            var op = filter[0].ToString();

            switch (op)
            {
                case "has":
                    if (filter.Count < 2)
                        return false;
                    var operand = filter[1].ToString();
                    return operand != "$id" && operand != "$type";
                case "in":
                case "!in":
                case "!has":
                case "none":
                    return false;
                case "==":
                case "!=":
                case ">":
                case ">=":
                case "<":
                case "<=":
                    return filter.Count == 3 && (filter[1].Type == JTokenType.Array || filter[2].Type == JTokenType.Array);
                case "any":
                case "all":
                    for (int i = 1; i < filter.Count; i++)
                    {
                        if (!IsExpressionFilter(filter[i] as JArray) && filter.Type != JTokenType.Boolean)
                            return false;
                    }

                    return true;
            }
            return true;
        }

        public static object CheckValue(JValue value)
        {
            if (value != null && value.Type != JTokenType.Boolean && value.Type != JTokenType.Float && value.Type != JTokenType.Integer && value.Type != JTokenType.String)
                throw new ArgumentException("filter expression value must be a bool, float or string");

            return value;
        }

        public static GeometryType ToGeometryType(JValue value)
        {
            if (value.Type != JTokenType.String)
                throw new ArgumentException("value for $type filter must be a string");

            var type = value.ToString();

            switch (type)
            {
                case "Point":
                    return GeometryType.Point;
                case "LineString":
                    return GeometryType.LineString;
                case "Polygon":
                    return GeometryType.Polygon;
            }

            throw new ArgumentException("value for $type filter must be Point, LineString or Polygon");
        }

        public static object ToFeatureIdentifier(JValue value)
        {
            return CheckValue(value);
        }

        private IFilter ConvertUnaryFilter(JArray filter, bool invert = false)
        {
            if (filter.Count < 2)
                throw new ArgumentException("filter expression must have 2 elements");

            if (filter[1].Type != JTokenType.String)
                throw new ArgumentException("filter expression key must be a string");

            var key = filter[1].ToString();

            if (key.Equals("$id"))
            {
                if (invert)
                    return new NotHasIdentifierFilter();
                else
                    return new HasIdentifierFilter();
            }
            else
            {
                if (invert)
                    return new NotHasFilter(key);
                else
                    return new HasFilter(key);
            }
        }

        private IFilter ConvertEqualityFilter(JArray filter, bool invert = false)
        {
            if (filter.Count < 3)
                throw new ArgumentException("filter expression must have 3 elements");

            if (filter[1].Type != JTokenType.String)
                throw new ArgumentException("filter expression key must be a string");

            var key = filter[1].ToString();

            if (key.Equals("$type"))
            {
                var filterValue = ToGeometryType((JValue)filter[2]);

                if (invert)
                    return new TypeNotEqualsFilter(filterValue);
                else
                    return new TypeEqualsFilter(filterValue);
            }
            else if (key.Equals("$id"))
            {
                var filterValue = ToFeatureIdentifier(filter[2] as JValue);

                if (filterValue == null)
                    return null;

                if (invert)
                    return new IdentifierNotEqualsFilter(filterValue.ToString());
                else
                    return new IdentifierEqualsFilter(filterValue.ToString());
            }
            else
            {
                if (!(filter[2] is JValue filterValue))
                    return null;

                if (invert)
                    return new NotEqualsFilter(key, filterValue);
                else
                    return new EqualsFilter(key, filterValue);
            }
        }

        private IFilter ConvertBinaryFilter<T>(JArray filter) where T : BinaryFilter
        {
            if (filter.Count < 3)
                throw new ArgumentException("filter expression must have 3 elements");

            if (filter[1].Type != JTokenType.String)
                throw new ArgumentException("filter expression key must be a string");

            var key = filter[1].ToString();
            var filterValue = CheckValue(filter[2] as JValue);

            if (filterValue == null)
                return null;

            return (BinaryFilter)Activator.CreateInstance(typeof(T), key, filterValue);
        }

        private IFilter ConvertSetFilter(JArray filter, bool invert = false)
        {
            if (filter.Count < 2)
                throw new ArgumentException("filter expression must have 2 elements");

            if (filter[1].Type != JTokenType.String)
                throw new ArgumentException("filter expression key must be a string");

            var key = filter[1].ToString();

            if (key.Equals("$type"))
            {
                var filterList = new List<GeometryType>();

                for (int i = 2; i < filter.Count; i++)
                    filterList.Add(ToGeometryType((JValue)filter[i]));

                if (invert)
                    return new TypeNotInFilter(filterList);
                else
                    return new TypeInFilter(filterList);
            }
            else if (key.Equals("$id"))
            {
                var filterList = new List<string>();

                for (int i = 2; i < filter.Count; i++)
                {
                    var filterValue = ToFeatureIdentifier(filter[i] as JValue).ToString();

                    if (!string.IsNullOrEmpty(filterValue))
                        filterList.Add(filterValue);
                }

                if (invert)
                    return new IdentifierNotInFilter(filterList);
                else
                    return new IdentifierInFilter(filterList);
            }
            else
            {
                var filterList = new List<JValue>();

                for (int i = 2; i < filter.Count; i++)
                {
                    if (filter[i] is JValue value)
                        filterList.Add(value);
                }

                if (invert)
                    return new NotInFilter(key, filterList);
                else
                    return new InFilter(key, filterList);
            }
        }

        public IFilter ConvertCompoundFilter<T>(JArray filter) where T : CompoundFilter
        {
            List<IFilter> filters = new List<IFilter>();

            for (int i = 1; i < filter.Count; i++)
            {
                if (filter[i].Type != JTokenType.Array)
                    throw new ArgumentException("compound filters must be arrays");

                var element = ConvertFilter((JArray)filter[i]);

                if (element != null)
                    filters.Add(element);
            }

            return (CompoundFilter)Activator.CreateInstance(typeof(T), filters);
        }

        public IFilter ConvertExpressionFilter(JArray filter)
        {
            // TODO
            //optional<Filter> convertExpressionFilter(const Convertible&value, Error & error)
            //{
            //    optional<std::unique_ptr<Expression>> expression = convert<std::unique_ptr<Expression>>(value, error, expression::type::Boolean);
            //    if (!expression)
            //    {
            //        return { };
            //    }
            //    return { ExpressionFilter { std::move(*expression) } };
            //}
            return new ExpressionFilter();
        }

        /// <summary>
        /// Convert filter array to usable filter objects
        /// </summary>
        /// <param name="filter">JArray with all filters</param>
        /// <returns>A filter usable while checking styles</returns>
        public IFilter ConvertFilter(JArray filter)
        {
            if (IsExpressionFilter(filter))
                return ConvertExpressionFilter(filter);

            if (filter == null)
                throw new ArgumentException("filter expression must be an array");

            if (filter.Count < 1)
                throw new ArgumentException("filter expression must have at least 1 element");

            if (filter[0].Type != JTokenType.String)
                throw new ArgumentException("filter operator must be a string");

            var op = filter[0].ToString();

            switch (op)
            {
                case "==":
                    return ConvertEqualityFilter(filter);
                case "!=":
                    return ConvertEqualityFilter(filter, true);
                case ">":
                    return ConvertBinaryFilter<GreaterThanFilter>(filter);
                case ">=":
                    return ConvertBinaryFilter<GreaterThanEqualsFilter>(filter);
                case "<":
                    return ConvertBinaryFilter<LessThanFilter>(filter);
                case "<=":
                    return ConvertBinaryFilter<LessThanEqualsFilter>(filter);
                case "in":
                    return ConvertSetFilter(filter);
                case "!in":
                    return ConvertSetFilter(filter, true);
                case "all":
                    return ConvertCompoundFilter<AllFilter>(filter);
                case "any":
                    return ConvertCompoundFilter<AnyFilter>(filter);
                case "none":
                    return ConvertCompoundFilter<NoneFilter>(filter);
                case "has":
                    return ConvertUnaryFilter(filter);
                case "!has":
                    return ConvertUnaryFilter(filter, true);
                default:
                    throw new ArgumentException("filter operator must be one of \"==\", \"!=\", \">\", \">=\", \"<\", \"<=\", \"in\", \"!in\", \"all\", \"any\", \"none\", \"has\", or \"!has\"");
            }
        }
    }
}