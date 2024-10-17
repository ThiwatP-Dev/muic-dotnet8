let seatAcademicLevel = $('#AcademicLevelId');
let seatTerm = $('#TermId');
let seatFacultyId = $('#FacultyId')
let seatDepartmentId = $('#DepartmentId')
let seatCourse = $('#CourseIds');
let seatSection = $('#SectionNumbers');
let seatTableId = $('#js-course-seat')

var startEditButton = '.js-start-edit';
var cancelEditButton = '.js-cancel-edit';
var saveEditButton = '.js-save-edit';
var editedInput = '.js-edit-input';
var editedValue = '.js-edit-value';
var hideClass = 'd-none';
var parentRow;
var parentRowRemark;

$(seatAcademicLevel).on('change', function() {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTermsByAcademicLevelId,
            data: {
                id: $(this).val(),
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        seatCourse.empty()
        seatSection.empty()
        seatTerm.append(getDefaultSelectOption(seatTerm));

        response.forEach((item, index) => {
            seatTerm.append(getSelectOptions(item.id, item.termText));
        });
        
        seatCourse.trigger("chosen:updated");
        seatSection.trigger("chosen:updated");
        seatTerm.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(seatTerm).on('change', function(){

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesSelectList,
            data: {
                termId: $(this).val(),
                facultyId: seatFacultyId.val(), 
                departmentId: seatDepartmentId.val() 
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        seatCourse.empty();

        response.forEach((item, index) => {
            seatCourse.append(getSelectOptions(item.value, item.text));
        });

        seatCourse.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(seatCourse).on('change', function() {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetSectionByCourses,
            data: {
                courseIds : $(this).val(),
                termId : seatTerm.val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        seatSection.empty();

        response.forEach((item, index) => {
            seatSection.append(getSelectOptions(item.value, item.text));
        });

        seatSection.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(seatFacultyId).on('change', function() {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesSelectList,
            data: {
                facultyId : $(this).val(),
                departmentId: seatDepartmentId.val(),
                termId: seatTerm.val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        seatCourse.empty();
        seatSection.empty();

        response.forEach((item, index) => {
            seatCourse.append(getSelectOptions(item.value, item.text));
        });

        seatCourse.trigger("chosen:updated");
        seatSection.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(seatDepartmentId).on('change', function() {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetCoursesSelectList,
            data: {
                departmentId : $(this).val(),
                facultyId: seatFacultyId.val(),
                termId: seatTerm.val()
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        seatCourse.empty();
        seatSection.empty();

        response.forEach((item, index) => {
            seatCourse.append(getSelectOptions(item.value, item.text));
        });

        seatCourse.trigger("chosen:updated");
        seatSection.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(saveEditButton).on('click', function(e) {
    let parentRow = $(this).parents('tr');
    let sectionId = parentRow.find(startEditButton).data('section-id');
    let editElements = parentRow.find('.js-edit-element');
    let seatAvailable = editElements.find('#seat-available');
    let editElementRemark = $('#remark-' + sectionId);
    let remark = editElementRemark.find('#section-remark');
    let seatLimit = editElements.find('#seat-limit');
    if (seatLimit.val() < 0) {
        Alert.renderAlert('Error', ErrorMessage.NegativeData, 'warning');
    } else {
        $('#preloader').fadeIn();

        var ajax = new AJAX_Helper(
            {
                url: SeatEditUrl,
                data: {
                    id : sectionId,
                    seatLimit: seatLimit.val(),
                    remark: remark.val()
                },
                dataType: 'json'
            }
        );
        
        ajax.POST().done(function (response) { 
            if(response.isInvalidSeat)
            {
                editElements.find('#seat-available-value').text(response.section.seatAvailable);
                editElements.find('#seat-limit-value').text(response.section.seatLimit);
                editElementRemark.find('#section-remark-value').text("");
                editElementRemark.find('#section-remark-value').append("<b>Remark: </b>"  + response.section.remark);

                seatAvailable.value= response.section.seatAvailable;
                seatAvailable.data('default-value', response.section.seatAvailable);
                seatLimit.val(response.section.seatLimit);
                seatLimit.data('default-value', response.section.seatLimit);

                remark.val(response.section.remark);
                remark.data('default-value', response.section.remark);

                Alert.renderAlert("Warning", "Unable to save ,The number of seat limit joint section more than seat limit master section!", "warning");
            } else {
                editElements.find('#seat-available-value').text(response.section.seatAvailable);
                editElements.find('#seat-limit-value').text(response.section.seatLimit);
                editElementRemark.find('#section-remark-value').text("");
                editElementRemark.find('#section-remark-value').append("<b>Remark: </b>"  + response.section.remark);

                seatAvailable.value= response.section.seatAvailable;
                seatAvailable.data('default-value', response.section.seatAvailable);
                seatLimit.val(response.section.seatLimit);
                seatLimit.data('default-value', response.section.seatLimit);

                remark.val(response.section.remark);
                remark.data('default-value', response.section.remark);
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            ErrorCallback(jqXHR, textStatus, errorThrown);
        });
        
        $('#preloader').fadeOut();
    }
})

function columnTotal() {
    var bodyRows = $(seatTableId).find('tbody tr');
    var footerColumn = $(seatTableId).find('tfoot tr td');

    $(footerColumn).each(function(columnIndex) {

        if($(this).hasClass('js-col-total')) {
            var currentAmount = 0;

            $(bodyRows).each(function() {
                $(this).find('td').each(function(index) {
                    if (index == columnIndex) {
                        let cellAmount = $(this).find('div').html();
                        cellAmount = Number(cellAmount.replace(/[^0-9.-]+/g,""));
                        currentAmount += cellAmount;
                    }
                })
            })

            $(this).html(`<b>${ currentAmount }</b>`);
        }
    })
}

$(document).ready(function() {
    EditTable.renderEditTable(seatTableId)
})

var EditTable = (function() {
    
    var RenderEditTable = function(editTableClass) {
        let editTable = $(editTableClass);
        var saveButton = $(editTable).find('.js-save-edit');
        let cancelButton = $(editTable).find(cancelEditButton);
        let editButton = $(editTable).find(startEditButton);
        
        $('.editable-chosen-select').chosen();

        var nextElementOfSelect = $(editTableClass).find('select').next();
        nextElementOfSelect.toggleClass(hideClass);

        $(cancelButton).click(function() {
            parentRow = $(this).parents('tr');
            let sectionId = parentRow.find(startEditButton).data("section-id");
            parentRowRemark = $('#remark-' + sectionId);

            let restoreInputsRemark = parentRowRemark.find(editedInput);
            let restoreInputs = parentRow.find(editedInput);
            
            restoreInputs.each(function() {
                let defaultValue = $(this).data('default-value');
                $(this).val(defaultValue);
            })

            restoreInputsRemark.each(function() {
                let defaultValue = $(this).data('default-value');
                $(this).val(defaultValue);
            })

            toggleHide(parentRowRemark);
            toggleChosenHide(parentRowRemark);

            toggleHide(parentRow);
            toggleChosenHide(parentRow);
        })

        $(editButton).click(function() {
            parentRow = $(this).parents('tr');
            let sectionId = parentRow.find(startEditButton).data("section-id");
            parentRowRemark = $('#remark-' + sectionId);
            toggleHide(parentRow);
            toggleHide(parentRowRemark);
            parentRow.find('.chosen-container').removeClass(hideClass);
            parentRowRemark.find('.chosen-container').removeClass(hideClass);
        })

        $(saveButton).click(function() {
            parentRow = $(this).parents('tr');
            let sectionId = parentRow.find(startEditButton).data("section-id");
            saveEdit(parentRow);
            toggleHide(parentRow);
            toggleChosenHide(parentRow);

            parentRowRemark = $('#remark-' + sectionId);
            saveEdit(parentRowRemark);
            toggleHide(parentRowRemark);
            toggleChosenHide(parentRowRemark);
        })
    }

    var ToggleEdit = function(parentRow)  {
        toggleHide(parentRow);
    }

    var toggleHide = function(parentRow) {
        let inputElement = parentRow.find(editedInput);
        let valueElement = parentRow.find(editedValue);
        let selectElement = parentRow.find('select');
        let nextElementOfSelect = selectElement.next();

        inputElement.toggleClass(hideClass);
        valueElement.toggleClass(hideClass);
        nextElementOfSelect.toggleClass(hideClass);
        
        selectElement.chosen({inherit_select_classes: true});
        selectElement.trigger('chosen:updated');
                
        parentRow.find(startEditButton).toggleClass(hideClass);
        parentRow.find('.js-quit-edit').toggleClass(hideClass);
    }

    var toggleChosenHide = function(parentRow) {
        parentRow.find('.chosen-container').toggleClass(hideClass);
    }

    var saveEdit = function(parentRow) {
        let valueElements = parentRow.find(editedValue);
        let inputElement = parentRow.find(`input${ editedInput }, select${ editedInput }`);

        inputElement.each( function(index, item) {
            if ($(this).is('input')) {
                $(this).data('default-value', item.value);
                $(valueElements[index]).html(item.value);
            } else if ($(this).is('select')) {
                let selectedText = $(item).next().find('a > span').html();
                $(this).data('default-value', item.value);
                $(valueElements[index]).html(selectedText);
            }
            
        })
    }

    return {
        toggleHideEdit : ToggleEdit,
        renderEditTable : RenderEditTable
    };
})();