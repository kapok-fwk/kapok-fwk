namespace Kapok.Core.FilterParsing;

internal enum TokenId
{
    Unknown,
    End,
    Identifier,
    StringLiteral,
    IntegerLiteral,
    RealLiteral,
    Exclamation,
    Percent,
    Amphersand,
    OpenParen,
    CloseParen,
    Asterisk,
    Plus,
    Minus,
    Slash,
    LessThan,
    Equal,
    GreaterThan,
    Bar,
    ExclamationEqual,
    LessThanEqual,
    LessGreater,
    GreaterThanEqual,
    DoubleGreaterThan,
    DoubleLessThan,
    Tilde,
    ExclamationTilde
}