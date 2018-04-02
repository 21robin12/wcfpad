/// <reference path="../typings/knockout.d.ts" />

declare var ko: KnockoutStatic;

class MonacoCompletionItem {
    public label: string;
    public insertText: string;

    constructor(from: CompletionTreeItem) {
        this.label = from.label;
        this.insertText = from.insertText;
    }
}

interface ICompletionItemProvider {
    // https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.completionitemprovider.html
    triggerCharacters: string[];
    provideCompletionItems(model: any, position: any): MonacoCompletionItem[];
}

interface IServiceMethod {
    (parameters: any): void;
    methodName: string;
    endpoint: Endpoint;
}

class CustomCompletionItemProvider implements ICompletionItemProvider {

    public triggerCharacters: string[] = ["."];
    private completionTreeItemFactory: CompletionTreeItemFactory = new CompletionTreeItemFactory();
    private resultsPanelViewModel: ResultsPanelViewModel;
    private endpoints: any;

    constructor(resultsPanelViewModel: ResultsPanelViewModel, endpoints: any) {
        this.resultsPanelViewModel = resultsPanelViewModel;
        this.endpoints = endpoints;
    }

    private completionTree: KnockoutComputed<CompletionTreeItem> = ko.pureComputed(() => {
        var children = this.endpoints().map((endpoint: Endpoint) => {
            var methods = this.completionTreeItemFactory.fromEndpointMethods(endpoint);
            return CompletionTreeItem.Node(endpoint.name(), methods);
        });

        var root = CompletionTreeItem.Node("_endpoints", children);
        return root;
    });

    public getServicesObject() {
        var _endpoints = {};
        this.addChildren(_endpoints, this.completionTree().children);
        return _endpoints;
    }

    // TODO prevent autocomplete inside a string, e.g. "_endpoints." should not display a dropdown
    public provideCompletionItems(model: any, position: any) {
        // see https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.completionitem.html

        var line = model._lines[position.lineNumber - 1].text;
        var justTypedDot = line[position.column - 2] === ".";
        if (justTypedDot) {
            // just typed a dot - look through dot sequence 
            var root = this.completionTree().label;
            var currentDotSequence = this.getCurrentDotSequence(line);
            var sequenceElements = currentDotSequence.split(".");

            var currentTreeItems = [this.completionTree()];

            // don't consider last sequence element - it will always be "" since we just typed a dot
            for (var i = 0; i < sequenceElements.length - 1; i++) {
                var currentSequenceElement = sequenceElements[i];
                var matchingTreeItem = this.findTreeItemMatching(currentSequenceElement, currentTreeItems);

                if (matchingTreeItem === null) {
                    // our sequence has deviated from the tree => don't display autocomplete
                    return [];
                }

                currentTreeItems = matchingTreeItem.children;
            }

            return this.extractMonacoCompletionItems(currentTreeItems);
        } else {
            // didn't type a dot - must be top-level sequence
            return this.extractMonacoCompletionItems([this.completionTree()]);
        }
    };

    private getCurrentDotSequence(line: string) {
        // TODO enable autocomplete when dot sequence is split over multiple lines
        var dotSequences = line.match(/[_a-zA-Z0-9\.]+/g);
        if (dotSequences === null || dotSequences.length === 0) {
            return "";
        }

        return dotSequences[dotSequences.length - 1];
    }

    private extractMonacoCompletionItems(treeItems: CompletionTreeItem[]) {
        return treeItems ? treeItems.map((ti) => { return new MonacoCompletionItem(ti); }) : [];
    }

    private findTreeItemMatching(label: string, treeItems: CompletionTreeItem[]) {   
        if (!treeItems) {
            return null;
        }

        var matching = treeItems.filter((ti) => { return ti.label === label });
        return matching && matching.length > 0 ? matching[0] : null;
    }

    private addChildren(o: any, children: CompletionTreeItem[]) {
        for (var i = 0; i < children.length; i++) {
            var child = children[i];

            if (child.children) {
                // has children - add
                var newObj = {};
                this.addChildren(newObj, child.children);
                o[child.label] = newObj;
            } else {
                // no children - is a method
                o[child.label] = this.newServiceMethod(child.label, child.endpoint);
            }
        }
    }

    private newServiceMethod(methodName: string, endpoint: Endpoint): IServiceMethod {
        // weird way of defining an object - like plain JS. has to be like this to have a function with properties (and has to be a function so we can call it via `eval`)
        // see https://stackoverflow.com/questions/12766528/build-a-function-object-with-properties-in-typescript
        var serviceMethod: any = (parameters) => {
            ajax.execute("MainPageController", "Invoke", [serviceMethod.endpoint.id, serviceMethod.methodName, JSON.stringify(parameters)], (res) => {
                var parsed = JSON.parse(res);
                if (parsed.isError) {
                    this.resultsPanelViewModel.addError(parsed.message);
                } else {
                    this.resultsPanelViewModel.addResultFromObject(res);
                }
            });
        };

        serviceMethod.methodName = methodName;
        serviceMethod.endpoint = endpoint;

        return serviceMethod;
    }
}