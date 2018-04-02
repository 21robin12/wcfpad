/// <reference path="typings/knockout.d.ts" />

declare var ko: KnockoutStatic;

class ParameterType {
    public defaultEditorValue: string;
    public isEnumerable: boolean;
    public members: Parameter[];
}

class Parameter {
    public parameterName: string;
    public parameterType: ParameterType;
}

class Method {
    public methodName: string;
    public parameters: Parameter[];
    public outParameters: Parameter[]; // e.g. "out int myvar" parameters AND (return) if non-void  
}
 
class Endpoint {
    public id: string;
    public address: string;
    public methods: Method[];
    public name: KnockoutObservable<string> = ko.observable("");
    public endpointDirectory: string;

    private savedName: string;

    public hasFocus: KnockoutObservable<boolean> = ko.observable(false);

    public hoverCopy: KnockoutObservable<boolean> = ko.observable(false);
    public hoverRebuild: KnockoutObservable<boolean> = ko.observable(false);
    public hoverOpenFolder: KnockoutObservable<boolean> = ko.observable(false);
    public hoverDelete: KnockoutObservable<boolean> = ko.observable(false);

    public showPopover: KnockoutComputed<boolean> = ko.pureComputed(() => {
        return this.hoverCopy() || this.hoverRebuild() || this.hoverOpenFolder() || this.hoverDelete();
    });

    public popoverText: KnockoutComputed<string> = ko.pureComputed(() => {
        if (this.hoverCopy()) {
            return "Copy endpoint address to the clipboard";
        }

        if (this.hoverRebuild()) {
            return "Rebuild WCF client for this endpoint. Use when WCF service has changed";
        }

        if (this.hoverOpenFolder()) {
            return "Open folder containing WCF client";
        }

        if (this.hoverDelete()) {
            return "Delete endpoint";
        }

        return null;
    });

    constructor(endpointObject: any) {
        this.id = endpointObject.id;
        this.address = endpointObject.definition.address;
        this.name(endpointObject.definition.displayName);
        this.savedName = endpointObject.definition.displayName;
        this.methods = this.mapMethod(endpointObject.methods);
        this.endpointDirectory = endpointObject.definition.endpointDirectory;

        this.name.subscribeChanged((oldValue, newValue) => {
            if (newValue !== "" && !/^[_a-zA-Z][_a-zA-Z0-9]*$/.test(newValue)) {
                this.name(oldValue);
            }
        });
    }

    public saveEndpointName() {
        ajax.execute("MainPageController", "RenameEndpoint", [this.id, this.name()], (success: any) => { });
        this.savedName = this.name();
    }

    private mapMethod(methods: any[]): Method[] {
        if (methods === null) {
            return null;
        }

        return methods.map((method) => {
            var endpointMethod = new Method();
            endpointMethod.methodName = method.methodName;
            endpointMethod.parameters = this.mapParameters(method.parameters);
            endpointMethod.outParameters = this.mapParameters(method.outParameters);

            return endpointMethod;
        });
    }

    private mapParameters(parameters: any[]): Parameter[] {
        if (parameters === null) {
            return null;
        }

        return parameters.map((parameter) => {
            var endpointMethodParameter = new Parameter();
            endpointMethodParameter.parameterName = parameter.parameterName;

            endpointMethodParameter.parameterType = new ParameterType();
            endpointMethodParameter.parameterType.defaultEditorValue = parameter.parameterType.defaultEditorValue;
            endpointMethodParameter.parameterType.isEnumerable = parameter.parameterType.isEnumerable;
            endpointMethodParameter.parameterType.members = this.mapParameters(parameter.parameterType.members);

            return endpointMethodParameter;
        });
    }
}