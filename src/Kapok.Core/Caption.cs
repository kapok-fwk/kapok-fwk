using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;

// ReSharper disable UnusedMember.Local

namespace Kapok;

/// <summary>
/// Represents a description that can be defined in multiple languages.
/// </summary>
[JsonConverter(typeof(CaptionJsonSerializer))]
public class Caption : IXmlSerializable, ICollection<KeyValuePair<string, string>>, ICloneable, IEquatable<Caption>
{
    private readonly Dictionary<string, string> _captionPerLanguage = new();

    /// <summary>
    /// String which is used when no language is defined. This string identifies
    /// as well the default language when a caption for language could not be found.
    /// </summary>
    public static readonly string EmptyLanguage = string.Empty;

    public string this[string language]
    {
        get => _captionPerLanguage[language];
        set
        {
            if (_captionPerLanguage.ContainsKey(language))
                _captionPerLanguage[language] = value;
            else
                _captionPerLanguage.Add(language, value);
        }
    }

    public string this[CultureInfo culture]
    {
        get => this[culture.Name];
        set => this[culture.Name] = value;
    }

    public static implicit operator Caption(string value)
    {
        var o = new Caption();
        o._captionPerLanguage.Add(EmptyLanguage, value);

        return o;
    }

    public string? LanguageOrDefault(CultureInfo cultureInfo)
    {
        return LanguageOrDefault(cultureInfo.Name);
    }

    public string? LanguageOrDefault(string language)
    {
        if (_captionPerLanguage.Count == 0)
            return null;

        if (_captionPerLanguage.ContainsKey(language))
            return _captionPerLanguage[language];

        if (_captionPerLanguage.ContainsKey(CultureInfo.GetCultureInfo(language).Name))
            return _captionPerLanguage[CultureInfo.GetCultureInfo(language).Name];

        if (_captionPerLanguage.ContainsKey(EmptyLanguage))
            return _captionPerLanguage[EmptyLanguage];

        return _captionPerLanguage.First().Value;
    }

    /// <summary>
    /// Returns true when this caption has no content at all.
    /// </summary>
    public bool IsEmpty => _captionPerLanguage.Count == 0;

    public override string ToString()
    {
        return LanguageOrDefault(CultureInfo.CurrentCulture) ?? string.Empty;
    }

    public object Clone()
    {
        var newObject = new Caption();
        newObject._captionPerLanguage.AddRange(this._captionPerLanguage);
        return newObject;
    }

    void Clear()
    {
        _captionPerLanguage.Clear();
    }

    // NOTE: this is public to make object-initiation possible with { {language, caption}, {language, caption}, ... } equal to an dicitionary
    public void Add(string language, string caption)
    {
        _captionPerLanguage.Add(language, caption);
    }

    bool Remove(string language)
    {
        return _captionPerLanguage.Remove(language);
    }

    System.Xml.Schema.XmlSchema? IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
    {
        while (reader.Read() && reader.Name.Equals("Caption") && reader.NodeType == System.Xml.XmlNodeType.Element)
        {
            string lang = reader.GetAttribute("lang") ?? EmptyLanguage;
            string caption = reader.ReadElementContentAsString();

            Add(lang, caption);
        }
    }

    void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
    {
        foreach (KeyValuePair<string, string> captionPair in _captionPerLanguage)
        {
            writer.WriteStartElement("Caption");

            writer.WriteAttributeString("lang", captionPair.Key);
            writer.WriteCData(captionPair.Value);

            writer.WriteEndElement();
        }
    }

    #region ICollection<KeyValuePair<string, string>>

    bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
    {
        return ((ICollection<KeyValuePair<string, string>>) _captionPerLanguage).Remove(item);
    }

    int ICollection<KeyValuePair<string, string>>.Count => Count;

    bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

    int Count => _captionPerLanguage.Count;

    void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
    {
        _captionPerLanguage.Add(item.Key, item.Value);
    }

    void ICollection<KeyValuePair<string, string>>.Clear()
    {
        Clear();
    }

    bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
    {
        return ((ICollection<KeyValuePair<string, string>>) _captionPerLanguage).Contains(item);
    }

    void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, string>>) _captionPerLanguage).CopyTo(array, arrayIndex);
    }

    #endregion

    #region IEnumerator<KeyValuePair<string, string>>

    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
    {
        return _captionPerLanguage.GetEnumerator();
    }

    #endregion

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    { 
        return _captionPerLanguage.GetEnumerator();
    }

    #endregion

    public bool Equals(Caption? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(_captionPerLanguage, other._captionPerLanguage);
    }

    public override bool Equals(object? obj)
    {
        return obj is Caption role && Equals(role);
    }
        
    public static bool operator ==(Caption? left, Caption? right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(Caption? left, Caption? right)
    {
        if (ReferenceEquals(left, null))
        {
            return !ReferenceEquals(right, null);
        }

        return !left.Equals(right);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = GetType().FullName?.GetHashCode() ?? 0;

            hash = (hash * 7) ^ _captionPerLanguage.GetHashCode();

            return hash;
        }
    }
}

public static class CaptionExtensions
{
    public static Caption ToCaption(this PropertyInfo propertyInfo)
    {
        var caption = new Caption();

        var displayName = propertyInfo.GetDisplayAttributeNameOrDefault();

        caption[CultureInfo.CurrentUICulture] = displayName;

        return caption;
    }
}

public class CaptionJsonSerializer : JsonConverter<Caption>
{
    public override void WriteJson(JsonWriter writer, Caption? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var dict = new Dictionary<string, string>(value);

        serializer.Serialize(writer, dict);
    }

    public override Caption ReadJson(JsonReader reader, Type objectType, Caption? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        JToken jsonToken = JToken.Load(reader);

        if (jsonToken.Type == JTokenType.Array)
        {
            // Legacy support: Data is saved in the following format:
            // [{"Key": "en-US", "Value": "<text>"}, ...]

            var caption = new Caption();

            foreach (var items in jsonToken.ToList())
            {
                var pair = items.ToObject<KeyValuePair<string, string>>();

                caption.Add(pair.Key, pair.Value);
            }

            return caption;
        }

        if (jsonToken.Type == JTokenType.Object)
        {
            // JSON formant:
            // {"en-US": "<text>", ...}

            var jsonObject = (JObject)jsonToken;

            var caption = new Caption();

            foreach (var jsonProperty in jsonObject.Properties().ToList())
            {
                caption.Add(jsonProperty.Name, jsonProperty.Value.ToString());
            }

            return caption;
        }

        if (jsonToken.Type == JTokenType.Null)
            return new Caption();

        throw new JsonException($"Unexpected json token for object {typeof(Caption).FullName}: {jsonToken}");
    }
}