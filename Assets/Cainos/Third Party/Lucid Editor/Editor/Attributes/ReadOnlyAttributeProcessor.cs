using Cainos.LucidEditor;
using UnityEditor;

namespace Cainos.LucidEditor
{
    [CustomAttributeProcessor(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeProcessor : PropertyProcessor
    {
        public override void OnBeforeDrawProperty()
        {
            property.isEditable = false;
        }
    }
}
