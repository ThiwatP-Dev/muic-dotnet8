var section = '.js-cascade-section';
var sectionSeatLimit = '.js-cascade-section-seat-limit';
var jointSeatLimit = '.js-cascade-joint-seat-limit';
var mainInstructor = '.js-main-instructor';
var sectionExtraSeat = '.js-section-extra-seat';
var periodInstructor = '.js-period-instructor';
var jointExtraSeat = '.js-joint-extra-seat';
var teachingType = '.js-cascade-teaching-type';
var disabledFinal = ".js-final-examination"
var disabledMidterm = ".js-midterm-examination"
var jointSeatLimitValue = 0;
// var jointExtraSeatValue = 0;
var periodInstructorValue = 0;
var course = '.js-offer-course';
var term = '.js-offer-term';
var sectionNumber = '.js-offer-number';

$(document).ready(function() {
    $('#close-confirm-modal, #open-confirm-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget); // Button that triggered the modal
        
        let controller = button.data('controller');
        let action = button.data('action');
        let value = button.data('value');
    
        let fullRoute = `/${ controller }/${ action }/${ value }`;
    
        let confirmButton = $('.js-confirm-btn');
        confirmButton.attr("href", `${ fullRoute }`)
    });

    $('#course-offered-details-modal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let courseOfferedId = button.data('value')
    
        var ajax = new AJAX_Helper(
            {
                url: CourseOfferedDetailsUrl,
                data: {
                    id: courseOfferedId,
                },
                dataType: 'html'
            }
        );
    
        ajax.GET().done(function (response) { 
            $('#modalWrapper-course-offered-details').empty().append(response);
            RenderTableStyle.columnAlign();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#course-offered-main-instructor').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let courseOfferedId = button.data('value')
        var returnUrl = button.data('return-url');
    
        var ajax = new AJAX_Helper(
            {
                url: CourseOfferedChangeInstructorUrl,
                data: {
                    id: courseOfferedId,
                    returnUrl: returnUrl
                },
                dataType: 'html'
            }
        );
    
        ajax.GET().done(function (response) { 
            $('#modalWrapper-course-offered-main-instructor').empty().append(response);
            RenderTableStyle.columnAlign();
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#course-offered-joint-section').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget);
        let parentSectionId = button.data('value')
        var returnUrl = button.data('return-url');
    
        var ajax = new AJAX_Helper(
            {
                url: CourseOfferedAddJointSection,
                data: {
                    id: parentSectionId,
                    returnUrl: returnUrl
                },
                dataType: 'html'
            }
        );

        ajax.GET().done(function (response) { 
            $('#content-course-offered-joint-section').empty().append(response);
            $('.chosen-select').chosen();
            $(document).on('keyup focusout', '#joint-section-seat-limit',function () {
                let maxSeatLimit = $(this).data("max");
                let seatLimit = $(this).val();
                if(!seatLimit.toString().match(/^[-]?\d*\.?\d*$/) 
                    || 0 > parseFloat(seatLimit) 
                    || maxSeatLimit < parseFloat(seatLimit) 
                    || seatLimit == "")
                {
                    $('#js-seat-limit-text').html('seat limit invalid');
                    if(seatLimit == "")
                    {
                        SaveButton();
                    } else {
                        $(this).val("");
                        SaveButton();
                    }
                }
                else
                {
                    $('#js-seat-limit-text').html("");
                    SaveButton();
                }
            })
            $(document).on('change', '#joint-section-course-id',function () {
                SaveButton()
            })
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    var inputTable = new RowAddAble({
        TargetTable: '#js-table-period',
        TableTitle: 'Section Peroid',
        ButtonTitle: 'Period'
    })
    inputTable.RenderButton();
    InputMask.renderTimeMask();

    var jointTable = new RowAddAble({
        TargetTable: "#js-joint-section",
        TableTitle: 'Joint Section',
        ButtonTitle: 'Joint Section'
    });
    jointTable.RenderButton();
    getLectureTeachingTypeId();

    var isStatusApproval = $(".js-section-status").val();

    if(isStatusApproval == "True")
    {
        $(".js-group-joint-section").find(".js-add-row").addClass("d-none");
    }
});

$(document).on('click', '.js-add-row', function() {
    jointSeatLimitValue = $(sectionSeatLimit).val();
    // jointExtraSeatValue = $(sectionExtraSeat).val();
    periodInstructorValue = $(mainInstructor).val();
    let tablePeriod = $(`#js-table-period tbody tr:last`);
    let jointSection = $(`#js-joint-section tbody tr:last`);
    
    let currentSeatLimit = $(jointSection).closest('tr').find(jointSeatLimit);
    // let currentExtraSeat = $(jointSection).closest('tr').find(jointExtraSeat);
    if (periodInstructorValue > 0) {
        let currentPeriodInstructor = $(tablePeriod).closest('tr').find(periodInstructor);
        $(currentPeriodInstructor).val(periodInstructorValue).trigger('chosen:updated');
    }
    
    $(currentSeatLimit).val(jointSeatLimitValue);
    // $(currentExtraSeat).val(jointExtraSeatValue);
    let currentExtraSeat = $(jointSection).closest('tr').find(jointExtraSeat);
    $(currentExtraSeat).val(0);

    let startTime = $(tablePeriod).closest('tr').find('.js-offer-start');
    $(startTime).val('08:00');

    let endTime = $(tablePeriod).closest('tr').find('.js-offer-end');
    $(endTime).val('09:50');

    let day = $(tablePeriod).closest('tr').find('.js-period-day');
    $(day).val('-1').trigger('chosen:updated');

    getLectureTeachingTypeId();
    minMaxLimit();
});

function SaveButton()
{
    let seatlimit = $('#joint-section-seat-limit').val();
    let courseId = $('#joint-section-course-id').val();
    if(seatlimit == null || seatlimit == "" || courseId == null || courseId == 0)
    {
        $('#add-joint-section-save').prop("disabled", true);
    } else {
        $('#add-joint-section-save').prop("disabled", false);
    }
}

function cascadeMaxSeatLimitBySectionSeatLimit(seatLimit) {
    var ajax = new AJAX_Helper(
        {
            url: MaxSeatLimitBySectionSeatLimitUrl,
            data: {
                seatLimit: seatLimit
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(jointSeatLimit).attr("max", response);
        minMaxLimit();
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getLectureTeachingTypeId() {
    var ajax = new AJAX_Helper(
        {
            url: GetLectureTeachingTypeId,
            data: {
                teachingType: "lecture"
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        let tablePeriod = $(`#js-table-period tbody tr:last`);
        let currentTeachingType = $(tablePeriod).closest('tr').find(teachingType);
        $(currentTeachingType).val(response).trigger('chosen:updated');
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(document).on('change', sectionSeatLimit, function() {
    var seatLimit = $(this).val()
    cascadeMaxSeatLimitBySectionSeatLimit(seatLimit);
});

$(document).on('keyup', sectionSeatLimit, function() {
    var seatLimit = $(this).val()
    $(jointSeatLimit).val(seatLimit);
    jointSeatLimitValue = seatLimit;
});

$(document).on('keyup', sectionExtraSeat, function() {
    var extraSeat = $(this).val()
    $(jointExtraSeat).val(extraSeat);
    // jointExtraSeatValue = extraSeat;
});

$(document).on('change', mainInstructor, function() {
    let mainInstructorId = $(this).val();
    let currentSection = $(this).closest('section');
    let currentPeriodInstructor = currentSection.find(periodInstructor);
    $(currentPeriodInstructor).val(mainInstructorId).trigger('chosen:updated');
    periodInstructorValue = mainInstructorId;
});

$(document).on('change', course, function() {
    var ajax = new AJAX_Helper(
        {
            url: GetNextSectionNumberUrl,
            data: {
                courseId: $(this).val(),
                termId: $(term).val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(sectionNumber).val(response);
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
});

function changeAddButton() {
    var academicLevelId = $('.courseoffer-level').val();
    var termId = $('.courseoffer-term').val();
    var contextPath = $('.courseoffer-context').val();
    var currentPath = $('.courseoffer-add-btn').attr('href');
    if (termId > 0 && academicLevelId > 0) {
        var url = `${currentPath.split(/[?#]/)[0]}?academicLevelId=${academicLevelId}&termId=${termId}&returnUrl=${contextPath}`;
        $('.courseoffer-add-btn').attr('href', url);
    }
}

$(document).on('change', '.courseoffer-term', function () {
    changeAddButton();
});
$(document).on('change', '.courseoffer-level', function () {
    changeAddButton();
});

$(document).on('click', '#IsDisabledMidterm', function() {
    if($(this).prop('checked'))
    {
        $(disabledMidterm).addClass("d-none");
    } else {
        $(disabledMidterm).removeClass("d-none");
    }
});
$(document).on('click', '#IsDisabledFinal', function() {
    if($(this).prop('checked'))
    {
        $(disabledFinal).addClass("d-none");
    } else {
        $(disabledFinal).removeClass("d-none");
    }
});