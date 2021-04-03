using System;

namespace Hierarchy2
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayOnHierarchyAttribute : Attribute
    {
        public string tooltip = "";
    }
}