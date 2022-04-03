using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Kapok.Core.FilterParsing.SupportedOperands;

namespace Kapok.Core.FilterParsing;

public class FilterExpressionParser
{
    private readonly MethodFinder _methodFinder;
    private readonly TextParser _textParser;
    private readonly IExpressionHelper _expressionHelper;
    private readonly IExpressionPromoter _expressionPromoter;

    private ParameterExpression _it;
    private ParameterExpression? _root;
    private readonly MemberExpression _propertyMemberExpression;
    private readonly PropertyInfo _propertyInfo;

    private readonly IDictionary<string, object> _keywords = StaticKeywords;

    private static IDictionary<string, object> StaticKeywords =>
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            {"true", Constants.TrueLiteral},
            {"false", Constants.FalseLiteral},
            {"null", Constants.NullLiteral}
        };

    protected readonly CultureInfo Culture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterExpressionParser"/> class.
    /// </summary>
    /// <param name="itType">The base type which contains the property which shall be filtered.</param>
    /// <param name="propertyName">The property to which this filter is applied.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="cultureInfo"></param>
    public FilterExpressionParser(Type itType, string propertyName, string expression, CultureInfo? cultureInfo = null)
    {
        Culture = cultureInfo ?? CultureInfo.InvariantCulture;

        ParameterExpression[] parameters = new []{
            Expression.Parameter(itType, string.Empty)
        };

        _propertyInfo = itType.GetProperty(propertyName);
        if (_propertyInfo == null)
            throw new ArgumentException(string.Format(Res.ItPropertyNotFound, propertyName, itType.FullName), nameof(propertyName));

        _propertyMemberExpression = Expression.Property(parameters[0], _propertyInfo);

        if (string.IsNullOrEmpty(expression))
            throw new ArgumentException("Parameter must have a value and cannot be empty", nameof(expression));

        ProcessParameters(parameters);

        _textParser = new TextParser(expression);
        _expressionPromoter = new ExpressionPromoter();
        _methodFinder = new MethodFinder(_expressionPromoter);
        _expressionHelper = new ExpressionHelper();
    }

    void ProcessParameters(ParameterExpression[] parameters)
    {
        // If there is only 1 ParameterExpression, do also allow access using 'it'
        if (parameters.Length == 1)
        {
            _it = parameters[0];

            if (_root == null)
            {
                _root = _it;
            }
        }
    }

    /// <summary>
    /// Uses the TextParser to parse the string into the specified result type.
    /// </summary>
    /// <returns>Expression</returns>
    public Expression Parse()
    {
        Type resultType = typeof(bool);

        // we first try to parse the complete text as static filter

        Expression? expr = null;
        try
        {
            var result = FilterStringToPropertyValue(_propertyInfo, _textParser._text, Culture);

            expr = Expression.Equal(
                _propertyMemberExpression,
                Expression.Constant(result, _propertyInfo.PropertyType));
        }
        catch (Exception)
        {
            // ignored
        }

        // if that did not work, we go forward with the filter string parsing
        if (expr == null)
        {
            _textParser.NextToken();
            int exprPos = _textParser.CurrentToken.Pos;
            expr = ParseOrOperator();
            if ((expr = _expressionPromoter.Promote(expr, resultType, true, false)) == null)
            {
                throw ParseError(exprPos, Res.ExpressionTypeMismatch, TypeHelper.GetTypeName(resultType));
            }
            _textParser.ValidateToken(TokenId.End, Res.SyntaxError);
        }

        return Expression.Lambda(expr, _root);
    }

    public static string? PropertyValueToFilterString(PropertyInfo propertyInfo, object? value, CultureInfo cultureInfo)
    {
        if (value == null)
            return "null";

        var propertyType = propertyInfo.PropertyType;

        if (propertyType.IsNumericType())
        {
            return value.ToString();
        }

        if (propertyType.IsEnum)
        {
            return EnumExtension.EnumValueToDisplayName(value, cultureInfo);
        }

        if (propertyType.IsValueType)
        {
            if (propertyType == typeof(bool))
            {
                return (bool)value ? "true" : "false";
            }

            if (propertyType == typeof(DateTime))
            {
                var dataTypeAttribute = propertyInfo.GetCustomAttribute<DataTypeAttribute>();
                if (dataTypeAttribute != null)
                {
                    switch (dataTypeAttribute.DataType)
                    {
                        case DataType.Date:
                            return ((DateTime)value).ToString("d", cultureInfo);
                        case DataType.Time:
                            return ((DateTime)value).ToString("T", cultureInfo);
                        case DataType.DateTime:
                            return ((DateTime)value).ToString("G", cultureInfo);
                    }
                }

                return ((DateTime)value).ToString("G", cultureInfo);
            }

            if (propertyType == typeof(TimeSpan))
            {
                return ((TimeSpan)value).ToString("g", cultureInfo);
            }

            if (propertyType == typeof(Guid))
            {
                return ((Guid)value).ToString("D");
            }

            return null;
        }

        if (propertyType == typeof(string))
        {
            if (((string)value).Contains('\"'))
                // TODO: escaping is not supported in a query string today
                return null;

            bool quoteString = false;

            if ((string) value == string.Empty)
                quoteString = true;
            else
            {
                var operatorChar = new[] { '!', '=', '<', '>', '~', '&', '|', '(', ')', '*', '-', '/', '\'' };
                if (((string)value).Intersect(operatorChar).Any())
                {
                    return null;
                }

                var whitespaceChars = new[] { ' ', '\t' };

                if (((string)value).Intersect(whitespaceChars).Any()) // contains any whitespace character
                {
                    quoteString = true;
                }
                else if (StaticKeywords.ContainsKey((string)value))
                {
                    quoteString = true;
                }
            }

            if (quoteString)
                return $"\"{value}\"";
            return (string)value;
        }

        return null;
    }

    public static object? FilterStringToPropertyValue(PropertyInfo propertyInfo, string filterString, CultureInfo cultureInfo)
    {
        var propertyType = propertyInfo.PropertyType;

        if (propertyType.IsEnum)
        {
            // here we first try to parse the enum value by display name
            if (EnumExtension.TryParseDisplayName(propertyType, filterString, out var enumValue, cultureInfo))
                return enumValue;

            // if that doesn't work, use the default parser, maybe someone used the system naming.
            return Enum.Parse(propertyType, filterString, true);
        }

        if (propertyType.IsNumericType())
        {
            var numberType = propertyType;
            Type? nullableTypeCast = Nullable.GetUnderlyingType(numberType);
            if (nullableTypeCast != null)
            {
                if (filterString == "null")
                    return null;

                numberType = nullableTypeCast;
            }

            if (numberType == typeof(byte))
                return byte.Parse(filterString, cultureInfo);
            if (numberType == typeof(sbyte))
                return sbyte.Parse(filterString, cultureInfo);
            if (numberType == typeof(ushort))
                return ushort.Parse(filterString, cultureInfo);
            if (numberType == typeof(uint))
                return uint.Parse(filterString, cultureInfo);
            if (numberType == typeof(ulong))
                return ulong.Parse(filterString, cultureInfo);
            if (numberType == typeof(short))
                return short.Parse(filterString, cultureInfo);
            if (numberType == typeof(int))
                return int.Parse(filterString, cultureInfo);
            if (numberType == typeof(long))
                return long.Parse(filterString, cultureInfo);
            if (numberType == typeof(decimal))
                return decimal.Parse(filterString, cultureInfo);
            if (numberType == typeof(float))
                return float.Parse(filterString, cultureInfo);

            throw new NotSupportedException($"Not expected numeric type {numberType.FullName}");
        }

        if (filterString == "null")
            return null;

        if (propertyType.IsValueType)
        {
            if (propertyType == typeof(bool))
            {
                switch (filterString)
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                    default:
                        throw new ParseException("Invalid static value for boolean", 0);
                }
            }

            if (propertyType == typeof(DateTime))
            {
                var dataTypeAttribute = propertyInfo.GetCustomAttribute<DataTypeAttribute>();
                if (dataTypeAttribute != null)
                {
                    switch (dataTypeAttribute.DataType)
                    {
                        case DataType.Date:
                            return DateTime.ParseExact(filterString, "d", cultureInfo);
                        case DataType.Time:
                            return DateTime.ParseExact(filterString, "T", cultureInfo);
                        case DataType.DateTime:
                            return DateTime.ParseExact(filterString, "G", cultureInfo);
                    }
                }

                return DateTime.ParseExact(filterString, "G", cultureInfo);
            }

            if (propertyType == typeof(TimeSpan))
            {
                return TimeSpan.ParseExact(filterString,"g", cultureInfo);
            }

            if (propertyType == typeof(Guid))
            {
                return Guid.Parse(filterString);
            }

            throw new NotSupportedException($"Not expected value type {propertyType.FullName}");
        }

        if (propertyType == typeof(string))
        {
            if (filterString[0] == '"' &&  filterString[filterString.Length-1] == '"')
            {
                // TODO: this would be the right place to parse chars like \t \n etc. in the string
                return filterString.Substring(1, filterString.Length - 2);
            }

            var operatorChar = new[] { '!', '=', '<', '>', '~', '&', '|', '(', ')' };
            if (filterString.Intersect(operatorChar).Any())
            {
                throw new ParseException("Operator in static filter string", 0);
            }

            return filterString;
        }

        throw new NotSupportedException($"Not expected type {propertyType.FullName}");
    }

    public bool TryParse(out Expression? expression, out ParseException? parseException)
    {
        // TODO: this is a kind of an ugly solution because we use 'try/catch' to find out if it works which is not so good when it comes to performance issues
        try
        {
            expression = Parse();
            parseException = null;
            return true;
        }
        catch (ParseException e)
        {
            expression = null;
            parseException = e;
            return false;
        }
    }

    // |, or operator
    Expression ParseOrOperator()
    {
        Expression left = ParseAndOperator();
        while (_textParser.CurrentToken.Id == TokenId.Bar || TokenIdentifierIs("or"))
        {
            Token op = _textParser.CurrentToken;
            _textParser.NextToken();
            Expression right = ParseAndOperator();
            CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Pos);
            left = Expression.OrElse(left, right);
        }
        return left;
    }

    // &, and operator
    Expression ParseAndOperator()
    {
        Expression left = ParseComparisonOperator();
        while (_textParser.CurrentToken.Id == TokenId.Amphersand || TokenIdentifierIs("and"))
        {
            Token op = _textParser.CurrentToken;
            _textParser.NextToken();
            Expression right = ParseComparisonOperator();
            CheckAndPromoteOperands(typeof(ILogicalSignatures), op.Text, ref left, ref right, op.Pos);
            left = Expression.AndAlso(left, right);
        }
        return left;
    }

    // =, ==, !=, <>, >, >=, <, <=, ~ operators
    Expression ParseComparisonOperator()
    {
        Expression left = _propertyMemberExpression;
        bool firstRun = true;

        while (firstRun ||
               _textParser.CurrentToken.Id == TokenId.Equal ||
               _textParser.CurrentToken.Id == TokenId.ExclamationEqual || _textParser.CurrentToken.Id == TokenId.LessGreater ||
               _textParser.CurrentToken.Id == TokenId.Tilde || _textParser.CurrentToken.Id == TokenId.ExclamationTilde ||
               _textParser.CurrentToken.Id == TokenId.GreaterThan || _textParser.CurrentToken.Id == TokenId.GreaterThanEqual ||
               _textParser.CurrentToken.Id == TokenId.LessThan || _textParser.CurrentToken.Id == TokenId.LessThanEqual)
        {
            ConstantExpression constantExpr;
            TypeConverter typeConverter;
            Token op = _textParser.CurrentToken;

            // when no operator is given, the operator is automatically set to equal.
            if (firstRun)
            {
                firstRun = false;

                if (_textParser.CurrentToken.Id != TokenId.Equal &&
                    _textParser.CurrentToken.Id != TokenId.ExclamationEqual && _textParser.CurrentToken.Id != TokenId.LessGreater &&
                    _textParser.CurrentToken.Id != TokenId.Tilde && _textParser.CurrentToken.Id != TokenId.ExclamationTilde &&
                    _textParser.CurrentToken.Id != TokenId.GreaterThan && _textParser.CurrentToken.Id != TokenId.GreaterThanEqual &&
                    _textParser.CurrentToken.Id != TokenId.LessThan && _textParser.CurrentToken.Id != TokenId.LessThanEqual)
                {
                    var oldOp = op;
                    op = new Token
                    {
                        Id = TokenId.Equal,
                        Text = "=",
                        OriginalId = oldOp.OriginalId,
                        Pos = oldOp.Pos
                    };
                }
                else
                {
                    _textParser.NextToken();
                }
            }
            else
            {
                _textParser.NextToken();
            }

            Expression right = ParseShiftOperator();
            bool isEquality = op.Id == TokenId.Equal || op.Id == TokenId.ExclamationEqual || op.Id == TokenId.LessGreater;

            if (isEquality && (!left.Type.IsValueType && !right.Type.IsValueType || left.Type == typeof(Guid) && right.Type == typeof(Guid)))
            {
                // If left or right is NullLiteral, just continue. Else check if the types differ.
                if (!(left == Constants.NullLiteral || right == Constants.NullLiteral) && left.Type != right.Type)
                {
                    if (left.Type.IsAssignableFrom(right.Type))
                    {
                        right = Expression.Convert(right, left.Type);
                    }
                    else if (right.Type.IsAssignableFrom(left.Type))
                    {
                        left = Expression.Convert(left, right.Type);
                    }
                    else
                    {
                        throw IncompatibleOperandsError(op.Text, left, right, op.Pos);
                    }
                }
            }
            else if (TypeHelper.IsEnumType(left.Type) || TypeHelper.IsEnumType(right.Type))
            {
                if (left.Type != right.Type)
                {
                    Expression e;
                    if ((e = _expressionPromoter.Promote(right, left.Type, true, false)) != null)
                    {
                        right = e;
                    }
                    else if ((e = _expressionPromoter.Promote(left, right.Type, true, false)) != null)
                    {
                        left = e;
                    }
                    else if (TypeHelper.IsEnumType(left.Type) && (constantExpr = right as ConstantExpression) != null)
                    {
                        right = ParseEnumToConstantExpression(op.Pos, left.Type, constantExpr);
                    }
                    else if (TypeHelper.IsEnumType(right.Type) && (constantExpr = left as ConstantExpression) != null)
                    {
                        left = ParseEnumToConstantExpression(op.Pos, right.Type, constantExpr);
                    }
                    else
                    {
                        throw IncompatibleOperandsError(op.Text, left, right, op.Pos);
                    }
                }
            }
            else if ((constantExpr = right as ConstantExpression) != null && constantExpr.Value is string && (typeConverter = TypeDescriptor.GetConverter(left.Type)) != null)
            {
                try
                {
                    right = Expression.Constant(typeConverter.ConvertFromInvariantString((string)constantExpr.Value), left.Type);
                }
                catch (FormatException e)
                {
                    throw new ParseException(e.Message, op.Pos);
                }
            }
            else if ((constantExpr = left as ConstantExpression) != null && constantExpr.Value is string && (typeConverter = TypeDescriptor.GetConverter(right.Type)) != null)
            {
                try
                {
                    left = Expression.Constant(typeConverter.ConvertFromInvariantString((string)constantExpr.Value), right.Type);
                }
                catch (FormatException e)
                {
                    throw new ParseException(e.Message, op.Pos);
                }

            }
            else
            {
                bool typesAreSameAndImplementCorrectInterface = false;
                if (left.Type == right.Type)
                {
                    var interfaces = left.Type.GetInterfaces().Where(x => x.IsGenericType);
                    if (isEquality)
                    {
                        typesAreSameAndImplementCorrectInterface = interfaces.Any(x => x.GetGenericTypeDefinition() == typeof(IEquatable<>));
                    }
                    else
                    {
                        typesAreSameAndImplementCorrectInterface = interfaces.Any(x => x.GetGenericTypeDefinition() == typeof(IComparable<>));
                    }
                }


                if (!typesAreSameAndImplementCorrectInterface)
                {
                    if (left.Type.IsClass && right is ConstantExpression && HasImplicitConversion(left.Type, right.Type))
                    {
                        left = Expression.Convert(left, right.Type);
                    }
                    else if (right.Type.IsClass && left is ConstantExpression && HasImplicitConversion(right.Type, left.Type))
                    {
                        right = Expression.Convert(right, left.Type);
                    }
                    else if(left.Type == typeof(string))
                    {
                        var toStringMethod = right.Type.GetMethod(nameof(ToString), BindingFlags.Public | BindingFlags.Instance, null, new Type []{}, null);

                        right = Expression.Call(right, toStringMethod);
                    }
                    else
                    {
                        CheckAndPromoteOperands(isEquality ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures), op.Text, ref left, ref right, op.Pos);
                    }
                }
            }

            switch (op.Id)
            {
                case TokenId.Equal:
                    left = _expressionHelper.GenerateEqual(left, right);
                    break;
                case TokenId.ExclamationEqual:
                case TokenId.LessGreater:
                    left = _expressionHelper.GenerateNotEqual(left, right);
                    break;
                case TokenId.GreaterThan:
                    left = _expressionHelper.GenerateGreaterThan(left, right);
                    break;
                case TokenId.GreaterThanEqual:
                    left = _expressionHelper.GenerateGreaterThanEqual(left, right);
                    break;
                case TokenId.LessThan:
                    left = _expressionHelper.GenerateLessThan(left, right);
                    break;
                case TokenId.LessThanEqual:
                    left = _expressionHelper.GenerateLessThanEqual(left, right);
                    break;
                case TokenId.Tilde:
                    left = _expressionHelper.GenerateLike(left, right);
                    break;
                case TokenId.ExclamationTilde:
                    left = _expressionHelper.GenerateNotLike(left, right);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return left;
    }

    private bool HasImplicitConversion(Type baseType, Type targetType)
    {
        return baseType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(mi => mi.Name == "op_Implicit" && mi.ReturnType == targetType)
            .Any(mi => mi.GetParameters().FirstOrDefault()?.ParameterType == baseType);
    }

    private ConstantExpression ParseEnumToConstantExpression(int pos, Type leftType, ConstantExpression constantExpr)
    {
        return Expression.Constant(ParseConstantExpressionToEnum(pos, leftType, constantExpr), leftType);
    }

    private object ParseConstantExpressionToEnum(int pos, Type leftType, ConstantExpression constantExpr)
    {
        try
        {
            if (constantExpr.Value is string stringValue)
            {
                return Enum.Parse(TypeHelper.GetNonNullableType(leftType), stringValue, true);
            }
        }
        catch
        {
            throw ParseError(pos, Res.ExpressionTypeMismatch, leftType);
        }

        try
        {
            return Enum.ToObject(TypeHelper.GetNonNullableType(leftType), constantExpr.Value);
        }
        catch
        {
            throw ParseError(pos, Res.ExpressionTypeMismatch, leftType);
        }
    }

    // <<, >> operators
    Expression ParseShiftOperator()
    {
        Expression left = ParseAdditive();
        while (_textParser.CurrentToken.Id == TokenId.DoubleLessThan || _textParser.CurrentToken.Id == TokenId.DoubleGreaterThan)
        {
            Token op = _textParser.CurrentToken;
            _textParser.NextToken();
            Expression right = ParseAdditive();
            switch (op.Id)
            {
                case TokenId.DoubleLessThan:
                    CheckAndPromoteOperands(typeof(IShiftSignatures), op.Text, ref left, ref right, op.Pos);
                    left = Expression.LeftShift(left, right);
                    break;
                case TokenId.DoubleGreaterThan:
                    CheckAndPromoteOperands(typeof(IShiftSignatures), op.Text, ref left, ref right, op.Pos);
                    left = Expression.RightShift(left, right);
                    break;
            }
        }
        return left;
    }

    // +, - operators
    Expression ParseAdditive()
    {
        Expression left = ParseMultiplicative();
        while (_textParser.CurrentToken.Id == TokenId.Plus || _textParser.CurrentToken.Id == TokenId.Minus)
        {
            Token op = _textParser.CurrentToken;
            _textParser.NextToken();
            Expression right = ParseMultiplicative();
            switch (op.Id)
            {
                case TokenId.Plus:
                    if (left.Type == typeof(string) || right.Type == typeof(string))
                    {
                        left = _expressionHelper.GenerateStringConcat(left, right);
                    }
                    else
                    {
                        CheckAndPromoteOperands(typeof(IAddSignatures), op.Text, ref left, ref right, op.Pos);
                        left = _expressionHelper.GenerateAdd(left, right);
                    }
                    break;
                case TokenId.Minus:
                    CheckAndPromoteOperands(typeof(ISubtractSignatures), op.Text, ref left, ref right, op.Pos);
                    left = _expressionHelper.GenerateSubtract(left, right);
                    break;
            }
        }
        return left;
    }

    // *, /, %, mod operators
    Expression ParseMultiplicative()
    {
        Expression left = ParseUnary();
        while (_textParser.CurrentToken.Id == TokenId.Asterisk || _textParser.CurrentToken.Id == TokenId.Slash ||
               _textParser.CurrentToken.Id == TokenId.Percent || TokenIdentifierIs("mod"))
        {
            Token op = _textParser.CurrentToken;
            _textParser.NextToken();
            Expression right = ParseUnary();
            CheckAndPromoteOperands(typeof(IArithmeticSignatures), op.Text, ref left, ref right, op.Pos);
            switch (op.Id)
            {
                case TokenId.Asterisk:
                    left = Expression.Multiply(left, right);
                    break;
                case TokenId.Slash:
                    left = Expression.Divide(left, right);
                    break;
                case TokenId.Percent:
                case TokenId.Identifier:
                    left = Expression.Modulo(left, right);
                    break;
            }
        }
        return left;
    }

    // -, !, not unary operators
    Expression ParseUnary()
    {
        if (_textParser.CurrentToken.Id == TokenId.Minus || _textParser.CurrentToken.Id == TokenId.Exclamation || TokenIdentifierIs("not"))
        {
            Token op = _textParser.CurrentToken;
            _textParser.NextToken();
            if (op.Id == TokenId.Minus && (_textParser.CurrentToken.Id == TokenId.IntegerLiteral || _textParser.CurrentToken.Id == TokenId.RealLiteral))
            {
                _textParser.CurrentToken.Text = "-" + _textParser.CurrentToken.Text;
                _textParser.CurrentToken.Pos = op.Pos;
                return ParsePrimary();
            }

            Expression expr = ParseUnary();
            if (op.Id == TokenId.Minus)
            {
                CheckAndPromoteOperand(typeof(INegationSignatures), op.Text, ref expr, op.Pos);
                expr = Expression.Negate(expr);
            }
            else
            {
                CheckAndPromoteOperand(typeof(INotSignatures), op.Text, ref expr, op.Pos);
                expr = Expression.Not(expr);
            }

            return expr;
        }

        return ParsePrimary();
    }

    Expression ParsePrimary()
    {
        switch (_textParser.CurrentToken.Id)
        {
            case TokenId.Identifier:
                return ParseIdentifier();
            case TokenId.StringLiteral:
                return ParseStringLiteral();
            case TokenId.IntegerLiteral:
                return ParseIntegerLiteral();
            case TokenId.RealLiteral:
                return ParseRealLiteral();
            case TokenId.OpenParen:
                return ParseParenExpression();
            default:
                throw ParseError(Res.ExpressionExpected);
        }
    }

    Expression ParseStringLiteral()
    {
        _textParser.ValidateToken(TokenId.StringLiteral);
        char quote = _textParser.CurrentToken.Text[0];
        string s = _textParser.CurrentToken.Text.Substring(1, _textParser.CurrentToken.Text.Length - 2);
        int index1 = 0;
        while (true)
        {
            int index2 = s.IndexOf(quote, index1);
            if (index2 < 0)
            {
                break;
            }

            if (index2 + 1 < s.Length && s[index2 + 1] == quote)
            {
                s = s.Remove(index2, 1);
            }
            index1 = index2 + 1;
        }

        if (quote == '\'')
        {
            if (s.Length != 1)
            {
                throw ParseError(Res.InvalidCharacterLiteral);
            }
            _textParser.NextToken();
            return ConstantExpressionHelper.CreateLiteral(s[0], s);
        }
        _textParser.NextToken();
        return ConstantExpressionHelper.CreateLiteral(s, s);
    }

    Expression ParseIntegerLiteral()
    {
        _textParser.ValidateToken(TokenId.IntegerLiteral);

        string text = _textParser.CurrentToken.Text;
        string qualifier = null;
        char last = text[text.Length - 1];
        bool isHexadecimal = text.StartsWith(text[0] == '-' ? "-0x" : "0x", StringComparison.OrdinalIgnoreCase);
        char[] qualifierLetters = isHexadecimal
            ? new[] { 'U', 'u', 'L', 'l' }
            : new[] { 'U', 'u', 'L', 'l', 'F', 'f', 'D', 'd', 'M', 'm' };

        if (text[0] == '0')
        {
            // consider this integer literal as string because it starts with a leading zero (which the 'TryParse' command would cut off)

            _textParser.NextToken();

            return ConstantExpressionHelper.CreateLiteral(text, text);
        }

        if (qualifierLetters.Contains(last))
        {
            int pos = text.Length - 1, count = 0;
            while (qualifierLetters.Contains(text[pos]))
            {
                ++count;
                --pos;
            }
            qualifier = text.Substring(text.Length - count, count);
            text = text.Substring(0, text.Length - count);
        }

        if (text[0] != '-')
        {
            if (isHexadecimal)
            {
                text = text.Substring(2);
            }

            if (!ulong.TryParse(text, isHexadecimal ? NumberStyles.HexNumber : NumberStyles.Integer, Culture, out ulong value))
            {
                throw ParseError(Res.InvalidIntegerLiteral, text);
            }

            _textParser.NextToken();
            if (!string.IsNullOrEmpty(qualifier))
            {
                if (qualifier == "U" || qualifier == "u") return ConstantExpressionHelper.CreateLiteral((uint)value, text);
                if (qualifier == "L" || qualifier == "l") return ConstantExpressionHelper.CreateLiteral((long)value, text);

                // in case of UL, just return
                return ConstantExpressionHelper.CreateLiteral(value, text);
            }

            // if (value <= (int)short.MaxValue) return ConstantExpressionHelper.CreateLiteral((short)value, text);
            if (value <= int.MaxValue) return ConstantExpressionHelper.CreateLiteral((int)value, text);
            if (value <= uint.MaxValue) return ConstantExpressionHelper.CreateLiteral((uint)value, text);
            if (value <= long.MaxValue) return ConstantExpressionHelper.CreateLiteral((long)value, text);

            return ConstantExpressionHelper.CreateLiteral(value, text);
        }
        else
        {
            if (isHexadecimal)
            {
                text = text.Substring(3);
            }

            if (!long.TryParse(text, isHexadecimal ? NumberStyles.HexNumber : NumberStyles.Integer, Culture, out long value))
            {
                throw ParseError(Res.InvalidIntegerLiteral, text);
            }

            if (isHexadecimal)
            {
                value = -value;
            }

            _textParser.NextToken();
            if (!string.IsNullOrEmpty(qualifier))
            {
                if (qualifier == "L" || qualifier == "l")
                    return ConstantExpressionHelper.CreateLiteral(value, text);

                if (qualifier == "F" || qualifier == "f")
                    return TryParseAsFloat(text, qualifier[0]);

                if (qualifier == "D" || qualifier == "d")
                    return TryParseAsDouble(text, qualifier[0]);

                if (qualifier == "M" || qualifier == "m")
                    return TryParseAsDecimal(text, qualifier[0]);

                throw ParseError(Res.MinusCannotBeAppliedToUnsignedInteger);
            }

            if (value <= int.MaxValue)
            {
                return ConstantExpressionHelper.CreateLiteral((int)value, text);
            }

            return ConstantExpressionHelper.CreateLiteral(value, text);
        }
    }

    Expression ParseRealLiteral()
    {
        _textParser.ValidateToken(TokenId.RealLiteral);

        string text = _textParser.CurrentToken.Text;
        char qualifier = text[text.Length - 1];

        _textParser.NextToken();
        return TryParseAsFloat(text, qualifier);
    }

    Expression TryParseAsFloat(string text, char qualifier)
    {
        if (qualifier == 'F' || qualifier == 'f')
        {
            if (float.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Float, Culture, out float f))
            {
                return ConstantExpressionHelper.CreateLiteral(f, text);
            }
        }

        // not possible to find float qualifier, so try to parse as double
        return TryParseAsDecimal(text, qualifier);
    }

    Expression TryParseAsDecimal(string text, char qualifier)
    {
        if (qualifier == 'M' || qualifier == 'm')
        {
            if (decimal.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Number, Culture, out decimal d))
            {
                return ConstantExpressionHelper.CreateLiteral(d, text);
            }
        }

        // not possible to find float qualifier, so try to parse as double
        return TryParseAsDouble(text, qualifier);
    }

    Expression TryParseAsDouble(string text, char qualifier)
    {
        double d;
        if (qualifier == 'D' || qualifier == 'd')
        {
            if (double.TryParse(text.Substring(0, text.Length - 1), NumberStyles.Number, Culture, out d))
            {
                return ConstantExpressionHelper.CreateLiteral(d, text);
            }
        }

        if (double.TryParse(text, NumberStyles.Number, Culture, out d))
        {
            return ConstantExpressionHelper.CreateLiteral(d, text);
        }

        throw ParseError(Res.InvalidRealLiteral, text);
    }

    Expression ParseParenExpression()
    {
        _textParser.ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
        _textParser.NextToken();
        Expression e = ParseOrOperator();
        _textParser.ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
        _textParser.NextToken();
        return e;
    }

    Expression ParseIdentifier()
    {
        _textParser.ValidateToken(TokenId.Identifier);

        if (_keywords.TryGetValue(_textParser.CurrentToken.Text, out object value))
        {
            _textParser.NextToken();

            return (Expression)value;
        }

        if (_it != null)
        {
            string id = GetIdentifier();
            _textParser.NextToken();

            if (_it.Type.IsEnum)
            {
                var @enum = Enum.Parse(_it.Type, id, true);

                return Expression.Constant(@enum);
            }

            // convert identifier to string
            return Expression.Constant(id, typeof(string));
        }

        throw ParseError(Res.UnknownIdentifier, _textParser.CurrentToken.Text);
    }

    void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
    {
        Expression[] args = { expr };

        if (!_methodFinder.ContainsMethod(signatures, "F", false, args))
        {
            throw IncompatibleOperandError(opName, expr, errorPos);
        }

        expr = args[0];
    }

    void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
    {
        Expression[] args = { left, right };

        if (!_methodFinder.ContainsMethod(signatures, "F", false, args))
        {
            throw IncompatibleOperandsError(opName, left, right, errorPos);
        }

        left = args[0];
        right = args[1];
    }

    Exception IncompatibleOperandError(string opName, Expression expr, int errorPos)
    {
        return ParseError(errorPos, Res.IncompatibleOperand, opName, TypeHelper.GetTypeName(expr.Type));
    }

    Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int errorPos)
    {
        return ParseError(errorPos, Res.IncompatibleOperands, opName, TypeHelper.GetTypeName(left.Type), TypeHelper.GetTypeName(right.Type));
    }

    bool TokenIdentifierIs(string id)
    {
        return _textParser.CurrentToken.Id == TokenId.Identifier && string.Equals(id, _textParser.CurrentToken.Text, StringComparison.OrdinalIgnoreCase);
    }

    string GetIdentifier()
    {
        _textParser.ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
        string id = _textParser.CurrentToken.Text;
        if (id.Length > 1 && id[0] == '@')
        {
            id = id.Substring(1);
        }

        return id;
    }

    Exception ParseError(string format, params object[] args)
    {
        return ParseError(_textParser?.CurrentToken.Pos ?? 0, format, args);
    }

    Exception ParseError(int pos, string format, params object[] args)
    {
        return new ParseException(string.Format(Culture, format, args), pos);
    }
}