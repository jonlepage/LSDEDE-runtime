using System;
using UnityEngine;

namespace Cainos.LucidEditor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public class GroupAttribute : PropertyGroupAttribute
    {
        public GroupAttribute(string groupName)
            : base(groupName) { }
    }
}
