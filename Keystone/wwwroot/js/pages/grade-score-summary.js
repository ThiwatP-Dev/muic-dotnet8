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
var curveScoreTable = ".js-curve-table";

var scoreList;
var percentageList;
var originalLineDraws = new Array(2);
var gradeCharts = new Array(2);
var dataModel = $('#grade-score-data').data('model');


function gradeChartRenderer(jsCanvasId, index) {

    gradeRange = [];
    originalLineDraws[index] = Chart.controllers.bar.prototype.draw;
    
    let studentScore = [];
    let scoreLabels = [];
    let bgColors = [];
    let lineColors = [];
    let maxFrequency = Math.max.apply(Math, dataModel.GradingFrequencies.map(function(x) { 
                                                return x.Frequency; 
                                            }));
    let maxFreRound = maxFrequency * 1.1;
    let maxFreMod = maxFreRound % 10;
    let maxStudentY = maxFreMod != 0 ? maxFreRound + (10 - maxFreMod) : maxFreRound;
    let step = (maxStudentY / 10) > 10 ? 10 
                                       : maxStudentY / 10 > 4 ? 5 : 1 ;
    
    percentageList = [];
    var checkIsCalc = jsCanvasId == '#grade-chart' ? true : false;
    $(dataModel.StudentScoreAllocations).each( function() {
        if(this.IsCalcGrade == checkIsCalc)
        {
            percentageList.push(parseFloat(this.TotalScore));
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
    
        bgColors.push('rgba(44, 48, 77, 0.7)');
        lineColors.push('rgba(44, 48, 77, 1)');
    }
    
    var scoreData = {
        labels: scoreLabels, // score length (0 - max score)
        toolTip:{
            enabled: false,       //disable here
            animationEnabled: false //disable here
          },
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
    
    var chartArea = $(jsCanvasId);
    let chartTitle = $(chartArea).data('chart-title'),
        xTitle = $(chartArea).data('axis-x-title'),
        yTitle = $(chartArea).data('axis-y-title');

    gradeCharts[index] = new Chart(chartArea, {
        type: 'bar',
        data: scoreData,
        options: {
            responsive: true,
            barValueSpacing: 1,
            events: [],
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
                        max: maxStudentY, 
                        beginAtZero: true,
                        stepSize: step
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
                        max: maxStudentY, 
                        beginAtZero: true,
                        stepSize: step
                    }
                }]
            }
        }
    });
    var ctx = gradeCharts[index].chart.ctx;
    drawGrade(ctx, index, gradeCharts[index], jsCanvasId)
}

function drawGrade(canvas, index, gradeChart, jsCanvasId) {
    var dataGradeRanges = jsCanvasId == '#grade-chart' ? dataModel.GradingRangesCalc : dataModel.GradingRangesNotCalc;
    var i = 0;
    Chart.helpers.extend(Chart.controllers.bar.prototype, {
        draw: function () {
            canvas.save();
                $(dataGradeRanges).each( function(i, item) {
                    var fillColor = getGradeColor(item.Grade, 0.1),
                        lineColor = getGradeColor(item.Grade, 0.8);
                    var minScore = item.MinRange == 0 ? -0.5 : item.MinRange,
                    maxScore = item.MaxRange,    
                    xAxis = gradeChart.scales['x-axis-0'],
                    yAxis = gradeChart.scales['y-left'];
                    
                    var x1 = xAxis.getPixelForValue(minScore - 0.5),
                    x2 = xAxis.getPixelForValue(maxScore + 0.5),
                    y1 = yAxis.bottom,
                    y2 = yAxis.top;
                    canvas.beginPath();
                    canvas.moveTo(x1, y1);
                    canvas.strokeStyle = lineColor;
                    canvas.fillStyle = fillColor;
                    canvas.lineTo(x1, y2);
                    canvas.lineTo(x2, y2);
                    canvas.lineTo(x2, y1);
                    canvas.closePath();
                    canvas.fill();
                    canvas.stroke();

                    canvas.beginPath();
                    canvas.textAlign = 'center';
                    canvas.font = "15px Georgia";
                    canvas.fillStyle = lineColor;
                    canvas.fillText(`${ item.Grade }`, x1 + ((x2 - x1) / 2), y2 - 10);
                    canvas.closePath();
                })
                canvas.restore();
                originalLineDraws[index].apply(this, arguments);
                return;
        }
    });
}

$(document).ready(function() {
    if(dataModel.GradingCurves.length != 0 && dataModel.IsCalGradeExist == true)
    {
        gradeChartRenderer('#grade-chart', 0);
    }
    if(dataModel.GradingCurvesNotCalc.length != 0 && dataModel.IsNotCalGradeExist == true)
    {
        gradeChartRenderer('#grade-chart-2', 1);
    }
});

$(curveScoreTable).on('keyup', '.curve-min-score', function () {
    var nextMaxScoreValue = $(this).val() - 1;
    var currentRow = $(this).closest('tr');
    var nextRow = currentRow.next('tr');
    var nextRowAll = currentRow.nextAll('tr');
    var nextTwoRowAll = $(this).closest('tr').next('tr').nextAll('tr');
    if ($(this).val()) {
        currentRowMax = currentRow.find('.curve-max-score-val').val();
        currentRow.find('.curve-min-score').attr({ 'max' : currentRowMax - 1 });
        nextRow.find('.curve-max-score').html(nextMaxScoreValue);
        nextRow.find('.curve-max-score-val').val(nextMaxScoreValue);
        nextRow.find('.curve-min-score').prop('disabled', false);
        nextRow.find('.curve-min-score').attr({ 'max' : nextMaxScoreValue - 1 });
    }

    nextTwoRowAll.find('.curve-min-score').prop('disabled', true);
    nextTwoRowAll.find('.curve-max-score').html('');
    nextRowAll.find('.curve-min-score').val('');
});

$(".btn-recalc").on('click', function () {
    $("#IsSave").prop('checked', false);
    $("#form-edit-grade-curve").submit();
});

$(".btn-next").on('click', function () {
    $("#form-edit-grade-curve").submit();
});