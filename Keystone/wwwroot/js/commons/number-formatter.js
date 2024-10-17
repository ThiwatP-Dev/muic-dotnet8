var NumberFormat = (function() {
    var DecimalOneDigits = function(number) {
        return new Intl.NumberFormat(undefined, {
            style: 'decimal',
            minimumFractionDigits: 1
        }).format(number);
    }

    var DecimalTwoDigits = function(number) {
        return new Intl.NumberFormat(undefined, {
            style: 'decimal',
            minimumFractionDigits: 2
        }).format(number);
    }

    var BahtCurrency = function(number) {
        return new Intl.NumberFormat('th-TH', {
            style: 'currency',
            currency: 'THB'
        }).format(number);
    }

    var UsDollasCurrency = function(number) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(number);
    }

    return {
        renderDecimalOneDigits : DecimalOneDigits,
        renderDecimalTwoDigits : DecimalTwoDigits,
        renderBahtCurrency : BahtCurrency,
        renderUsDollasCurrency : UsDollasCurrency
    }

})();