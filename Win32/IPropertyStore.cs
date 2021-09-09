
namespace SharpGen.Runtime.Win32
{
    public partial class IPropertyStore
    {
        public Variant GetValue(PropertyKey key) => GetValue(ref key);

        public void SetValue(PropertyKey key, Variant value) => SetValue(ref key, value);
    }
}