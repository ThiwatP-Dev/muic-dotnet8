var totalCredit = 0;
var totalRegistCredit = 0;
var oldCredit = $('#js-summary-credit').html();

function calculateCredit(oldCredit) {

    totalCredit = 0;
    $('.js-credit').each( function() {
        
        let isDisabled = $(this).hasClass('disable-item');
        if ($(this).html()) {
            totalCredit += parseInt($(this).html());

            if (isDisabled) {
                totalCredit -= parseInt($(this).html());
            }
        }
    });

    $('.js-total-credit').html(totalCredit);

    totalRegistCredit = 0;
    $('.js-regist-credit').each( function() {

        let isDisabled = $(this).hasClass('disable-item');
        if ($(this).html()) {
            totalRegistCredit += parseInt($(this).html());

            if (isDisabled) {
                totalRegistCredit -= parseInt($(this).html());
            }
        } 
    });
    $('.js-total-regist-credit').html(totalRegistCredit);
    $('#js-summary-credit').html(`${ oldCredit } (+${ totalRegistCredit })`);
}

$(document).on('click','.js-delete-row-btn', function() {
    let referenceData = $(this).data('id').split('|');
    let tableId = referenceData[0];
    let rowIndex = parseInt(referenceData[1]) - 1;
    let allRow = $(tableId).find('tr');
    let currentRow = allRow[rowIndex];
    
    if (referenceData.length > 2) {
        $(currentRow).find('.js-credit').addClass('disable-item');
        $(currentRow).find('.js-regist-credit').addClass('disable-item');
        //$(currentRow).find('.js-toggle-disabled').prop('disabled', false).trigger("chosen:updated");
    } else if (rowIndex == 0 && allRow.length <= 3) {
        currentRow = allRow[1];
        let allInputs = $(currentRow).find('input')

        allInputs.each(function() {
            $(this).attr('course-id', 0);
            $(this).attr('section-id', 0);
            $(currentRow).find('.js-main-instructor').html('');
            $(currentRow).find('.js-credit').html(0);
            $(currentRow).find('.js-regist-credit').html(0);
        })
    }

    calculateCredit(oldCredit);
})

$(document).on('click','.js-refresh-row', function() {
    let referenceData = $(this).data('id').split('|');
    let tableId = referenceData[0];
    let rowIndex = referenceData[1].slice(-1);
    let allRow = $(tableId).find('tr')

    $(allRow[rowIndex]).find('.js-credit').removeClass('disable-item');
    $(allRow[rowIndex]).find('.js-regist-credit').removeClass('disable-item');
    //$(allRow[rowIndex]).find('.js-toggle-disabled').prop('disabled', true).trigger("chosen:updated");
    calculateCredit(oldCredit);
})

