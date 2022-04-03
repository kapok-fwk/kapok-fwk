namespace Kapok.Core.FilterParsing.SupportedOperands;

internal interface ILogicalSignatures
{
    void F(bool x, bool y);
    void F(bool? x, bool? y);
}