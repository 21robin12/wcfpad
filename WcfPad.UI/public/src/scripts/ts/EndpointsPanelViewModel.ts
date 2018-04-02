/// <reference path="typings/knockout.d.ts" />

declare var ko: KnockoutStatic;

class EndpointsPanelViewModel {
    public endpoints: KnockoutObservableArray<Endpoint> = ko.observableArray([]);
    private clipboard: Clipboard = new Clipboard();
    private notificationViewModel: NotificationViewModel = new NotificationViewModel();

    private mainPageViewModel: MainPageViewModel;
    private resultsPanelViewModel: ResultsPanelViewModel;

    constructor(resultsPanelViewModel: ResultsPanelViewModel, mainPageViewModel: MainPageViewModel) {
        this.resultsPanelViewModel = resultsPanelViewModel;
        this.mainPageViewModel = mainPageViewModel;
    }

    public addEndpoint(endpoint: Endpoint): void {
        endpoint.hasFocus.subscribe((hasFocus: boolean) => {
            if (!hasFocus) {
                this.ensureEndpointNamesValid();
                endpoint.saveEndpointName();
            }
        });

        this.endpoints.push(endpoint);
        this.ensureEndpointNamesValid();
        this.notificationViewModel.notify();
    }

    public deleteEndpoint(index: number) {
        var endpoint = this.endpoints()[index];
        var address = endpoint.address;
        var name = endpoint.name();
        this.endpoints.splice(index, 1);

        ajax.execute("MainPageController", "DeleteEndpoint", [endpoint.id], () => {
            this.resultsPanelViewModel.addResult("Endpoint '" + name + "' with address " + address + " was deleted");
        });
    }

    public copyAddressToClipboard(index: number) {
        var endpoint = this.endpoints()[index];
        this.clipboard.copy(endpoint.address);
        this.resultsPanelViewModel.addResult("Endpoint address " + endpoint.address + " was copied to clipboard")
    } 

    public openClientFolder(index: number) {
        var endpoint = this.endpoints()[index];
        ajax.execute("MainPageController", "OpenClientFolder", [endpoint.id], (success: any) => {
            if (!success) {
                this.resultsPanelViewModel.addError("Could not open folder containing WCF client for endpoint with address " + endpoint.address);
            }
        });
    }

    public rebuildWcfClient(index: number) {
        var endpoint = this.endpoints()[index];
        var address = endpoint.address;

        var endpointDirectories = [];

        for (var i = 0; i < this.endpoints().length; i++) {
            if (this.endpoints()[i].address === address && endpointDirectories.indexOf(this.endpoints()[i].endpointDirectory) < 0) {
                endpointDirectories.push(this.endpoints()[i].endpointDirectory);
            }
        }

        var endpointsToRebuild = ko.utils.arrayFilter(this.endpoints(), (item) => {
            return endpointDirectories.indexOf(item.endpointDirectory) > -1;
        });

        ko.utils.arrayForEach(endpointsToRebuild, (item) => {
            var index = this.endpoints().indexOf(item);
            this.endpoints().splice(index, 1);
        });

        // to force UI update - KO doesn't see splice()
        this.endpoints(this.endpoints());
        
        this.mainPageViewModel.addOrRebuildEndpoint(address, true);
    }

    private ensureEndpointNamesValid(): void {
        var names: string[] = [];
        for (var i = 0; i < this.endpoints().length; i++) {
            var hasNameChanged = false;
            var endpoint: Endpoint = this.endpoints()[i];

            if (endpoint.name() === "") {
                endpoint.name("endpoint");
                hasNameChanged = true;
            }

            var name = this.getUniqueName(names, endpoint.name());
            if (name !== endpoint.name()) {
                endpoint.name(name);
                hasNameChanged = true;
            }

            if (hasNameChanged) {
                endpoint.saveEndpointName();
            }

            names.push(name);
        }
    }

    private getUniqueName(names: string[], name: string, originalName: string = name, suffixNumber: number = 2) {
        if (names.indexOf(name) > -1) {
            // name already taken - need to make unique
            var newName = originalName + suffixNumber.toString();

            return this.getUniqueName(names, newName, originalName, suffixNumber + 1);
        }

        return name;
    }
}