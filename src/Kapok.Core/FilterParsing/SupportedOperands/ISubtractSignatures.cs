﻿namespace Kapok.BusinessLayer.FilterParsing.SupportedOperands;

internal interface ISubtractSignatures : IAddSignatures
{
    void F(DateTime x, DateTime y);
    void F(DateTime? x, DateTime? y);
}