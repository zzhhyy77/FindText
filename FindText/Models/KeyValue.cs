namespace FindText.Models
{
    public class KeyValue : VModelsBase
    {

        string _key;
        string _value;

        public string Key
        {
            get
            {
                return this._key;
            }
            set
            {
                this._key = value;
            }
        }

        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                RaisePropertyChanged();
            }
        }

        public static KeyValue New(string key, string value)
        {
            return new KeyValue() { Key = key, Value = value };
        }

    }
}
