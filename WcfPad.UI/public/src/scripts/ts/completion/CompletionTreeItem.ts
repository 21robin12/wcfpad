class CompletionTreeItem
{
    public label: string;
    public children: CompletionTreeItem[];
    public insertText: string;
    public endpoint: Endpoint;
    // kind: monaco.languages.CompletionItemKind.Property,
    // detail: "detail here",
    // documentation: "documentation"

    public static Node(label: string, children: CompletionTreeItem[])
    {
        var item = new CompletionTreeItem();
        item.label = label;
        item.children = children;
        return item;
    }

    public static Leaf(label: string, insertText: string, endpoint: Endpoint)
    {
        var item = new CompletionTreeItem();
        item.label = label;
        item.insertText = insertText;
        item.endpoint = endpoint;
        return item;
    }
}