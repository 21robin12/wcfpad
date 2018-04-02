/// <reference path="typings/knockout.d.ts" />

declare var ko: KnockoutStatic;

ko.subscribable.fn.subscribeChanged = function (callback) {
    var oldValue;
    this.subscribe(function (_oldValue) {
        oldValue = _oldValue;
    }, this, 'beforeChange');

    this.subscribe(function (newValue) {
        callback(oldValue, newValue);
    });
}; 

ko.bindingHandlers.hover = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        element.addEventListener("mouseover", () => {
            var value = valueAccessor();
            value(true);
        });

        element.addEventListener("mouseout", () => {
            var value = valueAccessor();
            value(false);
        });
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // This will be called once when the binding is first applied to an element,
        // and again whenever any observables/computeds that are accessed change
        // Update the DOM element based on the supplied values here.
    }
};