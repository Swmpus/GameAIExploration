public abstract class TreeNode
{
    protected TreeNode[] children;

    protected TreeNode() { children = null; }

    protected TreeNode(TreeNode[] children) { this.children = children; }

    public abstract void ExecuteNode(TreeState state);
}
