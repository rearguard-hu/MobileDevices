using Claunia.PropertyList;

namespace MobileDevices.iOS.PropertyLists
{
    /// <summary>
    /// A common interface for all classes which represent data which can be deserialized from a property list.
    /// </summary>
    public interface IPropertyListDeserializable
    {
        /// <summary>
        /// Deserializes this object from a <see cref="NSDictionary"/>.
        /// </summary>
        /// <param name="dictionary">
        /// A <see cref="NSDictionary"/> which represents the object to deserialize.
        /// </param>
        public void FromDictionary(NSDictionary dictionary);
    }
}
