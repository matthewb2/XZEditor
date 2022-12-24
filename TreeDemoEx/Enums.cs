using System;
using System.Collections.Generic;
using System.Text;

namespace TreeDemoEx
{
    public enum UpDownTraversalType
    {
        TopDown,
        BottomUp
    }

    public enum DepthBreadthTraversalType
    {
        DepthFirst,
        BreadthFirst
    }

    public enum NodeChangeType
    {
        NodeAdded,
        NodeRemoved
    }

    public enum NodeRelationType
    {
        Ancestor,
        Parent,
        Self,
        Child,
        Descendant
    }
}
