var feeItemSelect = '#js-waive-item';
var feeCourseSelect = '#js-cascade-fee-course';
var waiveAmountInput = '#js-cascade-fee-amount';

var receipt = '#js-receipt-details';
var feeItemsClass = '.js-fee-id';
var feeAmountClass = '.js-fee-amount';
var feePersonalAmountClass = '.js-personal-fee-amount';
var feeScholarshipAmountClass = '.js-scholarship-fee-amount';

var mainItem = '.js-main-item';
var subItem = '.js-sub-item';
var waiveItem = '.js-waive-item';
var totalRow = '.js-total-row';

var feeCourseDetail = '.js-fee-detail';
var courseIdInput = '.js-fee-course-id';
var courseNameInput = '.js-fee-course-name';

function getFeeItem(feeId) {
    let feeItems = $(receipt).find(feeItemsClass);
    let feeItem;

    feeItems.each(function() {
        if (feeId === $(this).val()) {
            feeItem = $(this);
        }
    })

    return feeItem;
}

function getCourseByFeeItem(feeId, subFee) {

    let receiptItems = [];
    subFee.find('.js-fee-course').each( function() {
        let feeCourseId = $(this).find(courseIdInput).val();
        let feeCourseName = $(this).find(courseNameInput).val();

        receiptItems.push(
            {
                FeeItemId : feeId, 
                CourseId : feeCourseId,
                CourseName : feeCourseName
            }
        )
    })

    var ajax = new AJAX_Helper(
        {
            url: ReceiptGetCourseByFeeItemUrl,
            data: {
                feeItemId: feeId,
                receiptItems: receiptItems
            },
            dataType: 'html'
        }
    );

    ajax.POST().done(function (response) {
        if (response != null && response.length > 0) {
            $(feeCourseSelect).prop('disabled', false);

            $(feeCourseSelect).append(getDefaultSelectOption($(feeCourseSelect)));

            response.forEach((item) => {
                $(feeCourseSelect).append(getSelectOptions(item.courseId, item.courseName));
            });

            $(feeCourseSelect).trigger('chosen:updated');
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function getFeeAmount(feeItem) {
    let currentAmount = $(feeItem).parents('.content-detail').find(feeAmountClass).val();

    $(waiveAmountInput).val(currentAmount);
    $(waiveAmountInput).attr('max', currentAmount);
}

function addWaiveFee(feeItem) {
    var cloneItem;
    let currentFee = $(feeItem);
    let currentFeeCourse = $(feeCourseSelect);
    if (!currentFeeCourse.prop('disabled')) {
        let currentCourseId = currentFeeCourse.val();
        let feeCourseIds = $(currentFee).parents(mainItem).next().find(courseIdInput);

        feeCourseIds.each(function() {
            if (currentCourseId === $(this).val()) {
                cloneItem = $(this).parents(feeCourseDetail).clone(false);
            }
        })
        var itemDetails = cloneItem.children();
        $(itemDetails[0]).prepend(`<span>(${ currentFee.prev().text() })</span>`);
    } else {
        cloneItem = currentFee.parents(mainItem).clone(false);
        var itemDetails = cloneItem.children();
        $(itemDetails[0]).removeClass('p-l-22').addClass('p-l-33');
    }

    let waiveFee = cloneItem.find(feeAmountClass);
    let waiveValue = $(waiveAmountInput).val();
    waiveValue = parseFloat(`-${ waiveValue }`)
    waiveFee.val(waiveValue);

    cloneItem.find('div').each( function(index) {
        if (index === 1) {
            $(this).empty()
                   .append(waiveFee)
                   .append(NumberFormat.renderDecimalTwoDigits(waiveValue))
                   .removeClass('col-3')
                   .addClass('col-3 text-right text-danger pr-0');
        } else if (index === 2 || index === 3) {
            $(this).empty();
        }
    })
    cloneItem.removeClass('js-main-item');

    var feeItemCount = $(mainItem).length;
    
    if (!$(totalRow).prev().hasClass('js-waive-item')) {
        $(receipt).find(totalRow).before(`
            <div class="row content-detail toggle-navigator">
                <div class="col-3 pl-0">
                    <span class="hidden-row-toggle disable-item"><i class="fas fa-minus-circle"></i></span>
                    Waive
                    <input class="js-fee-id" name="ReceiptDetailViewModels[${ feeItemCount }].ItemId" hidden/>
                </div> 
                <div class="col-3 text-right text-danger pr-0">
                    <span id="js-total-waive"></span>
                    <input id="js-waive-amount" name="ReceiptDetailViewModels[${ feeItemCount }].Total" hidden/>
                </div>
                <div class="col-2 pr-0"></div>
            </div>
            <div class="hidden-row js-waive-item"></div>
        `)

        $(waiveItem).append(cloneItem);
        let toggleButton = $(waiveItem).prev().find('.hidden-row-toggle');
        RenderToggle.toggleHiddenButton(toggleButton);
    } else {
        $(waiveItem).append(cloneItem);
        let toggleButton = $(waiveItem).prev().find('.hidden-row-toggle');
        
        let parentRow = $(toggleButton.closest('.toggle-navigator'))
        let hiddenRow = parentRow.next('div.hidden-row');
        let toggleButtonIcon = $(toggleButton).find('i');

        if (hiddenRow.hasClass('d-none')) {
            toggleButtonIcon.toggleClass('fa-minus-circle');
            toggleButtonIcon.toggleClass('fa-plus-circle');
            hiddenRow.toggleClass('d-none');
        }
    }

    let alreadyWaived = $(waiveItem).children('div');
    alreadyWaived.each( function(index) {
        let waiveInput = $(this).find('input');

        waiveInput.each( function() {
            let inputName = $(this).attr('name');
            let splitName;

            if ($(this).data('alter-name')) { // term fee input (which won't use)
                splitName = $(this).data('alter-name').split(RegularExpressions.AllDigitInBracket);
            } else {
                splitName = inputName.split(RegularExpressions.AllDigitInBracket);
            }
            let newName = `${ splitName[0] }[${ feeItemCount }]${ splitName[2] }[${ index }]${ splitName[4] }`;
            $(this).attr('name', newName);
        })
    })

    calculateReceipt();
}

function calculateReceipt() {
    let currentAmount = 0.00;
    let currentWaiveAmount = 0.00;
    let currentFee = $(receipt).find(mainItem);
    let waiveFees = $(waiveItem).find(feeAmountClass);

    currentFee.each( function() {
        currentAmount += parseFloat($(this).find(feeAmountClass).val());
    })

    waiveFees.each( function() {
        currentWaiveAmount += parseFloat($(this).val());
    })

    $('#js-total-waive').html(NumberFormat.renderDecimalTwoDigits(currentWaiveAmount));
    $('#js-waive-amount').val(currentWaiveAmount);

    currentAmount += currentWaiveAmount;
    $('#js-receipt-total').html(NumberFormat.renderDecimalTwoDigits(currentAmount));
}

$( function() {
    $("#receipt-preview-modal").on('shown.bs.modal', function () {
        var courses = []
        let addingRow = $('#js-adding').find('tbody tr');
        let studentId = $(`[name="StudentId"]`).val();
        let registrationTermId = $(`[name="RegistrationTermId"]`).val();
        let registrationRound = $(`[name="RegistrationRound"]`).val();
        let isRegistered = $(`[name="IsRegistered"]`).val();

        addingRow.each(function (index) {
            let courseId = $(`[name="AddingResults[${index}].CourseId"]`).val();
            let isPaid = $(`[name="AddingResults[${index}].IsPaid"]`).val();
            let refundValuePercentage = $(`[name="AddingResults[${index}].Refund"]`).val();
            let registrationCourseId = $(`[name="AddingResults[${index}].RegistrationCourseId"]`).val();
            let sectionId = $(`[name="AddingResults[${index}].SectionId"]`).val();

            var addingCourse = {
                RegistrationCourseId: registrationCourseId,
                CourseId: parseInt(courseId),
                RefundPercentage: refundValuePercentage || -1,
                IsPaid: isPaid,
                SectionId: sectionId || 0
            }
            courses[index] = addingCourse;
        })

        let RegisteringCourse = {
            StudentId: studentId,
            RegistrationTermId: registrationTermId,
            RegistrationRound: registrationRound,
            RegisteringCourses: courses,
            IsRegistered: isRegistered
        }

        var ajax = new AJAX_Helper(
            {
                url: RenderReceiptModal,
                data: JSON.stringify(RegisteringCourse),
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
            }
        );

        ajax.POST().done(function (response) {
            $('#modalWrapper-receipt-log').empty().append(response);

            RenderToggle.toggleHiddenButton('.hidden-row-toggle');
            $('.chosen-select').chosen();
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $("#receipt-preview-modal").modal('hide');
            Alert.renderAlert('Warning', 'This Student without fee group or term fee.', 'warning');
        });

        $(this).on('click', '#js-add-waive', function() {
            let selectFeeId = $(feeItemSelect).val();
            let selectFeeCourseId = $(feeCourseSelect).val();
            let currentFee = getFeeItem(selectFeeId);

            let isWaiveTermFee = false;
            let isWaiveCourseFee = false;
            let alreadyWaivedTermFee = $(waiveItem).find(feeItemsClass);

            alreadyWaivedTermFee.each( function() {
                if ($(this).val() === selectFeeId) {
                    isWaiveTermFee = true;
                }
            })

            if (selectFeeCourseId === null) {
                if (!isWaiveTermFee) {
                    if ($(feeCourseSelect).prop('disabled')) {
                        addWaiveFee(currentFee);
                    } else {
                        Alert.renderAlert('Error', 'Please select course.', 'error');
                    }
                } else {
                    Alert.renderAlert('Error', 'This item is already waive.', 'error');
                }
                
            } else {
                let alreadyWaivedCourseFee = $(waiveItem).find(courseIdInput);

                alreadyWaivedCourseFee.each( function() {
                    if ($(this).val() === selectFeeCourseId) {
                        isWaiveCourseFee = true;
                    }
                })

                if (!isWaiveCourseFee) {
                    addWaiveFee(currentFee);
                } else {
                    Alert.renderAlert('Error', 'This item is already waive', 'error');
                }
            }
        })

        $(this).on('change', feeItemSelect, function() {
            $(waiveAmountInput).val(0.00);

            let feeItemId = $(this).val();
            let feeItem = getFeeItem(feeItemId);

            let mainFee = feeItem.parents(mainItem);
            let subFee = mainFee.next();

            if (subFee.hasClass('js-sub-item')) {
                getCourseByFeeItem(feeItemId, subFee);
            } else {
                $(feeCourseSelect).append(getDefaultSelectOption($(feeCourseSelect)));
                $(feeCourseSelect).prop('disabled', true).trigger('chosen:updated');
                getFeeAmount(feeItem);
            }            
        })

        $(this).on('change', feeCourseSelect, function() {
            let currentCourseId = $(this).val();
            let currentFee = getFeeItem($(feeItemSelect).val());
            let feeCourseIds = $(currentFee).parents(mainItem).next().find(courseIdInput);

            feeCourseIds.each(function() {
                if (currentCourseId === $(this).val()) {
                    getFeeAmount($(this));
                }
            })
        })

        $(this).on('click', '#js-waive-trigger', function() {
            $('.js-waive-input').toggleClass('d-none');
        })
    })
})