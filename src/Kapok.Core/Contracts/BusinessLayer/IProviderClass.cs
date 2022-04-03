using Newtonsoft.Json.Linq;

namespace Kapok.Core;

/// <summary>
/// An interface which needs to be used for classes which provide a defined functionality. Normally
/// such class need to inherit a basis class.
///
/// Sample:
///
/// abstract class NumberSequenceCalculationMethod : IProviderClass
///    * abstract string GetNextNum(int lastNumericNum, string lastStringNum) - calculates the next number
///    - this class offers a basis class to implement an individual calculation method for number sequences
///
/// class NumericNumberSequenceCalculationMethod : NumberSequenceCalculationMethod
///    - this class offers a method to calculate the next number based on an numeric increment
/// </summary>
public interface IProviderClass
{
    /// <summary>
    /// The name of the provider visible to the user
    /// </summary>
    Caption ProviderName { get; }

    /// <summary>
    /// Parameters for this provider, provided in an Json object
    /// </summary>
    JObject Parameter { get; set; }
}