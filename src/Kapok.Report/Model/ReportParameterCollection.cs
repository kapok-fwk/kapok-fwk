using System.Collections;
using System.Collections.ObjectModel;

namespace Kapok.Report.Model;

public class ReportParameterCollection : ICollection<ReportParameter>
{
    private readonly Dictionary<string, ReportParameter> _reportParameters = new();
        
    public ReportParameter Add(string name, Type dataType, object? defaultValue = default)
    {
        var newParameter = new ReportParameter(name, dataType)
        {
            DefaultValue = defaultValue
        };

        Add(newParameter);
        return newParameter;
    }

    public void Add(ReportParameter parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));

        if (string.IsNullOrEmpty(parameter.Name))
            throw new ArgumentException($"The parameter.Name {nameof(parameter.Name)} can not be null or empty.");

        if (_reportParameters.Values.Any(p => p.Name == parameter.Name))
            throw new ArgumentException($"A parameter with name {parameter.Name} has already been added to the report collection.", nameof(parameter));
            
        _reportParameters.Add(parameter.Name, parameter);
    }

    public void Clear()
    {
        _reportParameters.Clear();
    }

    public int Count => _reportParameters.Count;
    public bool IsReadOnly => false;

    public bool Contains(string parameterName)
    {
        return _reportParameters.ContainsKey(parameterName);
    }

    public bool Remove(string parameterName)
    {
        return _reportParameters.Remove(parameterName);
    }

    public bool TryGetValue(string parameterName, out ReportParameter? parameter)
    {
        return _reportParameters.TryGetValue(parameterName, out parameter);
    }

    public ReportParameter this[string parameterName]
    {
        get => _reportParameters[parameterName];
        set => _reportParameters[parameterName] = value;
    }

    public IEnumerator<ReportParameter> GetEnumerator()
    {
        return _reportParameters.Values.GetEnumerator();
    }

    #region ICollection<ReportParameter>

    bool ICollection<ReportParameter>.Contains(ReportParameter item)
    {
        return _reportParameters.Values.Contains(item);
    }

    void ICollection<ReportParameter>.CopyTo(ReportParameter[] array, int arrayIndex)
    {
        _reportParameters.Values.CopyTo(array, arrayIndex);
    }

    bool ICollection<ReportParameter>.Remove(ReportParameter? item)
    {
        if (item == null)
            return false;

        return Remove(item.Name);
    }

    #endregion

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Returns the parameters as readonly dictionary.
    /// </summary>
    /// <returns>
    /// The returned dictionary is readonly. The key is the parameter name and the dictionary value
    /// is the parameter value.
    /// </returns>
    public IReadOnlyDictionary<string, object?> ToDictionary()
    {
#pragma warning disable CS8620
        return new ReadOnlyDictionary<string, object?>(
            (from p in _reportParameters
             select new
             {
                 p.Key,
                 p.Value.Value
             }).ToDictionary(p => p.Key, p => p.Value)
            );
#pragma warning restore CS8620
    }
}