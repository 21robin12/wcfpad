/// <reference path="typings/knockout.d.ts" />

declare var ko: KnockoutStatic;
declare var monaco: any;
declare var require: any;
declare var ajax: Ajax;

class MainPageViewModel {

    private editor: any;

    private address: KnockoutObservable<string> = ko.observable("");
    private isLoading: KnockoutObservable<boolean> = ko.observable(false);
    private selectedTabIndex: KnockoutObservable<number> = ko.observable(0);   

    private resultsPanelViewModel: ResultsPanelViewModel = new ResultsPanelViewModel();
    private endpointsPanelViewModel: EndpointsPanelViewModel = new EndpointsPanelViewModel(this.resultsPanelViewModel, this);
    private completionItemProvider: CustomCompletionItemProvider = new CustomCompletionItemProvider(this.resultsPanelViewModel, this.endpointsPanelViewModel.endpoints);
     

    constructor() {
        this.initializeEditor();

        document.onkeydown = (e) => {
            if (e.key === "F5") {
                this.invoke();
            }
        }

        this.loadEndpointsFromSettings();
    }

    public invoke() {
        // required - must be in scope for subsequent eval(...) statement
        var _endpoints = this.completionItemProvider.getServicesObject();

        try {
            this.resultsPanelViewModel.addResult("Waiting...");
            eval(this.editor.getValue());
        } catch (e) {
            this.resultsPanelViewModel.addError(e.message);
        }        
    };

    public onEndpointInputKeyPress(context, e) {
        if (e.key === "Enter") {
            this.addEndpoint();
            return false;
        } else {
            return true;
        }
    }

    public addEndpoint() {
        if (!this.isLoading() && this.address()) {
            var existingEndpoint = ko.utils.arrayFirst(this.endpointsPanelViewModel.endpoints(), (item) => {
                return item.address === this.address();
            });

            if (existingEndpoint) {
                // already exists - don't re-add
                this.resultsPanelViewModel.addResult("Endpoint with address " + this.address() + " already exists with name '" + existingEndpoint.name() + "'");
            } else {
                // doesn't exist - parse and add to list
                var address = this.address();
                this.address("");
                this.addOrRebuildEndpoint(address);
            }
        }
    };

    public addOrRebuildEndpoint(address: string, rebuild: boolean = false) {
        this.isLoading(true);
        ajax.execute("MainPageController", rebuild ? "RebuildWcfClient" : "AddEndpoints", [address], (res: any) => {
            var parsed = JSON.parse(res);
            if (parsed.isError) {
                this.resultsPanelViewModel.addError(parsed.message);
            } else if (parsed && parsed.length) {
                this.addEndpointsToViewModel(parsed);
                this.resultsPanelViewModel.addResult("Successfully " + (rebuild ? "rebuilt " : "added ") + parsed.length + " endpoints at address " + address);
            }

            this.isLoading(false);
        });
    }

    private addEndpointsToViewModel(endpoints: any) {
        for (var i = 0; i < endpoints.length; i++) {
            var endpoint = new Endpoint(endpoints[i]);
            this.endpointsPanelViewModel.addEndpoint(endpoint);
        }
    }

    public selectTab(index: number) {
        this.selectedTabIndex(index);
    }

    private initializeEditor() {
        require.config({ paths: { 'vs': 'public/dist/vs' } });
        require(['vs/editor/editor.main'], () => {
            this.editor = monaco.editor.create(document.getElementById('monaco-container'), {
                value: "\/\/ Begin by adding an endpoint, and then type '_endpoints.' in this editor to view the available endpoints \n",
                language: 'javascript',
                theme: "vs-dark",
                automaticLayout: true,
                wordWrap: "on",
                minimap: {
                    enabled: false
                }
            });

            setTimeout(() => {
                this.editor.setPosition({ column: 0, lineNumber: 2 });
            }, 0);

            monaco.languages.typescript.javascriptDefaults.setCompilerOptions({ noLib: true, allowNonTsExtensions: true });
            monaco.languages.registerCompletionItemProvider('javascript', this.completionItemProvider);
        });
    }

    private loadEndpointsFromSettings() {
        this.isLoading(true);
        ajax.execute("MainPageController", "LoadEndpointsFromSettings", null, (res: any) => {
            var parsed = JSON.parse(res);
            if (parsed.isError) {
                this.resultsPanelViewModel.addError(parsed.message);
            } else if (parsed && parsed.length) {
                this.addEndpointsToViewModel(parsed);
                this.resultsPanelViewModel.addResult("Successfully loaded endpoints");
            }

            this.isLoading(false);
        });
    }
}