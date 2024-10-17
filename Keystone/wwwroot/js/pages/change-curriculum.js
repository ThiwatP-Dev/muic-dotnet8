$( function () {
    var inputTable = new RowAddAble({
        TargetTable: '#js-add-course',
        ButtonTitle: 'Course',
        TableTitle: "Curriculum's Course"
    })

    inputTable.RenderButton();
});