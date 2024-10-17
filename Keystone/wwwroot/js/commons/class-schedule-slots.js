// default constant value
const BG_COLOR_PRIMARY = "bg-color-primary";
const CLASS_TIME_SLOTS = ['08:00', '10:00', '12:00', '14:00', '16:00', '18:00', '20:00'];
const SLOT_INTERVAL = 120 // minutes

var ClassScheduleSlot = (function(config) {
    var dataSets = config.dataSets;
    
    function renderClassScheduleSlots() {
        try {
            clearClassSchedule();
            $.each(dataSets, function(index, schedule) {
                var copySchedule = JSON.parse(JSON.stringify(schedule));

                copySchedule.ScheduleTimes.forEach(function(classTime, index) {
                    classTime.StartTime = changeTimeFormat(classTime.StartTime);
                    classTime.EndTime = changeTimeFormat(classTime.EndTime);
    
                    var classTimeSlotView = getClassTimeSlotView(classTime, copySchedule.ColorCode, copySchedule.CourseCode, copySchedule.Section);
                    var indexTimeSlot = getTimeSlotStartIndex(classTime.StartTime) + 1; // +1 for skip day name column Started Column in each row
                    $(config.table).find('tr').eq(classTime.Day + 1).find('td').eq(indexTimeSlot).find('.wrapper').append(classTimeSlotView);
                });
            });
        }

        catch(err) {
            console.log("Fail to render table", err);
        }
    }

    function clearClassSchedule() {
        $(config.table).find('.js-class-time-slot').remove();
    }

    return {
        renderClassScheduleSlots: renderClassScheduleSlots,
        clearClassSchedule: clearClassSchedule
    };
});

function getClassTimeSlotView(classTime, color, courseCode, section) {
    // create time slot view [course / section]
    var classTimeSlotView = createEmptyTimeSlot();

    // var leftOffset = getLeftOffset(classTime.StartTime); // length from start time slot e.g. class start: 09:20, slot start time: 09:00
    // var widthTimeSlotPercentage = getWidthTimeSlotPercentage(classTime.StartTime, classTime.EndTime);
    // var columnBorderWidth = getColumnBorderWidth(widthTimeSlotPercentage);

    // if (isUndefinedValue(color)) {
    //     classTimeSlotView.addClass(BG_COLOR_PRIMARY);
    // }

    var left = calculateOffset(classTime.StartTime);
    var width = calculateTimeSlotWidth(classTime.StartTime, classTime.EndTime);
    classTimeSlotView.css({
        'position': 'absolute',
        'left': `${ left }%`,
        'width': `${ width }%`,
        'background-color' : color
    });
    
    classTimeSlotView.attr('dayNumber', classTime.Day);
    classTimeSlotView.attr('color', color);
    classTimeSlotView.html(`${ courseCode }`);

    if (!isUndefinedValue(section) && section != "") {
        classTimeSlotView.html(`${ courseCode }  (${ section }) <div class="u-font-size-13">(${ classTime.Room })</div>`);
    } else {
        classTimeSlotView.html(`${ courseCode } <div class="u-font-size-13">(${ classTime.Room })</div>`);
    }
    
    return classTimeSlotView;
}
function calculateTimeSlotWidth(startedTime, endedTime) {
    // convert start time to minutes
    var startedMinutes = convertToMinute(startedTime);
    // convert ended time to minutes
    var endedMinutes = convertToMinute(endedTime);

    var totalMinutes = endedMinutes - startedMinutes;
    // 60 Mins = 100%
    // 1 Min = 100/60 ~ 1.67%
    var width = (totalMinutes * 100) / SLOT_INTERVAL;
    return width;
}

function convertToMinute(timeString) { // 08:30
    var minutes = parseInt(timeString.split(':')[1]);
    if (minutes == 50) {
        minutes += 10;
    }

    return parseInt(timeString.split(':')[0] * 60) + minutes;
}

function getTimeSlotStartIndex(startedTime) {
    var convertedTime = convertToMinute(startedTime) + 1; // for skip slot eg. 10:00 should go to slot 3(10:00 - 11:00) but for logic below will go to slot 2(09:00 - 10:00)
    for (i = 0; i < CLASS_TIME_SLOTS.length - 1; i++) {
        if (convertedTime >= convertToMinute(CLASS_TIME_SLOTS[i]) && convertedTime <= convertToMinute(CLASS_TIME_SLOTS[i + 1])) {
            return i;
        }
    }
    return 0;
}

function calculateOffset(startedTime) {
    var slotIndex = getTimeSlotStartIndex(startedTime);
    var slotTime = CLASS_TIME_SLOTS[slotIndex];
    if (slotTime == startedTime) {
        return 0;
    }

    var offset = calculateTimeSlotWidth(slotTime, startedTime);
    return offset;
}

function createEmptyTimeSlot() {
    return $($.parseHTML('<span class="js-class-time-slot class-time-slot"><span></span></span>'));
}