using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Admin.Models
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class SelectOption
    {
        [JsonProperty] public string Key { get; set; }
        [JsonProperty] public string Value { get; set; }

        public SelectOption(KeyValuePair<string, string> keyValuePair) : this(keyValuePair.Key, keyValuePair.Value) { }
        public SelectOption(KeyValuePair<int, string> keyValuePair) : this(keyValuePair.Key, keyValuePair.Value) { }
        public SelectOption(KeyValuePair<Enum, string> keyValuePair) : this(keyValuePair.Key, keyValuePair.Value) { }
        public SelectOption(KeyValuePair<Guid, string> keyValuePair) : this(keyValuePair.Key, keyValuePair.Value) { }
        public SelectOption(KeyValuePair<Guid?, string> keyValuePair) : this(keyValuePair.Key, keyValuePair.Value) { }
        public SelectOption(string key, string value) { Key = key; Value = value; }
        public SelectOption(int key, string value) { Key = key.ToString(); Value = value; }
        public SelectOption(Enum key, string value) { Key = key.ToString(); Value = value; }
        public SelectOption(Guid key, string value) { Key = key.ToString(); Value = value; }
        public SelectOption(Guid? key, string value) { Key = key.ToString(); Value = value; }

        public override bool Equals(object obj)
        {
            return obj is SelectOption other && Key == other.Key && Value == other.Value;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class SelectOption<T> : SelectOption
    {
        [JsonIgnore]
        private T _keyTyped;
        [JsonIgnore]
        public T KeyTyped { get => _keyTyped; set { _keyTyped = value; Key = value.ToString(); } }
        
        public SelectOption(KeyValuePair<T, string> keyValuePair) : this(keyValuePair.Key, keyValuePair.Value) { }
        public SelectOption(T key, string value) : base(key.ToString(), value) { KeyTyped = key; }
    }
}
