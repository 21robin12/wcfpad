class CompletionTreeItemFactory {
    public fromEndpointMethods(endpoint: Endpoint): CompletionTreeItem[] {
        return endpoint.methods.map((method) => {
            if (method.parameters.length === 0) {
                return CompletionTreeItem.Leaf(method.methodName, method.methodName + "({});", endpoint);
            }

            var insertTextLines = [
                method.methodName + "({"
            ];

            for (var i = 0; i < method.parameters.length; i++) {
                var inputParameter = method.parameters[i];
                this.addParameterInfo(insertTextLines, inputParameter, 1, i + 1 === method.parameters.length, false);
            }

            insertTextLines.push("});");

            return CompletionTreeItem.Leaf(method.methodName, insertTextLines.join("\n"), endpoint);
        });
    }

    private addParameterInfo(insertTextLines: string[], inputParameter: any, tabDepth: number, isLast: boolean, isArrayItem: boolean) {
        if (inputParameter.parameterType.isEnumerable) {
            // enumerable type
            insertTextLines.push(this.nTabs(tabDepth) + inputParameter.parameterName + ": [");
            var innerInputParameter = inputParameter.parameterType.members[0];
            this.addParameterInfo(insertTextLines, innerInputParameter, tabDepth + 1, true, true);
            insertTextLines.push(this.nTabs(tabDepth) + "]" + (isLast ? "" : ","));

        } else if (inputParameter.parameterType.members && inputParameter.parameterType.members.length > 0) {
            // parameter has children - output nested structure
            insertTextLines.push(this.nTabs(tabDepth) + (isArrayItem ? "{" : inputParameter.parameterName + ": {"));
            for (var i = 0; i < inputParameter.parameterType.members.length; i++) {
                var innerInputParameter = inputParameter.parameterType.members[i];
                this.addParameterInfo(insertTextLines, innerInputParameter, tabDepth + 1, i + 1 === inputParameter.parameterType.members.length, false);
            }

            insertTextLines.push(this.nTabs(tabDepth) + "}" + (isLast ? "" : ","));
        } else {
            // parameter has no children - output primitive
            insertTextLines.push(this.nTabs(tabDepth) + (isArrayItem ? "" : inputParameter.parameterName + ": ") + inputParameter.parameterType.defaultEditorValue + (isLast ? "" : ","))
        }
    }

    private nTabs(n: number) {
        var tabs = "";
        for (var i = 0; i < n; i++) {
            tabs += "\t";
        }

        return tabs;
    }
}