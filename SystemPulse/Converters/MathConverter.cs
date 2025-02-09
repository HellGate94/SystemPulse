using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace SystemPulse.Converters;
public class MathConverter : IValueConverter {
    private Func<double, double>? _convertFormula;
    private Func<double, double>? _convertBackFormula;

    public string? Formula {
        set {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Formula));
            _convertFormula = CompileFormula(value);
        }
    }

    public string? BackFormula {
        set {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(BackFormula));
            _convertBackFormula = CompileFormula(value);
        }
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        double inputValue = System.Convert.ToDouble(value, culture);
        return _convertFormula?.Invoke(inputValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        double inputValue = System.Convert.ToDouble(value, culture);
        return _convertBackFormula?.Invoke(inputValue);
    }

    private static Func<double, double> CompileFormula(string formula) {
        if (string.IsNullOrWhiteSpace(formula))
            throw new ArgumentException("Formula cannot be null or whitespace", nameof(formula));

        var parameter = Expression.Parameter(typeof(double), "x");
        var tokens = Tokenize(formula);
        var parser = new Parser(tokens, parameter);
        var expression = parser.Parse();
        return Expression.Lambda<Func<double, double>>(expression, parameter).Compile();
    }

    private static IEnumerable<Token> Tokenize(string formula) {
        int i = 0;
        while (i < formula.Length) {
            char c = formula[i];

            if (char.IsWhiteSpace(c)) {
                i++;
                continue;
            }

            if (char.IsDigit(c)) {
                int start = i;
                while (i < formula.Length && (char.IsDigit(formula[i]) || formula[i] == '.'))
                    i++;
                yield return new Token(TokenType.NumberLiteral, formula[start..i]);
            } else if (char.IsLetter(c)) {
                int start = i;
                while (i < formula.Length && char.IsLetter(formula[i]))
                    i++;
                yield return new Token(TokenType.Identifier, formula[start..i]);
            } else {
                yield return c switch {
                    '+' => new Token(TokenType.Plus),
                    '-' => new Token(TokenType.Minus),
                    '*' => new Token(TokenType.Multiply),
                    '/' => new Token(TokenType.Divide),
                    '(' => new Token(TokenType.ParenLeft),
                    ')' => new Token(TokenType.ParenRight),
                    ',' => new Token(TokenType.Comma),
                    _ => throw new InvalidOperationException($"Unexpected character: {c}")
                };
                i++;
            }
        }
        yield return new Token(TokenType.End);
    }

    private enum TokenType {
        NumberLiteral,
        Identifier,
        Plus,
        Minus,
        Multiply,
        Divide,
        ParenLeft,
        ParenRight,
        Comma,
        End
    }

    private record Token(TokenType Type, string? Value = null);

    private class Parser {
        private static readonly Dictionary<string, double> Constants = new() {
            ["pi"] = double.Pi,
            ["inf"] = double.PositiveInfinity,
        };

        private static readonly MethodInfo AbsMethod = typeof(double).GetMethod(nameof(double.Abs), [typeof(double)])!;
        private static readonly MethodInfo CeilMethod = typeof(double).GetMethod(nameof(double.Ceiling), [typeof(double)])!;
        private static readonly MethodInfo FloorMethod = typeof(double).GetMethod(nameof(double.Floor), [typeof(double)])!;
        private static readonly MethodInfo RoundMethod = typeof(double).GetMethod(nameof(double.Round), [typeof(double)])!;
        private static readonly MethodInfo TruncMethod = typeof(double).GetMethod(nameof(double.Truncate), [typeof(double)])!;
        private static readonly MethodInfo SqrtMethod = typeof(double).GetMethod(nameof(double.Sqrt), [typeof(double)])!;
        private static readonly MethodInfo PowMethod = typeof(double).GetMethod(nameof(double.Pow), [typeof(double), typeof(double)])!;
        private static readonly MethodInfo ExpMethod = typeof(double).GetMethod(nameof(double.Exp), [typeof(double)])!;
        private static readonly MethodInfo LogMethod = typeof(double).GetMethod(nameof(double.Log), [typeof(double), typeof(double)])!;
        private static readonly MethodInfo LnMethod = typeof(double).GetMethod(nameof(double.Log), [typeof(double)])!;
        private static readonly MethodInfo SinMethod = typeof(double).GetMethod(nameof(double.Sin), [typeof(double)])!;
        private static readonly MethodInfo CosMethod = typeof(double).GetMethod(nameof(double.Cos), [typeof(double)])!;
        private static readonly MethodInfo TanMethod = typeof(double).GetMethod(nameof(double.Tan), [typeof(double)])!;

        private static readonly Dictionary<string, Func<IEnumerable<Expression>, Expression>> Functions = new() {
            ["abs"] = args => Expression.Call(AbsMethod, args),
            ["ceil"] = args => Expression.Call(CeilMethod, args),
            ["floor"] = args => Expression.Call(FloorMethod, args),
            ["round"] = args => Expression.Call(RoundMethod, args),
            ["trunc"] = args => Expression.Call(TruncMethod, args),
            ["sqrt"] = args => Expression.Call(SqrtMethod, args),
            ["pow"] = args => Expression.Call(PowMethod, args),
            ["exp"] = args => Expression.Call(ExpMethod, args),
            ["log"] = args => Expression.Call(LogMethod, args),
            ["ln"] = args => Expression.Call(LnMethod, args),
            ["sin"] = args => Expression.Call(SinMethod, args),
            ["cos"] = args => Expression.Call(CosMethod, args),
            ["tan"] = args =>Expression.Call(TanMethod, args),
        };

        private readonly IEnumerator<Token> _tokens;
        private readonly ParameterExpression _parameter;
        private Token? _nextToken;

        public Parser(IEnumerable<Token> tokens, ParameterExpression parameter) {
            _tokens = tokens.GetEnumerator();
            _parameter = parameter;
        }

        private Token Peek() {
            if (_nextToken is null) {
                if (!_tokens.MoveNext())
                    throw new InvalidOperationException("Unexpected end of token stream.");
                _nextToken = _tokens.Current;
            }
            return _nextToken;
        }

        private Token Advance() {
            Token token = Peek();
            _nextToken = null;
            return token;
        }

        private Token Expect(TokenType tokenType) {
            Token token = Advance();
            if (token.Type != tokenType)
                throw new InvalidOperationException($"Expected token {tokenType} but got {token.Type}.");
            return token;
        }

        public Expression Parse() {
            Expression expr = ParseExpression();
            if (Peek().Type != TokenType.End)
                throw new InvalidOperationException("Unexpected tokens at end of expression.");
            return expr;
        }

        private Expression ParseExpression() => ParseAdditionExpression();

        private Expression ParseOperatorChain(Func<Expression> parseOperand, Func<TokenType, Func<Expression, Expression, Expression>?> toOperator) {
            Expression left = parseOperand();

            while (true) {
                var opFunc = toOperator(Peek().Type);
                if (opFunc is null)
                    break;
                Advance();
                Expression right = parseOperand();
                left = opFunc(left, right);
            }

            return left;
        }

        private Expression ParseAdditionExpression() => ParseOperatorChain(
            ParseMultiplicationExpression,
            tokenType => tokenType switch {
                TokenType.Plus => (left, right) => Expression.Add(left, right),
                TokenType.Minus => (left, right) => Expression.Subtract(left, right),
                _ => null
            });

        private Expression ParseMultiplicationExpression() => ParseOperatorChain(
            ParsePrefixUnaryOperator,
            tokenType => tokenType switch {
                TokenType.Multiply => (left, right) => Expression.Multiply(left, right),
                TokenType.Divide => (left, right) => Expression.Divide(left, right),
                _ => null
            });

        private Expression ParsePrefixUnaryOperator() {
            if (Peek().Type is TokenType.Plus or TokenType.Minus) {
                TokenType op = Advance().Type;
                Expression operand = ParsePrefixUnaryOperator();
                return op switch {
                    TokenType.Minus => Expression.Negate(operand),
                    TokenType.Plus or _ => Expression.UnaryPlus(operand),
                };
            }

            return ParsePrimaryExpression();
        }

        private Expression ParsePrimaryExpression() {
            Token token = Advance();
            return token.Type switch {
                TokenType.NumberLiteral => Expression.Constant(double.Parse(token.Value!, CultureInfo.InvariantCulture)),
                TokenType.Identifier => ParseIdentifier(token),
                TokenType.ParenLeft => ParseParenExpression(),
                _ => throw new InvalidOperationException($"Unexpected token: {token.Type}.")
            };
        }

        private Expression ParseParenExpression() {
            Expression inner = ParseExpression();
            Expect(TokenType.ParenRight);
            return inner;
        }

        private Expression ParseIdentifier(Token token) {
            string id = token.Value!;

            if (string.Equals(id, "x", StringComparison.OrdinalIgnoreCase))
                return _parameter;

            if (Constants.TryGetValue(id, out double constantValue))
                return Expression.Constant(constantValue);

            if (Functions.TryGetValue(id, out var function)) {
                Expect(TokenType.ParenLeft);
                List<Expression> args = [];
                if (Peek().Type != TokenType.ParenRight) {
                    args.Add(ParseExpression());
                    while (Peek().Type == TokenType.Comma) {
                        Advance();
                        args.Add(ParseExpression());
                    }
                }
                Expect(TokenType.ParenRight);
                return function(args);
            }

            throw new InvalidOperationException($"Unknown identifier: {id}");
        }
    }
}
