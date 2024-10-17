var gradingFormId = '#grading-wizard';

var allocationTableId = '#js-allocation-table';
var allocationTotalScore = '#js-total-score';
var allocationShowLimit = '#js-display-limit';
var allocationType = '.js-allocation-type';
var allocationScore = '.js-type-score';
var allocationAbbr = '.js-type-abbr';

var scoringTableId = '#js-scoring-table';
var scoringStudent = '.js-student-score';
var scoringTotal = '.js-total-score';
var percentage = '.js-score-percentage';

var gradeRange;
var gradeTable = '#js-grade-table';
var gradeTitle = '.js-grade-title';
var gradeScores = '.js-grade-score';
var gradeWeight = '.js-grade-weight';
var resultTable = '#js-grading-result';
var gradeMaxScores = 'input[data-score="max"]';
var gradeMinScores = 'input[data-score="min"]';
var averageGPA = '#average-gpa';
var minScore = '#min-score';
var maxScore = '#max-score';
var meanScore = '#mean-score';
var sdScore = '#sd-score';
var withdrawCount = '#withdraw-count';

var scoreList;
var percentageList;
var originalLineDraws;
var gradeCharts;

// ---------- function ---------- //
function initWizard() {
    var gradingForm = $(gradingFormId);

    gradingForm.steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        onStepChanging: function (event, currentIndex, newIndex)
        {
            gradingForm.validate().settings.ignore = ":disabled";
            gradingForm.valid();
            return saveState(ajaxCallBack, gradingForm, 'next', currentIndex, newIndex);
        },
        onStepChanged: function (event, currentIndex) {
            // if (currentIndex == 2) {
            //      gradeChartRenderer();
            // }
            $('#js-submit-form').data('index', currentIndex);
        },
        onFinishing: function (event, currentIndex)
        {
            gradingForm.validate().settings.ignore = ":disabled";
            gradingForm.valid();
            return saveState(ajaxCallBack, gradingForm, 'finish');
        }
    });

    $('.actions ul li a[href="#previous"]').parent().after(getSaveStepsButton())
    
    $('#preloader').fadeOut();
}

