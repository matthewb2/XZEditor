using System;
using System.Collections.Generic;
using System.Text;

namespace TreeDemoEx
{
    public interface ITreeNodeAware<T>
      where T : new()
    {
        TreeNode<T> Node { get; set; }
    }
}
