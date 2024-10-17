let optionButton = '.js-curriculum-option'
let editOptionButton = '.js-curriculum-edit-option'
var corequisiteCourse = '.js-cascade-corequisite-course';
var equivalentCourse = '.js-cascade-equivalent-course';
var exportButton = '.js-export-excel';
var formBlackList = '#form-black-list';
$(document).ready(function() {
    var inputTableSpecialization = new RowAddAble({
        TargetTable: '#js-specialization-group',
        ButtonTitle: 'Specialization Group'
    })
    inputTableSpecialization.RenderButton();

    var inputTable = new RowAddAble({
        TargetTable: '#js-blacklist-course-table',
        ButtonTitle: 'Blacklist Course',
        ButtonTitle: 'Blacklist Course',
        TableTitle: 'Blacklist Courses'
    });
    inputTable.RenderButton();

    var tab = '#nav-link-' + getUrlParameter('tabIndex');
    $(tab).tab('show'); 

    $(exportButton).removeClass('d-none');
});

$(document).on('click', optionButton, function(e) {
    e.stopPropagation();
    let courseGroupId = $(this).data('group-id');
    let returnUrl = $(this).data('return-url');
    let specialGroupId = $(this).data('special-group-id');
    let isSpecialGroup = $(this).data('special-group');

    //$('#js-get-group-id').attr('value', courseGroupId);
    $(this).popover({
            content: `
                <a class="popover-link js-add-group mb-2" href="/CourseGroup/CreateGroup?groupId=${ courseGroupId }&returnUrl=${ returnUrl }&isSpecialGroup=${ isSpecialGroup }&&specialGroupId=${ specialGroupId }">
                    <i class="la la-plus"></i> Add Group
                </a>`,
            placement: 'bottom',
            trigger: 'focus',
            html: true
        }
    );
    $('.popover').popover().remove();
    $(this).popover('show');
})

$(document).on('click', exportButton, function(e) {
    $(formBlackList).on('submit', function() {
        $('.loader').fadeOut();
        $("#preloader").delay(1000).fadeOut("slow");
    })
})

$(document).on('click', editOptionButton, function(e) {
    e.stopPropagation();
    let courseGroupId = $(this).data('group-id');
    let returnUrl = $(this).data('return-url');
    let specialGroupId = $(this).data('special-group-id');
    let isSpecialGroup = $(this).data('special-group');
    
    $(this).popover({
            content: `
                <a class="popover-link js-add-group mb-2" href="/CourseGroup/EditGroup?id=${ courseGroupId }&returnUrl=${ returnUrl }&isSpecialGroup=${ isSpecialGroup }&&specialGroupId=${ specialGroupId }">
                    Edit Group
                </a><br>
                <a class="popover-link js-add-course" href="/CourseGroup/EditCourse?id=${ courseGroupId }&returnUrl=${ returnUrl }">
                   Edit Course
                </a>`,
            placement: 'bottom',
            trigger: 'focus',
            html: true
        }
    );
    $('.popover').popover().remove();
    $(this).popover('show');
})

$(document).on('click', function(e) {
    $(optionButton).popover('hide')
    $(editOptionButton).popover('hide')
})

$(function() {
    var corequisiteTable = new RowAddAble({
        TargetTable: '#js-corequisite-table',
        TableTitle: 'Corequisite',
        ButtonTitle: 'Corequisite'
    })

    corequisiteTable.RenderButton();

    var equivalentTable = new RowAddAble({
        TargetTable: '#js-equivalent-table',
        TableTitle: 'Equivalent',
        ButtonTitle: 'Equivalent'
    })

    equivalentTable.RenderButton();
});