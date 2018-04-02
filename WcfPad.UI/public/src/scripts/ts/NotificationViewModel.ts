/// <reference path="typings/knockout.d.ts" />

class NotificationViewModel {
    public opacity: KnockoutObservable<number> = ko.observable(0);  
    private intervalId: number;

    notify() {
        clearInterval(this.intervalId);

        this.opacity(1);

        this.intervalId = setInterval(() => {
            var opacity = this.opacity();
            opacity -= 0.02;
            if (opacity < 0) {
                this.opacity(0);
                clearInterval(this.intervalId);
            } else {
                this.opacity(opacity);
            }
        }, 20);
    }
}