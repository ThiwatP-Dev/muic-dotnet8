var formId = '#export-excel-form';

function checkDuplicatedCourseGroup()
{
    var courseIds = $(".js-course-id").map((i, el) => el.value);
    var courseIdsSorted = courseIds.sort(); 
    var courseIdsDuplicated = [];
    for (var i = 0; i < courseIdsSorted.length - 1; i++) {
        if (courseIdsSorted[i + 1] == courseIdsSorted[i]) {
            courseIdsDuplicated.push(courseIdsSorted[i]);
        }
    }

    if (courseIdsDuplicated.length > 0) 
    {
        var errorMsg = "";
        courseIdsDuplicated.forEach(element => {
            errorMsg += "\n" + $(".js-course-name-" + element)[0].value;
        });
        Alert.renderAlert("Error", "Duplicated courses in groups." + errorMsg, "error");
        return false;
    }
    return true;
}

function saveForm()
{
    if (checkDuplicatedCourseGroup())
    {
        $("#IsPublish").val(false);
        $("#js-form").submit();
    }
}

function publishForm()
{
    if (checkDuplicatedCourseGroup())
    {
        $("#IsPublish").val(true);
        $("#js-form").submit();
    }
}

function print()
{
    if (checkDuplicatedCourseGroup())
    {
        $("#IsPrint").val(true);
        $("#js-form").submit();
    }
}

$(formId).on('submit', function() {
    $('.loader').fadeOut();
    $("#preloader").delay(1000).fadeOut("slow");
})