/* Save each step function */
function saveState(callBack, formData, submitType, currentIndex, nextIndex) {
    var currentFormData = formData.serialize();

    switch(submitType) {
        case "save":
            var targetAction = `${ GradeManagementSaveUrl }?currentIndex=${ currentIndex }`;
            var submitData = currentFormData;
            break;

        case "next":
            var targetAction = `${ GradeManagementContinueUrl }?currentIndex=${ currentIndex }&nextIndex=${ nextIndex }`;
            var submitData = currentFormData;
            break;

        case "finish":
            var targetAction = GradeManagementFinishUrl;
            var submitData = currentFormData;
            break;
    }

    var ajax = new AJAX_Helper(
        {
            url: targetAction,
            data: submitData,
            dataType: 'application/x-www-form-urlencoded; charset=utf-8'
        }
    );

    return ajax.POST().done(function (response) {
        callBack(response);

        var gradingAllocationId = $('#js-grading-allocation-id').val();
        if (nextIndex == 0) {
            gradingAllocation(gradingAllocationId);
        }

        if (nextIndex == 2) {
            standardGradingGroups($('#js-selected-standard-grading-group-ids').val());
        }
        
        if (nextIndex == 3) {
            calculateGradingResult(gradingAllocationId);
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function calculateGradingResult(allocationId) {
    var ajax = new AJAX_Helper({
            url: GradeManagementGradingResultUrl,
            data : { 
                gradingAllocationId: allocationId, 
            },
            dataType: 'html'
        }
    );

    ajax.POST().done(function (response) {
        $(resultTable).empty().html(response);

        // RenderTableStyle.columnAlign();
        // var children = $(resultTable).children('tbody')[0].children;
        // for (var i = 0; i < response.length; i++) {
        //     if (children.length != undefined) {
        //         var id = "#" + response[i].registrationCourse.courseCode + "_" + response[i].registrationCourse.sectionNumber + "_" + response[i].student.code + "_";
        //         $(id + 'TotalScoreId').html(response[i].totalScore.toFixed(2));
        //         $(id + 'PercentageId').html(response[i].percentage.toFixed(2));
        //         if ($(id + 'IsWithdrawal').val() == "True") 
        //         {
        //             $(id + 'GradeId').html(response[i].registrationCourse.grade.name);
        //         }
        //         else 
        //         {
        //             $(id + 'GradeId').html(response[i].grade.name);
        //         }
        //         if (response[i].studentScores != undefined && response[i].studentScores.length > 0) 
        //         {
        //             for (var j = 0; j < response[i].studentScores.length; j++) 
        //             {
        //                 $(id + j).html(response[i].studentScores[j].fullScore.toFixed(2));
        //             };
        //         }

        //         if (response[i].isCheating) 
        //         {
        //             $(id + 'TotalScoreId').parents('tr').addClass('bg-danger-pastel')
        //         }
        //         else 
        //         {
        //             $(id + 'TotalScoreId').parents('tr').removeClass('bg-danger-pastel')
        //         }
        //     }
        // }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function ajaxCallBack(response) {
    let allocationId = parseInt(response);
    if (isNaN(allocationId)) {
        window.location.href = response
    }
    let isSuccess = allocationId == 0 ? false : true;
    if (isSuccess) {
        $('#js-grading-allocation-id').val(allocationId);
        let a = $('#js-grading-allocation-reset-id');
        a.val(allocationId);
    }

    return isSuccess;
}

/* Allocation step function */
function calculateTotalScore(scoreList, resultArea) {
    let currentScore = 0;

    $(scoreList).each( function() {
        let currentFullScore = $(this).val();

        if (currentFullScore === "") {
            currentFullScore = 0;
        }

        currentScore += parseFloat(currentFullScore);
    })

    $(resultArea).html(currentScore.toFixed(2));
}

function checkTotalScore() {
    let totalScore = parseFloat($(allocationTotalScore).html());
    let limit = parseFloat($('#js-score-limit').val());

    if (totalScore > limit) {
        $(allocationShowLimit).html('Score exceed limit');
        $(allocationTotalScore).removeClass('color-success').addClass('color-warning')
    } else if (totalScore <= limit) {
        let leftScore = parseFloat(limit - totalScore).toFixed(2)
        $(allocationShowLimit).html(`${ leftScore } left`);
        $(allocationTotalScore).removeClass('color-warning').addClass('color-success')
    } else {
        $(allocationShowLimit).html(`No limit score`);
    }
}

function gradingAllocation(allocationId) {
    var ajax = new AJAX_Helper({
            url: GradeManagementGetGradingAllocationUrl,
            data : { 
                gradingAllocationId: allocationId, 
            },
            dataType: 'html'
        }
    );

    ajax.GET().done(function (response) {
        var children = $('#js-grade-table').children('tbody')[0].children;
        for (var i = 0; i < response.length; i++) {
            if (children.length != undefined) {
                $('#GradingScoresCurve_' + i + '__Minimum').val(response[i].minimum.toFixed(2));
                $('#GradingScoresCurve_' + i + '__Maximum').val(response[i].maximum.toFixed(2));
                $('#GradingScores_' + i + '__Minimum').val(response[i].minimum.toFixed(2));
                $('#GradingScores_' + i + '__Maximum').val(response[i].maximum.toFixed(2));
            }
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

/* Grading step function */
function gradePointSuggestion(score, scoreType, currentRow, gradeChart, index) {
    let nextRow = currentRow.next('tr');
    let prevRow = currentRow.prev('tr');

    if (score.match(RegularExpressions.IsIntOrFloat) != null) {
        if (nextRow.length != 0 && scoreType === "min") {
            nextRow.find(gradeMaxScores).val((parseFloat(score) - 0.01).toFixed(2))
        }
        
        if (prevRow.length != 0 && scoreType === "max") {
            prevRow.find(gradeMinScores).val((parseFloat(score) + 0.01).toFixed(2))
        }
    } else if (score === "") {
        if (nextRow.length != 0 && scoreType === "min") {
            nextRow.find(gradeMaxScores).val("")
        }
        
        if (prevRow.length != 0 && scoreType === "max") {
            prevRow.find(gradeMinScores).val("")
        }
    }
    //isGradeRowCompleted(nextRow, gradeChart, index);
    //isGradeRowCompleted(prevRow, gradeChart, index);
}

function isGradeRowCompleted(inputRow, gradeChart, index) {
    let rowMaxScore = $(inputRow).find(gradeMaxScores).val();
    let rowMinScore = $(inputRow).find(gradeMinScores).val();
    let rowWeight = $(inputRow).find(gradeWeight).val();
    let rowGrade = $(inputRow).find(gradeTitle).html();

    if (rowMaxScore != '' && rowMinScore != '' && rowWeight != '' && rowGrade != null && rowGrade != "") {
        let gradeIndex = gradeRange[index].findIndex(x => x.grade === rowGrade);
        let studentCount = 0;

        $(percentageList).each( function() {
            if (this <= rowMaxScore && this >= rowMinScore) {
                studentCount++;
            }
        });

        let currentGrade = {
            grade: rowGrade,
            min: parseFloat(rowMinScore),
            max: parseFloat(rowMaxScore),
            weight: parseFloat(rowWeight),
            count : studentCount
        }

        if (gradeIndex !== -1) {
            gradeRange[index][gradeIndex] = currentGrade
        } else {
            gradeRange[index].push(currentGrade)
        }

        gradeChart.config.options.gradeDistribution = gradeRange[index];
        gradeChart.update();
        var ctx = gradeChart.chart.ctx;
        var newGrades = gradeChart.config.options.gradeDistribution;
        drawGrade(ctx, newGrades, gradeChart, index)

        calculateGPA(index);
    }
}

function calculateGPA(index) {
    var sum = 0;
    var count = 0;
    $(percentageList).each(function() {
        if (this != -1) {
            gradeRange[index].forEach(grade => {
                if (grade.min <= this && grade.max >= this) {
                    sum += grade.weight;
                }
            });
            count++;
        }
    });
    $(averageGPA).text(Number(sum / count).toFixed(2));
    $(minScore).text(Math.min.apply(Math, scoreList));
    $(maxScore).text(Math.max.apply(Math, scoreList));

    var sum = 0;
    for (var i = 0; i < scoreList.length; i++ ){
        sum += parseFloat( scoreList[i], 10 ); //don't forget to add the base
    }
    var avg = sum / scoreList.length;
    $(meanScore).text(avg);

    $(sdScore).text(standardDeviation(scoreList));
    $(withdrawCount).text($("#js-withdraw-count").val());
    
}

function standardDeviation(values) {
    var avg = average(values);
    
    var squareDiffs = values.map(function(value){
      var diff = value - avg;
      var sqrDiff = diff * diff;
      return sqrDiff;
    });
    
    var avgSquareDiff = average(squareDiffs);
  
    var stdDev = Math.sqrt(avgSquareDiff);
    return stdDev;
  }
  
  function average(data) {
    var sum = data.reduce(function(sum, value){
      return sum + value;
    }, 0);
  
    var avg = sum / data.length;
    return avg;
  }

function standardGradingGroups(standardGradingGroupIds) {
    var ajax = new AJAX_Helper(
        {
            url: GradeManagementGetSelectedStandardGradingGroupsUrl,
            data: {
                ids: standardGradingGroupIds
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) {
        $('#js-get-standard-grading').empty().html(response);
        gradingCurve($('#js-grading-allocation-id').val());
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function gradingCurve(allocationId) {
    var ajax = new AJAX_Helper({
            url: GradeManagementGetGradingStatisticsUrl,
            data : { 
                gradingAllocationId: allocationId, 
            },
            dataType: 'html'
        }
    );

    ajax.GET().done(function (response) {
        var children = $('#js-grade-table').children('tbody')[0].children;
        for (var i = 0; i < response.length; i++) {
            if (children.length != undefined) {
                $('#GradingScoresCurve_' + i + '__Minimum').val(response[i].minimum.toFixed(2));
                $('#GradingScoresCurve_' + i + '__Maximum').val(response[i].maximum.toFixed(2));
            }
        }
        gradeChartRenderer();
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

function gradeChartRenderer() {
    if (gradeCharts !== undefined) {
        for (var i = 0; i < gradeCharts.length; i++)
        {
            if (gradeCharts[i] !== undefined) {
                $('#grade-chart-' + i).html('');
                var canvas = gradeCharts[i].ctx.canvas;

                // Store the current transformation matrix
                gradeCharts[i].ctx.save();

                // Use the identity matrix while clearing the canvas
                gradeCharts[i].ctx.setTransform(1, 0, 0, 1, 0, 0);
                gradeCharts[i].ctx.clearRect(0, 0, canvas.width, canvas.height);

                // Restore the transform
                gradeCharts[i].ctx.restore();
                
                //gradeCharts[i].destroy();   
            }
        }
        Chart.helpers.extend(Chart.controllers.bar.prototype.draw, null);
        Chart.controllers.bar.prototype.draw = function () {};
    }

    var graphCount = parseInt($('#graph-count').val());
    gradeRange = new Array(graphCount);
    originalLineDraws = new Array(graphCount);
    gradeCharts = new Array(graphCount);
    for (var graphIndex = 0; graphIndex < graphCount; graphIndex++)
    {
        gradeRange[graphIndex] = [];
        originalLineDraws[graphIndex] = Chart.controllers.bar.prototype.draw;
        scoreList = [];
        $(scoringTotal + '-' + graphIndex).each( function() {
            var scoreindex = $(this).parents('tr').find('.tr-index').val();
            if ($("#checkvalue" + scoreindex).val() != "true") {
                scoreList.push(parseFloat($(this).html()));
            }
        });
    
        let studentScore = [];
        let scoreLabels = [];
        let bgColors = [];
        let lineColors = [];
        let maxScore = $('#js-max-limit').html();
        let maxStudent = $('#js-max-limit').data('total-students');
    
        percentageList = [];
        $(scoringTotal + '-' + graphIndex).each( function() {
            var scoreindex = $(this).parents('tr').find('.tr-index').val();
            if ($("#checkvalue" + scoreindex).val() != "true") {
                percentageList.push(parseFloat($(this).html()) * 100 / maxScore);
            }
        });
    
        // main score data
        for (i = 0; i <= 100; i++) {
            var counter = 0;
            scoreLabels.push(i);
            if (percentageList.includes(i)) {
                counter = percentageList.filter(score => parseInt(score) == i).length;
            }
            studentScore.push(counter);
    
            bgColors.push('rgba(44, 48, 77, 0.5)');
            lineColors.push('rgba(44, 48, 77, 0.8)');
        }
    
        var scoreData = {
            labels: scoreLabels, // score length (0 - max score)
            datasets: [{
                data: studentScore, // counter
                backgroundColor: bgColors,
                borderColor: lineColors,
                borderWidth: 1,
                yAxisID: "y-left"
            },{
                data: studentScore, // counter
                barThickness: 0,
                yAxisID: "y-right"
            }]
        }
    
        var chartArea = $('#grade-chart-' + graphIndex);
        let chartTitle = $(chartArea).data('chart-title'),
            xTitle = $(chartArea).data('axis-x-title'),
            yTitle = $(chartArea).data('axis-y-title');

        gradeCharts[graphIndex] = new Chart(chartArea, {
            type: 'bar',
            data: scoreData,
            options: {
                responsive: true,
                barValueSpacing: 1,
                title: {
                    display: true,
                    text: chartTitle,
                    fontSize: 18,
                    lineHeight: 3
                },
                legend: { // display what is bar details
                    display: false
                },
                scales: {
                    xAxes: [{
                        scaleLabel: {
                            display: true,
                            labelString: xTitle,
                            fontSize : 14
                        },
                        ticks: {
                            min: 0,
                            stepSize: 1
                        }
                    }],
                    yAxes: [{
                        id: "y-left",
                        position: "left",
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: yTitle,
                            fontSize : 14
                        },
                        ticks: {
                            max: maxStudent, 
                            beginAtZero: true,
                            stepSize: 1
                        }
                    }, {
                        id: "y-right",
                        position: "right",
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: yTitle,
                            fontSize : 14
                        },
                        ticks: {
                            max: maxStudent, 
                            beginAtZero: true,
                            stepSize: 1
                        }
                    }]
                },
                gradeDistribution: gradeRange[graphIndex]
            }
        });
        $(document).find(gradeScores + '-' + graphIndex).each(function() {
            gradeScoresKeyUp($(this), gradeCharts[graphIndex], graphIndex);
        });
    }
}

function drawGrade(canvas, gradeList, gradeChart, graphIndex) {
    Chart.helpers.extend(Chart.controllers.bar.prototype, {
        draw: function () {
            canvas.save();

            $(gradeList).each( function(index, item) {
                
                var fillColor = getGradeColor(item.grade, 0.25),
                    lineColor = getGradeColor(item.grade, 1);

                var minScore = item.min,
                    maxScore = item.max,    
                    xAxis = gradeChart.scales['x-axis-0'],
                    yAxis = gradeChart.scales['y-left'];

                var x1 = xAxis.getPixelForValue(minScore-1),
                    x2 = xAxis.getPixelForValue(maxScore),
                    y1 = yAxis.bottom,
                    y2 = yAxis.top;

                canvas.beginPath();
                canvas.strokeStyle = lineColor;
                canvas.fillStyle = fillColor;
                canvas.moveTo(x1, y1);
                canvas.lineTo(x1, y2);
                canvas.lineTo(x2, y2);
                canvas.lineTo(x2, y1);
                canvas.stroke();
                canvas.fill();
                canvas.textAlign = 'center';
                canvas.fillStyle = lineColor;
                canvas.fillText(`${ item.grade } (${ item.count })`, x1 + ((x2 - x1) / 2), y2 - 10);
            })

            canvas.restore();
            return originalLineDraws[graphIndex].apply(this, arguments);
        }
    });
}

/* Confirm step function */
function editGrade(currentRow, buttonType) {
    let editInput = $(currentRow).find('.js-edit-input');
    let editValue = $(editInput).val();

    let result = $(currentRow).find('.js-edit-value');
    let restoreButton = $(currentRow).find('.js-restore-edit');

    let originalGrade = $(currentRow).find('.original-grade');
    let originalGradeValue = originalGrade.data('original');
   
    switch(buttonType) {
        case "edit":
            originalGrade.addClass('d-none');
            break;

        case "restore":
            originalGrade.addClass('d-none');
    
            result.removeClass('color-danger');
            result.html(originalGradeValue);
            $(editInput).val(originalGradeValue);
            $(editInput).data('default-value', originalGradeValue);

            restoreButton.addClass('d-none');
            break;

        case "save":
        case "cancel":
            if (editValue == originalGradeValue) {
                result.removeClass('color-danger');
                originalGrade.addClass('d-none');
                restoreButton.addClass('d-none');
            } else {
                result.addClass('color-danger');
                originalGrade.removeClass('d-none');
                restoreButton.removeClass('d-none');
            }
            break;
    }
}

function toggleSkipScore(index)
{
    //alert(index + "---" + $('#check'+index).is(':checked') + "---" + ($('#check'+index).val() == "on"));
    var className = '.js-student-score-' + index;
    $(className).each( function() {
        $(this).prop("disabled", $('#check'+index).is(':checked'));
        $('#checkvalue'+index).val($('#check'+index).is(':checked') ? "true" : "false");
    })
}

// ---------- event ---------- //
/* Save each step event */
$(gradingFormId).on('click', '#js-submit-form', function() {
    let currentIndex = $(this).data('index');
    saveState(ajaxCallBack, $(gradingFormId), 'save', currentIndex);
})

$(gradingFormId).on('click', '#js-export-score', function() {
    var currentFormData = $(gradingFormId).serialize();
    var ajax = new AJAX_Helper(
        {
            url: GradeManagementExportUrl,
            data: currentFormData,
            dataType: 'application/x-www-form-urlencoded; charset=utf-8'
        }
    );

    return ajax.POST().done(function (response) {
        if (response.isSucceeded) {
            window.location = '/GradeManagement/Download?fileGuid=' + response.result;
        }
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
})

$(gradingFormId).on('click', '#js-import-score', function() {
    var form_data = new FormData($(gradingFormId)[0]);
    $.ajax({
        url: GradeManagementImportUrl,
        type: 'POST',
        data: form_data,
        processData: false,
        contentType: false,
        success: function (response) {
            $('#js-get-students-score').empty().html(response);
            RenderTableStyle.columnAlign();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ErrorCallback(jqXHR, textStatus, errorThrown);
        }
    });
})

/* Allocation step event */
$(gradingFormId).on('keyup', '#js-score-limit', function() {
    checkTotalScore();
})

$(gradingFormId).on('keyup', allocationScore, function() {
    calculateTotalScore(allocationScore, allocationTotalScore);
    checkTotalScore();
})

$(gradingFormId).on('click', '.js-move-up, .js-move-down', function() {
    RowSwitcher.switchRow(this);
    RowSwitcher.orderRow($(this).parents('tbody'));
})

$(gradingFormId).on('focusin', 'table tbody tr td input', function() {
    $(this).addClass('bg-secondary-lighter');
})

$(gradingFormId).on('focusout', 'table tbody tr td input', function() {
    $(this).removeClass('bg-secondary-lighter');
    if (this.value != '') {
        var value = parseFloat(this.value);
        $(this).val(value.toFixed(2)); 
    }
})

/* Scoring step event */
$(gradingFormId).on('keyup', scoringStudent, function(e) {
    if (e.keyCode === 13) {
        let currentInput = $(this).attr('name');
        let rowIndex = parseInt(currentInput.match(RegularExpressions.SingleDigitInBracket)[1]); // .match() return array of match data details (require array index to use)
        let desireInput = currentInput.replace(RegularExpressions.SingleDigitInBracket, `[${ rowIndex+1 }]`);
        $(`input[name="${ desireInput }"]`).focus().select();
    } else {
        let currentCell = $(this).parents('td');
        let currentScore = $(this).val();
        let limitScore = parseFloat($(this).data('limit'));
        if (currentScore > limitScore || isNaN(currentScore)) {
            currentCell.addClass('bg-danger-pastel');
        } else {
            currentCell.removeClass('bg-danger-pastel');
        }
        
        let currentRow = $(this).parents('tr');
        let scoreList = currentRow.find(scoringStudent);
        let totalScore = currentRow.find(scoringTotal);
        calculateTotalScore(scoreList, totalScore);

        var fullScore = 0;
        $(scoreList).each( function() {
            fullScore += parseFloat(this.dataset.limit);
        })
        let percent = currentRow.find(percentage);
        percent.val((parseFloat(totalScore.html()) * 100 / fullScore).toFixed(2));
    }
})

/* Grading step event */
// $(gradingFormId).on('keyup', gradeScores, function() {
//     if (parseFloat($(this).val()) > 100) 
//     {
//         $(this).val((100).toFixed(2));
//     }
//     gradeScoresKeyUp($(this));
// })

function gradeScoresKeyUp(obj, gradeChart, index) {
    let score = obj.val();
    let scoreType = obj.data('score');
    let currentRow = obj.parents('tr');

    gradePointSuggestion(score, scoreType, currentRow, gradeChart, index);
    isGradeRowCompleted(currentRow, gradeChart, index);
}

// $(gradingFormId).on('change', gradeTitle, function() {
//     let currentRow = $(this).parents('tr');
//     isGradeRowCompleted(currentRow);
// })

/* Confirm step event */
$(gradingFormId).on('click', '.js-save-edit, .js-start-edit, .js-cancel-edit, .js-restore-edit', function() {
    let currentRow = $(this).parents('tr');
    let currentButtonType = $(this).data('type');

    editGrade(currentRow, currentButtonType);
})

$(document).ready(function() {
    initWizard();
    calculateTotalScore(allocationScore, allocationTotalScore);
    checkTotalScore();

    EditTable.renderEditTable('.editable-table');

    // $('#js-submit-form').on('click', function() {
    //     $(this).parents('form').submit();
    // })

    $('#browse-btn').on('click', function() {
        $('#upload-input').click();
    });

    $('#upload-input').on('change', function(e) {
        $("#file-name").val(e.currentTarget.files[0].name);
    });
})