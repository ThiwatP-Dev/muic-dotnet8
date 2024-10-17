/*------------------------------------
  Render Class Schedule
------------------------------------*/

$(document).ready(function() {
    var schedule = new ClassScheduleSlot({
        table: $('.js-class-schedule-table'),
        dataSets: $('#js-schedule').data('schedules')
    });
    
    schedule.renderClassScheduleSlots();
});


/*------------------------------------
  Filter Class Schedule
------------------------------------*/

function filterClassSchedule(filterDayNumber) {
    var hasClass = false
    $('.js-message-box', '#js-class-timeline').remove()

    $('li', ".js-filter-day-navtabs").each(function (index) {
        $(this).removeClass('active')
        if (index == filterDayNumber) {
            $(this).addClass('active')
        }
    })

    $('.js-class-timeline').each(function (index) {
        const dayNumber = $(this).attr('dayNumber')
        $(this).removeClass('d-none')
        if (filterDayNumber != dayNumber && filterDayNumber != 0 && dayNumber != 0) {
            $(this).addClass('d-none')
        } else {
            hasClass = true
        }
    })

    $('.js-class-time-slot', 'table').each(function () {
        const dayNumber = $(this).attr('dayNumber')
        const color = $(this).attr('color')
        $(this).css({
            'background-color': color
        })

        if (filterDayNumber != dayNumber && filterDayNumber != 0) {
            $(this).css({
                'background-color': ""
            })
        }
    })
}