using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace SystemPulse.Converters;
public class MathConverter : IValueConverter {
    private Func<double, double>? _convertFormula;
    private Func<double, double>? _convertBackFormula;

    public string? Formula { set => _convertFormula = CompileFormula(value); }
    public string? BackFormula { set => _convertBackFormula = CompileFormula(value); }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var inputValue = System.Convert.ToDouble(value);
        return _convertFormula?.Invoke(inputValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        var inputValue = System.Convert.ToDouble(value);
        return _convertBackFormula?.Invoke(inputValue);
    }

    private static Func<double, double> CompileFormula(string? formula) {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(formula));

        var parameter = Expression.Parameter(typeof(double), "x");
        var tokens = Tokenize(formula!);
        var parser = new Parser(tokens, parameter);
        var expression = parser.Parse();
        var lambda = Expression.Lambda<Func<double, double>>(expression, parameter);
        return lambda.Compile();
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
                var name = formula.Substring(start, i - start);
                yield return new Token(TokenType.Identifier, name);
            } else {
                yield return c switch {
                    '+' => new Token(TokenType.Plus),
                    '-' => new Token(TokenType.Minus),
                    '*' => new Token(TokenType.Multiply),
                    '/' => new Token(TokenType.Divide),
                    '(' => new Token(TokenType.ParenLeft),
                    ')' => new Token(TokenType.ParenRight),
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
        End
    }

    private record Token(TokenType Type, string? Value = null);

    private class Parser {
        private static readonly Dictionary<string, double> Constants = new() {
            ["pi"] = double.Pi,
            ["inf"] = double.PositiveInfinity,
        };

        private static readonly Dictionary<string, Func<Expression, Expression>> Functions = new() {
            ["sqrt"] = arg => Expression.Call(typeof(double).GetMethod("Sqrt")!, arg),
            ["abs"] = arg => Expression.Call(typeof(double).GetMethod("Abs")!, arg),
            ["sin"] = arg => Expression.Call(typeof(double).GetMethod("Sin")!, arg),
            ["ceil"] = arg => Expression.Call(typeof(double).GetMethod("Ceiling")!, arg),
            ["floor"] = arg => Expression.Call(typeof(double).GetMethod("Floor")!, arg),
            ["round"] = arg => Expression.Call(typeof(double).GetMethod("Round", [typeof(double)])!, arg),
            ["trunc"] = arg => Expression.Call(typeof(double).GetMethod("Truncate")!, arg),
            ["exp"] = arg => Expression.Call(typeof(double).GetMethod("Exp")!, arg),
            ["log"] = arg => Expression.Call(typeof(double).GetMethod("Log", [typeof(double)])!, arg),
            ["cos"] = arg => Expression.Call(typeof(double).GetMethod("Cos")!, arg),
            ["tan"] = arg => Expression.Call(typeof(double).GetMethod("Tan")!, arg),
        };

        private readonly IEnumerator<Token> _tokens;
        private readonly ParameterExpression _parameter;
        private Token _currentToken;
        private Token? _nextToken;

        Token Peek() {
            if (_nextToken == null) {
                _tokens.MoveNext();
                _nextToken = _tokens.Current;
            }
            return _nextToken;
        }

        Token Advance() {
            _currentToken = Peek();
            _nextToken = null;
            return _currentToken;
        }

        Token Expect(TokenType tokenType) {
            var token = Advance();
            if (token.Type != tokenType)
                throw new InvalidOperationException();
            return token;
        }

        public Parser(IEnumerable<Token> tokens, ParameterExpression parameter) {
            _tokens = tokens.GetEnumerator();
            _parameter = parameter;
        }

        public Expression Parse() {
            return ParseExpression();
        }


        private Expression ParseExpression() => ParseAdditionExpression();

        private Expression ParseOperatorChain(Func<Expression> parseOperand, Func<TokenType, Func<Expression, Expression, Expression>?> toOperator) {
            var left = parseOperand();

            while (true) {
                var op = toOperator(Peek().Type);

                if (op is null)
                    break;

                Advance();
                left = op(left, parseOperand());
            }

            return left;
        }

        private Expression ParseAdditionExpression() => ParseOperatorChain(
            ParseMultiplicationExpression,
            t => t switch {
                TokenType.Plus => (left, right) => Expression.Add(left, right),
                TokenType.Minus => (left, right) => Expression.Subtract(left, right),
                _ => null
            }
        );
        private Expression ParseMultiplicationExpression() => ParseOperatorChain(
            ParsePrefixUnaryOperator,
            t => t switch {
                TokenType.Multiply => (left, right) => Expression.Multiply(left, right),
                TokenType.Divide => (left, right) => Expression.Divide(left, right),
                _ => null
            }
        );
        private Expression ParsePrefixUnaryOperator() {
            TokenType? mbop = Peek().Type switch {
                TokenType.Plus => TokenType.Plus,
                TokenType.Minus => TokenType.Minus,
                _ => null,
            };

            if (mbop is not { } op)
                return ParsePrimaryExpression();

            var opToken = Advance();
            var expr = ParsePrefixUnaryOperator();

            return mbop switch {
                TokenType.Plus => Expression.Negate(expr),
                TokenType.Minus => Expression.Negate(expr),
                _ => Expression.Empty()
            };

        }

        private Expression ParseParenExpression(Token? paren = null) {
            paren ??= Expect(TokenType.ParenLeft);
            var inner = ParseExpression();
            Expect(TokenType.ParenRight);

            return inner;
        }

        private Expression ParsePrimaryExpression() {
            var token = Advance();

            switch (token.Type) {
                case TokenType.NumberLiteral:
                    return Expression.Constant(double.Parse(token.Value));

                case TokenType.Identifier:
                    return ParseIdentifier(token);

                case TokenType.ParenLeft:
                    return ParseParenExpression(token);

                default:
                    throw new Exception($"Expected an expression, got a {token} token.");
            }
        }
        private Expression ParseIdentifier(Token token) {
            string iden = token.Value!;

            if (iden == "x") {
                return _parameter;
            }

            if (Constants.TryGetValue(iden, out var constantValue)) {
                return Expression.Constant(constantValue);
            }

            if (Functions.TryGetValue(iden, out var function)) {
                Expect(TokenType.ParenLeft);
                var argument = ParseExpression();
                Expect(TokenType.ParenRight);

                return function(argument);
            }

            throw new InvalidOperationException($"Unknown identifier: {iden}");
        }
    }
}
