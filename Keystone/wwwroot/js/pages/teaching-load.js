$(document).ready(function() {
    var newCourse = $('.course-ancestor').html(); //get a course accordion template 
    var newInstructor = $('.instructor-ancestor').html(); //get an instructor accordion template
    var instructorCount = 1;
    var loadCount = 0;

    //can be more than a button per page
    $(document).on('click', '.optional-course', function() {
        // make all old accordians to collapse 
        $('.course-toggle').each(function() {
            $(this).addClass('collapsed')
            $(this).attr('aria-expanded', 'false')
        })
        $('.course-descendant').each(function() {
            $(this).removeClass('show')
        })

        // find out which instructor own this course then add new course
        let courseInstructorId = $(this).parents('.instructor-descendant').attr('id');
        let courseCount = $('#'+courseInstructorId).find('.course-parent').length + 1;
        let courseInstructorIndex = courseInstructorId.substring(27).split('-');

        let addCourse = $(newCourse);
        addCourse[0].setAttribute('id', 'tl-add-course-accordion' + courseInstructorIndex[0] + '-' + courseCount);
        $($('.course-ancestor')[courseInstructorIndex[0] - 1]).append(addCourse);

        // set newest course accordian to relative with the instructor
        let toggleCount = $('#'+courseInstructorId).find('.course-toggle');
        let descendantCount = $('#'+courseInstructorId).find('.course-descendant');
        $(toggleCount)[courseCount-1].setAttribute('data-parent', '#tl-add-course-accordion'+ courseInstructorIndex[0] + '-' + courseCount)
        $(toggleCount)[courseCount-1].setAttribute('data-target', '#tl-add-course-accordion' + courseInstructorIndex[0] + '-' + courseCount + '-1')
        $(descendantCount)[courseCount-1].setAttribute('id', 'tl-add-course-accordion' + courseInstructorIndex[0] + '-' + courseCount + '-1')

        // assign instructor index, course index and count to hidden input in table
        let tableId = 'js-loads-' + courseInstructorIndex[0] + '-' + courseCount;
        $($(descendantCount)[courseCount-1]).find('table').attr('id', tableId);

        // write instructor value to a new data set
        let instructorParentElement = $('#'+tableId).parents('.instructor-ancestor');
        var getInstructorParent = instructorParentElement.find('#create-load-instructor-' + courseInstructorIndex[0]).val();
        let hiddenInstructorInput = instructorParentElement.find('.instructor-reference');

        hiddenInstructorInput.each( function(){
            $(`#${ tableId } .instructor-reference:last`).attr('value',getInstructorParent);
        });

        // write faculty value to a new data set
        let getFacultyParent = $('#create-load-faculty').val();
        let hiddenFacultyInput = $('.instructor-ancestor').find('.faculty-reference');

        hiddenFacultyInput.each( function(){
            $(this).attr('value',getFacultyParent);
        });

        // Course chosen select Re-render
        let selectParent = $('#tl-add-course-accordion' + courseInstructorIndex[0] + '-' + courseCount).find('.select-parent');
        let courseSelect = selectParent.find('select')
        courseSelect.removeAttr('style');
        courseSelect.attr('id', 'create-load-course-' + courseCount)
        selectParent.empty();
        selectParent.append(courseSelect);

        let thisCourse = $(selectParent).find('select');
        thisCourse.chosen();
        thisCourse.after('<label>Course</label>');
        RenderOption.renderCourses(thisCourse, $('#create-load-instructor-' + courseInstructorIndex[0]));
        
        let allColumn = $(document).find('tbody > tr > td');
        allColumn.each( function(){
            let isSelect = $(this).find('select');
            if (isSelect.length){
                isSelect.removeAttr('style');

                let columnData = $(this).children('div');
                columnData.empty();
                columnData.append(isSelect);
                $(columnData).find('select').chosen();
            }
        });

        // assign data set index 
        loadCount = 0;
        let loadRow = $('.load-row');
        loadRow.each( function(){
            let cellData = $(this).find('select,input,div');
            cellData.each( function(){
                if (this.hasAttribute("name")){
                    let nameValue = $(this).attr("name").replace(/\[(\d)\]/g, '['+ loadCount + ']');
                    $(this).attr('name', nameValue);
                    
                }

                if (this.hasAttribute("id")){
                    let idValue = $(this).attr("id").replace(/(\d)/g, loadCount);
                    $(this).attr('id', idValue);
                }
            });

            loadCount++;
        });
            
    });

    /* --- add instructor event --- */ 
    //only one button per page
    $('.optional-instructor').on('click', function(){
        // make all old accordians to collapse 
        $('.instructor-toggle').each(function() {
            $(this).addClass('collapsed')
            $(this).attr('aria-expanded', 'false')
        })
        $('.instructor-descendant').each(function() {
            $(this).removeClass('show')
        })

        // add instructor count and use count for a newest accordian reference
        instructorCount += 1;
        let addInstructor = $(newInstructor);
        addInstructor[0].setAttribute('id', 'tl-add-instructor-accordion' + instructorCount)
        $('.instructor-ancestor').append(addInstructor);
        $('.instructor-toggle')[instructorCount-1].setAttribute('data-parent', '#tl-add-instructor-accordion' + instructorCount)
        $('.instructor-toggle')[instructorCount-1].setAttribute('data-target', '#tl-add-instructor-accordion' + instructorCount + '-1')
        $('.instructor-descendant')[instructorCount-1].setAttribute('id', 'tl-add-instructor-accordion'  + instructorCount + '-1')
        
        // set the first course to relative with this new instructor
        let firstCourseParent = $('#tl-add-instructor-accordion' + instructorCount).find('.course-parent');
        let firstCourseToggle = $('#tl-add-instructor-accordion' + instructorCount).find('.course-toggle');
        let firstCourseDescendant = $('#tl-add-instructor-accordion' + instructorCount).find('.course-descendant');
        $(firstCourseParent)[0].setAttribute('id', 'tl-add-course-accordion' + instructorCount + '-1')
        $(firstCourseToggle)[0].setAttribute('data-parent', '#tl-add-course-accordion'+ instructorCount + '-1')
        $(firstCourseToggle)[0].setAttribute('data-target', '#tl-add-course-accordion' + instructorCount + '-1-1')
        $(firstCourseDescendant)[0].setAttribute('id', 'tl-add-course-accordion' + instructorCount + '-1-1')

        let tableId = 'js-loads-' + instructorCount + '-1';
        $($(firstCourseDescendant)[0]).find('table').attr('id', tableId);

        // write faculty value to a new data set
        let getFacultyParent = $('#create-load-faculty').val();
        let hiddenFacultyInput = $('.instructor-ancestor').find('.faculty-reference');

        hiddenFacultyInput.each( function(){
            $(this).attr('value',getFacultyParent);
        });

        // Instructor chosen select Re-render
        let instructorSelectParent = $('#tl-add-instructor-accordion' + instructorCount).find('.select-parent:first');
        let instructorSelect = instructorSelectParent.find('select')
        instructorSelect.removeAttr('style');
        instructorSelect.attr('id', 'create-load-instructor-' + instructorCount)
        instructorSelectParent.empty();
        instructorSelectParent.append(instructorSelect);

        let thisInstructor = $(instructorSelectParent).find('select');
        thisInstructor.chosen();
        thisInstructor.after('<label>Instructor</label>');
        RenderOption.renderInstructors(thisInstructor);

        // Course chosen select Re-render
        let selectParent = $('#tl-add-course-accordion' + instructorCount + '-1').find('.select-parent');
        let courseSelect = selectParent.find('select')
        courseSelect.removeAttr('style');
        selectParent.empty();
        selectParent.append(courseSelect);
        $(selectParent).find('select').chosen();
        $(selectParent).find('select').after('<label>Course</label>');

        // loads table chosen select Re-render
        let allColumn = $(document).find('tbody > tr > td');
        allColumn.each( function(){
            let isSelect = $(this).find('select');
            if (isSelect.length){
                isSelect.removeAttr('style');

                let columnData = $(this).children('div');
                columnData.empty();
                columnData.append(isSelect);
                $(columnData).find('select').chosen();
            }
        });

        // assign data set index 
        loadCount = 0;
        let loadRow = $('.load-row');
        loadRow.each( function(){
            let cellData = $(this).find('select,input,div');
            cellData.each( function(){
                if (this.hasAttribute("name")){
                    let nameValue = $(this).attr("name").replace(/\[(\d)\]/g, '['+ loadCount + ']');
                    $(this).attr('name', nameValue);
                    
                }

                if (this.hasAttribute("id")){
                    let idValue = $(this).attr("id").replace(/(\d)/g, loadCount);
                    $(this).attr('id', idValue);
                }
            });

            loadCount++;
        });
    });
});