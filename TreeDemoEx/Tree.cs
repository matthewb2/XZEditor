﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TreeDemoEx
{
    public class Tree<T> : TreeNode<T>
       where T : new()
    {
        public Tree() { }

        public Tree(T RootValue)
        {
            Value = RootValue;
        }
    }
}
