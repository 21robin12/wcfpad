declare var csharpAjax: any;

class Ajax {

    private pending: any = {};

    public execute(controllerName: string, methodName: string, data: any, callback: any) {
        var guid = this.guid();
        this.pending[guid] = callback;
        csharpAjax.execute(guid, controllerName, methodName, data);
    }

    public resolve(guid: string, data: any) {
        var callback = this.pending[guid];
        if (callback) {
            callback(data);
        }

        delete this.pending[guid];
    }

    private guid() {
        var s4 = this.s4;
        return s4() + s4() + "-" + s4() + "-" + s4() + "-" +
            s4() + "-" + s4() + s4() + s4();
    }

    private s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }
}

// global (since we need access from C#)
var ajax = new Ajax();