let instructorLoad = 0;
let instructorParent = ".instructor-parent"
var instructorSelect = $("#create-load-instructor");
var facultySelect = $("#create-load-faculty");

// important course class
let courseSelect = ".course-select"
let courseParent = ".course-parent"
let courseRefer = ".course-reference"

var Option = (function() {

    var RenderCourses = function(selectInput, instructorData) {
        if (instructorData.val()) {

            var ajax = new AJAX_Helper(
                {
                    url: TeachingLoadGetInstructorCourseUrl,
                    data: {
                        id: instructorData.val() ,
                    },
                    dataType: 'json'
                }
            );
        
            ajax.POST().done(function (response) { 
        
                selectInput.append(getDefaultSelectOption(selectInput));
        
                response.forEach((item, index) => {
                    selectInput.append(getSelectOptions(item.id, item.text));
                });
        
                selectInput.trigger("chosen:updated");
            })
            .fail(function (jqXHR, textStatus, errorThrown) { 
                ErrorCallback(jqXHR, textStatus, errorThrown);
            });
        }
    }

    var CountLoad = function(showLoad) {
        instructorLoad = 0;
        let allLoadInput = $('.course-ancestor').find('.js-get-load');
        allLoadInput.each(function() {
            if (this.value) {
                instructorLoad += parseInt(this.value);
            }
        });
        showLoad.html(instructorLoad);
    }

    return {
        renderCourses : RenderCourses,
        countLoad : CountLoad
    };
})();

$(facultySelect).on('change', function() {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetInstructorsByFacultyId,
            data: {
                id: $(this).val(),
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 

        instructorSelect.append(getDefaultSelectOption(instructorSelect));

        response.forEach((item, index) => {
            instructorSelect.append(getSelectOptions(item.id, item.codeAndName));
        });

        instructorSelect.trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });

    $(facultySelect).prop('disabled', true).trigger("chosen:updated")

    let getFacultyParent = this.value;
    let hiddenFacultyInput = $('.instructor-ancestor').find('.faculty-reference');

    hiddenFacultyInput.each(function() {
        $(this).attr('value', getFacultyParent);
    });
});

//change course with instructor index
//get course and total section
$(document).on('change', function() {
    let instructorCount = instructorSelect.length; //default at 1

    for(var i = 1 ; i <= instructorCount ; i++) {
        var thisInstructor = $('#create-load-instructor-'+i)

        $(thisInstructor).on('change', function() {

            //select all courses in this instructor
            let allCourses = $("#tl-add-instructor-accordion" + (i-1)).find(courseSelect);
            allCourses.each(function() {
                Option.renderCourses($(this), thisInstructor);
            });

            //update instructor parent value to their children
            var getInstructorParent = this.value;
            let instructorParentElement = $(this).parents(instructorParent);
            let hiddenInstructorInput = instructorParentElement.find('.instructor-reference');

            hiddenInstructorInput.each(function() {
                $(this).attr('value', getInstructorParent);
            });
        });
    }

    //update course parent value to their children
    $(document).on('change', courseSelect, function() {
        var getCourseParent = this.value;
        let courseParentElement = $(this).parents(courseParent);
        let hiddenCourseInput = courseParentElement.find(courseRefer);

        hiddenCourseInput.each(function() {
            $(this).attr('value', getCourseParent);
        });
    });

    //update total section parent value to their children
    $(document).on('change', '.total-section-input', function() {
        var getTotalSectionParent = this.value;
        let totalSectionParentElement = $(this).parents(courseParent);
        let hiddenTotalSectionInput = totalSectionParentElement.find(courseRefer);

        hiddenTotalSectionInput.each(function() {
            $(this).attr('value', getTotalSectionParent);
        });
    });

    //calculate total loads
    let totalLoad = $('#js-load-count');
    Option.countLoad(totalLoad);
});