function cascadeSectionByRegistrationCourse(courseId, termId, target) {
    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourseId,
            data: {
                termId: termId,
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        $(target).empty();
        sectionId = response[0].value;
        if(sectionId != 0)
        {
            cascadeMainInstructorBySectionId(sectionId,$(target).closest('tr').find('.js-main-instructor'));
        }
        response.forEach((item) => {
            $(target).append(getSelectOptions(item.value, item.text));
        });

        $(target).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeMainInstructorBySectionId(sectionId, target)
{
    var ajax = new AJAX_Helper(
        {
            url: GetMainInstructorAndCheckSeatAvailableBySectionId,
            data: {
                sectionId: sectionId,
            },
            dataType: 'json'
        }
    );
    var sectionId = 0;
    ajax.POST().done(function (response) { 
        var responseConvert= JSON.parse(response);
        if(responseConvert.MainInstructorFullNameEn != "" && responseConvert.MainInstructorFullNameEn != null) {
            $(target).html("");
            $(target).html(responseConvert.MainInstructorFullNameEn);
        } else {
            $(target).html("");
        }

        if(responseConvert.Message != "" && responseConvert.Message != null) {
            Alert.renderAlert("Warning", `${ responseConvert.Message} : Seat not available`, "warning");
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function cascadeCreditAndRegistrationCredit(courseId, creditTarget, registrationCreditTarget) {
    var ajax = new AJAX_Helper(
        {
            url: GetCreditAndRegistrationCreditByCourseId,
            data: {
                courseId: courseId
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $(creditTarget).html(response.creditText);
        $(registrationCreditTarget).html(response.registrationCredit);
        calculateCredit(oldCredit);
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}
$(document).on('change', '.js-cascade-registration-section', function() {
    var mainInstructor = $(this).closest('tr').find('.js-main-instructor');
    var sectionId = $(this).val();
    cascadeMainInstructorBySectionId(sectionId, mainInstructor);
});

$(document).on('change', '.js-cascade-registration-course', function() {
    var termId = $(term).val();
    var courseId = $(this).val();
    var closestSection = $(this).closest('tr').find('.js-cascade-registration-section');
    var closestCredit = $(this).closest('tr').find('.js-credit');
    var closestRegistrationCredit = $(this).closest('tr').find('.js-regist-credit');
    var instructor = $(this).closest('tr').find('.js-main-instructor');

    if (closestSection.length > 0) {
        var ajax = new AJAX_Helper(
            {
                url: SelectListURLDict.GetSectionByCourseId,
                data: {
                    termId: termId,
                    courseId: courseId
                },
                dataType: 'json'
            }
        );
    
        ajax.POST().done(function (response) { 
            $(closestSection).empty();
            sectionId = response[0].value;
            if(sectionId != 0)
            {
                cascadeMainInstructorBySectionId(sectionId, instructor);
            }
            response.forEach((item) => {
                $(closestSection).append(getSelectOptions(item.value, item.text));
            });
    
            $(closestSection).trigger("chosen:updated");
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    }

    cascadeCreditAndRegistrationCredit(courseId, closestCredit, closestRegistrationCredit);
});

$(document).on('click', '.js-add-row', function() {
    var credit = $("#js-course-tbody tr:last").closest('tr').find('.js-credit');
    var regisCredit = $("#js-course-tbody tr:last").closest('tr').find('.js-regist-credit');
    var mainInstructor = $("#js-course-tbody tr:last").closest('tr').find('.js-main-instructor');
    $(credit).html("0 (0-0-0)");
    $(regisCredit).html("0");
    $(mainInstructor).html("");
});

$( function() {

    if ($('.js-student-status').html() == "Deleted") {
        Alert.renderAlert("Deleted Student", "This student have been deleted.", "warning");
    }

    $('.js-get-reg-credit').each( function() {
        totalCredit += parseInt($(this).html())
    })
    $('#js-total-get-reg-credit').html(totalCredit)

    calculateCredit(oldCredit);

    $('#cancel-receipt-modal').on('shown.bs.modal', function(event) {
        $('.js-confirm-cancel').on('change', function() {
            if (this.checked) {
                $('.js-cancel-receipt').removeClass('disabled')
            } else {
                $('.js-cancel-receipt').addClass('disabled')
            }
        })

        let cancelButton = $(event.relatedTarget); // Button that triggered the modal
        let receiptId = cancelButton.data('value');
        let fullRoute = `/Receipt/Cancel/${ receiptId }`;
        let cancelConfirmBtn = $('.js-cancel-receipt');
        cancelConfirmBtn.attr("href", `${ fullRoute }`)

        $('.js-cancel-receipt').on('click', function() {
            $('#cancel-receipt-modal').modal('hide').data('bs.modal', null);
        })
    })

    $('#js-receipt-details-modal').on('shown.bs.modal', function(event) {
        var button = event.relatedTarget;
        var buttonId = $(button).data('receipt-id');
        var eventType = $(button).data('type')

        if (eventType === "receipt-details") {

            var ajax = new AJAX_Helper(
                {
                    url: ReceiptDetailsUrl,
                    data: {
                        id: buttonId,
                    },
                    dataType: 'html',
                    contentType : "application/json; charset=utf-8"
                }
            );
        
            ajax.GET().done(function (response) { 
                $('#js-receipt-details-modal .modal-content').html(response);
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        } else if (eventType === "transaction") {

            var ajax = new AJAX_Helper(
                {
                    url: ReceiptTransectionUrl,
                    data: {
                        id: buttonId,
                    },
                    dataType: 'html',
                    contentType : "application/json; charset=utf-8"
                }
            );
        
            ajax.GET().done(function (response) { 
                $('#js-receipt-details-modal .modal-content').html(response);
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        } else {
            return;
        }
    })

    $('#return-seat-modal').on('show.bs.modal', function(event) {
        let button = $(event.relatedTarget); // Button that triggered the modal
        
        let controller = button.data('controller');
        let action = button.data('action');
        let channel = button.data('route-channel');
        let studentId = button.data('route-student-id');
        let termId = button.data('route-term-id');
    
        let fullRoute = `/${ controller }/${ action }/?channel=${ channel }&studentId=${ studentId }&termId=${ termId }&`;
        $('#js-return-seat').attr("href", `${ fullRoute }`)
    });

    $('#js-invoice-details-modal').on('shown.bs.modal', function (e) {
        let invoiceId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: InvoiceDetailUrl,
                data: {
                    id: invoiceId,
                },
                dataType: 'html',
                contentType: "application/json; charset=utf-8"
            }
        );

        ajax.GET().done(function (response) {
            $('#js-invoice-details-modal .modal-content').html(response);
        })
            .fail(function (jqXHR, textStatus, errorThrown) {
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
    });

    $('#js-confirm-invoice-details-modal').on('shown.bs.modal', function (e) {
        let invoiceId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: InvoicePartModalDetailUrl,
                data: {
                    id: invoiceId,
                },
                dataType: 'html',
                contentType: "application/json; charset=utf-8"
            }
        );

        ajax.GET().done(function (response) {
            $('#js-confirm-invoice-details-modal .modal-content').html(response);
        })
            .fail(function (jqXHR, textStatus, errorThrown) {
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
    })

})