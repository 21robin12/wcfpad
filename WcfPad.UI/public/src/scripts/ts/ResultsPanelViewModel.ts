/// <reference path="typings/knockout.d.ts" />

declare var ko: KnockoutStatic;

class ResultModel {
    text: string;
    isError: boolean;
    time: string;

    constructor(text: string, isError: boolean = false) {
        this.text = text;
        this.isError = isError;

        var date = new Date();
        this.time = this.leftPad(date.getHours(), 2) + ":"
			+ this.leftPad(date.getMinutes(), 2) + ":" 
			+ this.leftPad(date.getSeconds(), 2) + "." 
			+ this.leftPad(date.getMilliseconds(), 3);
    }
	
	private leftPad(n: number, digitsCount: number): string {
		var pad = "";
		for (var i = 0; i < digitsCount; i++) {
			pad += "0";
		}		
		
		var str = "" + n;
		return pad.substring(0, pad.length - str.length) + str;
	}
}

class ResultsPanelViewModel {
    private results: KnockoutObservableArray<ResultModel> = ko.observableArray([]); // TODO maximum number of results? when does the UI start to lag?
    private notificationViewModel: NotificationViewModel = new NotificationViewModel();

    public addResult(text: string) {
        this.results.push(new ResultModel(text));
        this.scrollToBottom();
        this.notificationViewModel.notify();
    }

    public addError(text: string) {
        this.results.push(new ResultModel(text, true));
        this.scrollToBottom();
        this.notificationViewModel.notify();
    }

    public addResultFromObject(object: any) {
        var formatted = JSON.stringify(JSON.parse(object), null, 4);
        this.addResult(formatted);
    }

    private scrollToBottom() {
        var resultsPanel = document.getElementById("results-panel");
        resultsPanel.scrollTop = resultsPanel.scrollHeight;
    }
}