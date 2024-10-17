let studentId = '#js-student-id';
let scholarshipModal = '.js-scholarship-modal';

let scholarshipType = '.js-cascade-scholarship-type';
let scholarship = '.js-cascade-scholarship';
let scholarshipExpiryTerm = '.js-expiry-term';
let scholarshipEffectiveTerm = '.js-effective-term';
let academicLevelId = '#js-academic-level';
let limitAmount = '.js-scholarship-limit-amount';

let approveDetails = '.js-approve-details'

$(document).ready(function() {
    $(scholarshipModal).on('shown.bs.modal', function() {
        $('.chosen-select').chosen();

        InputConfig.setDefaultZero();

        $(this).on('change', '.js-approve', function() {
            if ($(this).prop('checked')) {
                $(approveDetails).removeClass('disable-item');
                $(approveDetails).find('input,select').prop('disabled', false).trigger('chosen:updated');
            } else {
                $(approveDetails).addClass('disable-item');
                $(approveDetails).find('input,select').prop('disabled', true).trigger('chosen:updated');
            }
        })

        $(this).on('change', scholarshipType, function() {
            var ajax = new AJAX_Helper({
                    url: SelectListURLDict.GetScholarshipsByScholarshipTypeId,
                    data: {
                        scholarshipTypeId: $(this).val(),
                    },
                    dataType: 'json'
                }
            );
        
            ajax.POST().done(function (response) {
                $(scholarship).append(getDefaultSelectOption($(scholarship)));
    
                response.forEach((item, index) => {
                    $(scholarship).append(getSelectOptions(item.value, item.text));
                });
        
                $(scholarship).trigger("chosen:updated");
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        })

        $(this).on('change', scholarship, function() {
            var ajax = new AJAX_Helper(
                {
                    url: GetScholarshipLimitBudget,
                    data: {
                        id: $(this).val(),
                        studentId: $(studentId).val()
                    },
                    dataType: 'json'
                }
            );
        
            ajax.POST().done(function (response) {
                $(limitAmount).val(response);
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        })
    
        $(this).on('change', scholarshipEffectiveTerm, function() {
            let currentId = $(this).parents(scholarshipModal).find(scholarship).val();

            var ajax = new AJAX_Helper(
                {
                    url: SelectListURLDict.GetScholarshipExpiryTermId,
                    data: {
                        academicLevelId: $(academicLevelId).val(),
                        selectedTermId: $(this).val(),
                        scholarshipId: currentId
                    },
                    dataType: 'json'
                }
            );
    
            ajax.POST().done(function (response) {
                $(scholarshipExpiryTerm).append(getDefaultSelectOption($(scholarshipExpiryTerm)));
                var isNoExpiryTerm = true;
        
                response.forEach((item, index) => {
                    $(scholarshipExpiryTerm).append(getSelectOptions(item.id, item.termText));
                    if (item.isScholarshipExpiryTerm) {
                        $(scholarshipExpiryTerm).find('option[selected]').prop('selected', false);
                        $(scholarshipExpiryTerm).find('option:last-child').prop('selected', true);
                        isNoExpiryTerm = false
                    }
                });
    
                $(scholarshipExpiryTerm).trigger("chosen:updated");
        
                if (isNoExpiryTerm) {
                    Alert.renderAlert("Error", "Expiry term is not created", "error");
                }
            })
            .fail(function(jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        })
    })

    $('#edit-scholarship-student-modal').on('shown.bs.modal', function(e) {
        let scholarshipStudentId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: EditScholarshipStudentUrl,
                data: {
                    id: scholarshipStudentId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-edit-scholarship-student').empty().append(response);
            $('.chosen-select').chosen();
            DateTimeInput.renderSingleDate($('.js-single-date'));
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })

    $('#voucher-details-modal').on('shown.bs.modal', function(e) {
        let voucherId = $(e.relatedTarget).data('value')

        var ajax = new AJAX_Helper(
            {
                url: VoucherDetailsUrl,
                data: {
                    id: voucherId,
                },
                dataType: 'json'
            }
        );

        ajax.GET().done(function (response) { 
            $('#modalWrapper-voucher-details').empty().append(response);
            RenderTableStyle.columnAlign();
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    })
})