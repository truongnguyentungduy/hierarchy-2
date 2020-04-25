using UnityEngine.UIElements;

namespace Hierarchy2
{
    internal interface IHierarchyShelf
    {
        int ShelfPriority();
        VisualElement CreateShelfElement();
    }
}