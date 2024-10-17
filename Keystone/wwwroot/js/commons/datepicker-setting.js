const singleDateFormat = {
    timePicker: false,
    singleDatePicker: true,
    autoUpdateInput: false,
    locale: {
        format: 'DD/MM/YYYY'
    }
}

const monthYearFormat = {
    timePicker: false,
    singleDatePicker: true,
    autoUpdateInput: false,
    locale: {
        format: 'DD/MM/YYYY'
    }
}

const yearFormat = {
    autoclose: true,
    format: "yyyy",
    viewMode: "years",
    minViewMode: "years"
}

const dateTimeFormat = {
    timePicker: true,
    timePickerIncrement: 15,
    singleDatePicker: true,
    locale: {
        format: 'DD/MM/YYYY HH:mm'
    }
}

var DateTimeInput = (function() {

    var SingleDatePicker = function(input) {
        input.daterangepicker(singleDateFormat);

        input.on('apply.daterangepicker', function(ev, picker) {
            $(this).val(picker.startDate.format('DD/MM/YYYY'));
        });
    }

    var MonthYearPicker = function(input) {
        input.datepicker(monthYearFormat);
    }

    var YearPicker = function(input) {
        input.datepicker(yearFormat);
    }

    var DateTimePicker = function(input) {
        input.daterangepicker(dateTimeFormat);
    }

    return {
        renderSingleDate : SingleDatePicker,
        renderMonthYear : MonthYearPicker,
        renderYear : YearPicker,
        renderDateTime : DateTimePicker
    }

})();

$(function() {
    DateTimeInput.renderSingleDate($('.js-single-date'));
    DateTimeInput.renderMonthYear($('.js-month-year'));
    DateTimeInput.renderYear($('.js-single-year'));
    DateTimeInput.renderDateTime($('.js-date-time'));
});