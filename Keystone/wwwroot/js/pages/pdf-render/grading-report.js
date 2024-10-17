var dataModel = $('#report-preview').data('model');
var page1 = {
    p1 : ["สรุปการพิจารณาผลการสอบภาคเรียนที่________________________ปีการศึกษา____________________________",
          "คณะกรรมการกำกับมาตรฐานวิชาการ______________________________________________________________",],
    p2 : ["ในคราวประชุมครั้งที่____________________วันที่_________________________พิจารณาแล้วเห็นว่ารายงานผลการสอบของ",
          "วิชา_______________________________________________________________________ที่อาจารย์ผู้สอนเสนอมา คือ"],
    p3 : ["ช่วงระดับคะแนน", "เกรด", "จำนวนนักศึกษา"],
    p4 : ["เหมาะสมแล้ว", "ให้แก้ไขเป็น"],
    p5 : ["ลงนาม", "____________________", "ประธานกรรมการ", "กรรมการ", "/เลขานุการ"],
    p6 : "________________________________________________"

};
var page2 = {
    p1 : ["Examination Result Report", "Grade Report"],
    p2 : ["Division:", "Program:", "Trimester:", "Course ID:", "Section(s):", "Lecturer(s):"],
    p3 : ["GRADE DISTRIBUTION", "STATISTICAL INFORMATION ", "CLASS STATISTIC"],
    p4 : ["Class Statistic", "Class GPA:", "Class Min:", "Class Mean:", "Class Max:", "Class SD:"],
    p5 : "Grading Range",
    p6 : ["GRADE HISTOGRAM", "Normal Curve"],
};
var page3 = "Mark Allocation";

var classStatistics = dataModel.Body.ClassStatistics;
// mock data area
var dataModel = {
    "Title":"Report",
    "Subject":"Grading Result Report",
    "Creator":"Keystone V.",
    "Author":"Kornkamol Kiatopas",
    "Body":{
        "BarcodeNumber":dataModel.Body.BarcodeNumber,
        "Faculty":dataModel.Body.Faculty,
        "Department":dataModel.Body.Department,
        "Semester":dataModel.Body.Semester,
        "CoursesString":dataModel.Body.Course,
        "SectionString":dataModel.Body.SectionString,
        "InstructorsString":dataModel.Body.InstructorsString,
        "ClassStatistics":{"GPA":classStatistics.GPA,"Min":classStatistics.Min,"Mean":classStatistics.Mean,"Max":classStatistics.Max,"SD":classStatistics.SD},
        "GradingRanges":dataModel.Body.GradingRanges,
        "GradingRangeTotal":{"Frequency":2,"TotalPercentage":100},
        "GradeNormalCurves":dataModel.Body.GradeNormalCurves,
        "GradingFrequencies":dataModel.Body.GradingFrequencies,
        "SectionAllocations":dataModel.Body.SectionAllocations,
        "MarkAllocaiton":dataModel.Body.MarkAllocaiton,
        "StudentScoreAllocations":dataModel.Body.StudentScoreAllocations,
        "SectionMarkAllocations":dataModel.Body.SectionMarkAllocations,
        "PrintedAt":dataModel.Body.PrintedAt,
        "AcademicYear":dataModel.Body.AcademicYear,
        "AcademicTerm":dataModel.Body.AcademicTerm
    }
}
// mock data area

function drawOverallScoreTable(repo, currentY, data) {
    let scoreTable = [];
    let currentX = 10;
    let tableHeader = ['Score', 'Freq.', '%'];

    $(data).each( function(index) {
        scoreTable.push([this.Score, this.Frequency, this.TotalPercentage.toFixed(2)])
            
        if (index%50 === 0 && index !== 0) {
            repo.autoTable({
                head: [tableHeader],
                body: scoreTable,
                styles: {
                    halign: 'center',
                    lineWidth: 0.1,
                    lineColor: defaultColor.gray,
                    fontSize: 8,
                    cellPadding: 0.1,
                },
                headStyles: {
                    fillColor: defaultColor.lightGray,
                    textColor: defaultColor.black,
                },
                margin: {left: currentX},
                startY: currentY,
                tableWidth: 42.5,
            });

            scoreTable = [];
            currentX += 45;
        }
    })
}

function drawGradeRangeTable(repo, currentY, data) {
    let scoreTable = [];
    let currentX = 105;
    let tableHeader = [{content: 'Range', colSpan: 3}, 'Grade', 'Freq.', 'Total%', 'Grade%', 'Cum%'];
    let totalFrequency = 0;
    let summary = 0;

    $(data).each( function(index) {
        scoreTable.push([
            {content: (this.MinRange != null) ? this.MinRange : "", styles: {halign: 'right', right: 0}}, 
            (this.MinRange != null) ? 'To' : "", 
            {content: (this.MaxRange != null && this.MinRange != null) ? this.MaxRange : "", styles: {halign: 'left', left: 0}}, 
            this.Grade, 
            this.Frequency, 
            this.TotalPercentage.toFixed(2), 
            this.GradePercentage.toFixed(2), 
            this.CumulativePercentage.toFixed(2)
        ]);
        scoreTable.push([]);

        totalFrequency += this.Frequency;
        summary += this.TotalPercentage;
        if (index === data.length-1) {
            scoreTable.push(['', '', '', 'Total', totalFrequency, summary.toFixed(2), '', '']);
            scoreTable.push([]);
        }
    })

    repo.autoTable({
        theme: 'plain',
        head: [tableHeader, []],
        body: scoreTable,
        styles: {
            halign: 'center',
            fontSize: 8,
            textColor: defaultColor.black,
            fillColor: defaultColor.white,
            cellPadding: 0.25,
        },
        margin: {left: currentX},
        startY: currentY,
        tableWidth: 95,
        didParseCell: function (data) {
            if (data.row.index%2 === 1) {
                data.cell.styles.fillColor = defaultColor.black;
                data.cell.styles.cellPadding = 0;
                data.cell.styles.fontSize = 0.75;
            } else if (data.row.index === scoreTable.length-2) {
                data.cell.styles.fontStyle = 'bold';
            }
            finalHeight = data.table.height;
        }
    });

    return repo.previousAutoTable.finalY;
}

function drawSectionTable(repo, currentY, data) {
    let sectionTable = [];
    let currentX = 10;

    $(data).each( function() {
        sectionTable.push([this.Section, this.Course, this.Credit]);
    })

    repo.autoTable({
        head: [['Section', 'Course', 'Credit']],
        body: sectionTable,
        styles: {
            halign: 'center',
            fontSize: 9,
            cellPadding: 0.5,
        },
        headStyles: {
            fillColor: defaultColor.lightGray,
            textColor: defaultColor.black,
        },
        margin: {left: currentX},
        startY: currentY + 2.5,
        tableWidth: 75,
    });
}

function drawAllocationTable(repo, currentY, data) {
    let allocationTable = [];
    let currentX = 120;

    $(data).each( function() {
        allocationTable.push([this.Type, this.Abbreviation, this.FullScore]);
    })

    repo.autoTable({
        head: [['Type', 'Abbr.', 'Score']],
        body: allocationTable,
        styles: {
            halign: 'center',
            fontSize: 9,
            cellPadding: 0.5,
        },
        headStyles: {
            fillColor: defaultColor.lightGray,
            textColor: defaultColor.black,
        },
        margin: {left: currentX},
        startY: currentY + 2.5,
        tableWidth: 75,
    });
}

function drawFullReportTable(repo, currentY, data, rowNumber) {
    let reportTable = [];
    let currentX = 10;
    let tableHeader = ['#', 'Section', 'Student ID', 'Name', 'Major', 'Grade', 'Round', 'Total']

    $(data[0].Allocations).each( function() {
        tableHeader.push(this.Abbreviation)
    })

    $(data).each( function(index) {
        reportTable.push([++rowNumber, `${ this.CourseCode } (${ this.Section })`, this.StudentCode, `    ${ this.FullName }`, this.Major, this.Grade, this.Round, this.AllocationTotal]);
        $(this.Allocations).each( function() {
            reportTable[index].push(this.FullScore)
        })
    })
    
    repo.autoTable({
        head: [tableHeader],
        body: reportTable,
        styles: {
            halign: 'center',
            lineWidth: 0.1,
            lineColor: defaultColor.gray,
            fontSize: 8,
            cellPadding: 0.5,
        },
        headStyles: {
            fillColor: defaultColor.lightGray,
            textColor: defaultColor.black,
        },
        columnStyles : {
            3: { halign: 'left'},
        },
        margin: {left: currentX},
        startY: currentY + 5
    });
}

function generateChartData(data) {
    let scoreList = [];
    $(data.StudentScoreAllocations).each( function() {
        scoreList.push(parseInt(this.AllocationTotal));
    });

    let studentScore = [];
    let scoreLabels = [];
    let bgColors = [];
    let maxScore = 100;
    var maxFrequency = Math.max.apply(Math, data.GradingFrequencies.map(function(x) { 
        return x.Frequency; 
    }));
    let maxStudent = data.StudentScoreAllocations.length;
    let maxFreRound = maxFrequency * 1.1;
    let maxFreMod = maxFreRound % 10;
    let maxStudentY = maxFreMod != 0 ? maxFreRound + (10 - maxFreMod) : maxFreRound;
    let step = (maxStudentY / 10) > 10 ? 10 
                                       : maxStudentY / 10 > 4 ? 5 : 1 ;
    // main score data
    for (i = 0; i <= maxScore; i++) {
        let counter = 0;
        scoreLabels.push(i);
        if (scoreList.includes(i)) {
            counter = scoreList.filter(score => score == i).length;
        }
        studentScore.push(counter);

        bgColors.push('rgba(0, 0, 0, 1)');
    }

    let chartTitle = "Grade Distribution",
        xTitle = "Score",
        yTitle = "Number of Students" + "(" + maxStudent + ")";
    let scoreData = {
        labels: scoreLabels, // score length (0 - max score)
        datasets: [{
            data: studentScore, // counter            
            yAxisID: "y-left",
            backgroundColor: defaultColor.lightGray,
            borderColor: defaultColor.gray,
            borderWidth: 0.2,
        }]
    }


    let chartConfig = {
        type: 'bar',
        data: scoreData,
        options: {
            barValueSpacing: 1,
            title: {
                display: true,
                text: chartTitle,
                fontSize: 20,
            },
            legend: { // display what is bar details
                display: false
            },
            scales: {
                xAxes: [{
                    barPercentage: 0.8,
                    scaleLabel: {
                        display: true,
                        labelString: xTitle,
                        fontSize : 20,
                    },
                    ticks: {                        
                        min: 0,
                        stepSize: 2,
                        minStepSize : 0,
                        fontSize : 15,
                    }
                }],
                yAxes: [{
                    id: "y-left",
                    position: "left",
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: yTitle,
                        fontSize : 20
                    },
                    ticks: {
                        max: maxStudentY, 
                        beginAtZero: true,
                        stepSize: step,
                        fontSize : 20
                    }
                }]
            },
            bezierCurve : false,
            animation: {
                onComplete: generateChartImage
            }
        }
    }

    return chartConfig;
}

function generateHistogramData(data) {
    let grades = [];
    let frequencies = [];
    let bgColors = [];
    var maxFrequency = Math.max.apply(Math, data.GradingRanges.map(function(x) { 
        return x.Frequency; 
    }));
    let maxStudent = data.StudentScoreAllocations.length;
    let maxFreRound = maxFrequency * 1.1;
    let maxFreMod = maxFreRound % 10;
    let maxStudentY = maxFreMod != 0 ? maxFreRound + (10 - maxFreMod) : maxFreRound;
    let step = (maxStudentY / 10) > 10 ? 10 
                                       : maxStudentY / 10 > 4 ? 5 : 1 ;

    $([...data.GradingRanges].reverse()).each( function() {
        grades.push(this.Grade);
        frequencies.push(parseInt(this.Frequency));
        bgColors.push('rgba(0, 0, 0, 1)');
    });

    let gradeData = {
        labels: grades, // score length (0 - max score)
        datasets: [{
            data: frequencies, // counter
            backgroundColor: defaultColor.lightGray,
            borderColor: defaultColor.gray,
            borderWidth: 0.2,
            yAxisID: "y-left"
        }]
    }

    let chartTitle = "",
        xTitle = "Grade",
        yTitle = "Number of Students(" + maxStudent + ")";

    let chartConfig = {
        type: 'bar',
        data: gradeData,
        options: {
            barValueSpacing: 1,
            title: {
                display: true,
                text: chartTitle,
                fontSize: 50,
            },
            legend: { // display what is bar details
                display: false
            },
            scales: {
                xAxes: [{
                    barPercentage: 0.8,
                    scaleLabel: {
                        display: true,
                        labelString: xTitle,
                        fontSize : 30
                    },
                    ticks: {
                        min: 0,
                        stepSize: 1,
                        minStepSize : 0,
                        fontSize : 30,
                    }
                }],
                yAxes: [{
                    id: "y-left",
                    position: "left",
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: yTitle,
                        fontSize : 30
                    },
                    ticks: {
                        max: maxStudentY, 
                        beginAtZero: true,
                        stepSize: step,
                        fontSize : 30
                    }
                }]
            },
            bezierCurve : false,
            animation: {
                onComplete: generateChartImage
            }
        }
    }

    return chartConfig;
}

function renderChartImage(data) {
    let chartCanvas = document.getElementById('js-temporary-canvas');
    let context = chartCanvas.getContext('2d')
    new Chart(context, generateChartData(data));

    Chart.plugins.register({
        afterRender: function(c) {
            var ctx = c.chart.ctx;
            ctx.save();

            // fill background behind the drawn graph
            ctx.globalCompositeOperation = 'destination-over';
            ctx.fillStyle = 'white';
            ctx.fillRect(0, 0, c.chart.width, c.chart.height);
            ctx.restore();
        }
    });
}

function renderHistogramImage(data) {
    let histogramCanvas = document.getElementById('js-histogram-canvas');
    let context = histogramCanvas.getContext('2d')
    new Chart(context, generateHistogramData(data));

    Chart.plugins.register({
        afterRender: function(c) {
            var ctx = c.chart.ctx;
            ctx.save();

            // fill background behind the drawn graph
            ctx.globalCompositeOperation = 'destination-over';
            ctx.fillStyle = 'white';
            ctx.fillRect(0, 0, c.chart.width, c.chart.height);
            ctx.restore();
        }
    });
}

function generateGradeReport(dataModel, chartUrl, histogramUrl) {
    var repo = new jsPDF('p', 'mm', 'a4');
    var paddingX = 10,
        paddingY = 10,
        centerX = 105,
        halfCenterX = (centerX - paddingX)/2,
        fromBottom = 297-paddingY;

    var informationConfig = {
        align: 'left',
        lineHeightFactor: defaultSetting.lineHeightSmall,
        maxWidth: centerX - 25
    }
  
    repo.setProperties({
        title: dataModel.Title,
        subject: dataModel.Subject,
        author: dataModel.Author,
        creator: dataModel.Creator
    });

    /* -------------------------------------------------- page 1 -------------------------------------------------- */
    let currentY = paddingY;
    // PdfTemplate.repoHeaderA4(repo, paddingX, currentY);
    
    PdfSettings.format(repo, textFormats.sansBoldN)
    let termYear = dataModel.Body.Semester.split('/');
    repo.setFontSize(16)
        .text(page1.p1, paddingX, currentY += 25, PdfSettings.style(textStyles.ReportLeft))
        .text(dataModel.Body.AcademicTerm, 95, currentY, PdfSettings.style(textStyles.ReportCenter))
        .text(dataModel.Body.AcademicYear, 160, currentY, PdfSettings.style(textStyles.ReportCenter))
        .text(dataModel.Body.Department, 130, currentY + 7.5, PdfSettings.style(textStyles.ReportCenter));

    PdfSettings.format(repo, textFormats.sansN)
    let meetingNumber = " ",
        meetingDate = " ";
    repo.setFontSize(16)
        .text(page1.p2,paddingX, currentY += 15, PdfSettings.style(textStyles.ReportLeft))
        .text(meetingNumber, 60, currentY, PdfSettings.style(textStyles.ReportCenter))
        .text(meetingDate, 110, currentY, PdfSettings.style(textStyles.ReportCenter))
        .text(dataModel.Body.CoursesString, 85, currentY + 7.5, PdfSettings.style(textStyles.ReportCenter))
        .text(`Section: ${ dataModel.Body.SectionString }`, paddingX, currentY += 15, PdfSettings.style(textStyles.ReportLeft))
        .text(page1.p3[0], 70, currentY += 7.5, PdfSettings.style(textStyles.ReportCenter))
        .text(page1.p3[1], centerX, currentY, PdfSettings.style(textStyles.ReportCenter));
    let = false;
    $(dataModel.Body.GradingRanges).each( function() {
        if (this.IsCalculated)
        {
            repo.text(this.Range, 70, currentY += 7.5, PdfSettings.style(textStyles.ReportCenter));
            repo.text(this.Grade, centerX, currentY, PdfSettings.style(textStyles.ReportCenter));
            repo.text(this.Frequency.toString(), 140, currentY, PdfSettings.style(textStyles.ReportCenter));
        }
        else
        {
            flag = true;
        }
    })
    if (flag)
    {
        repo.text(page1.p6, 55, currentY += 5, PdfSettings.style(textStyles.SignatureLeft));
    }
    $(dataModel.Body.GradingRanges).each( function() {
        if (!this.IsCalculated)
        {
            repo.text(this.Range, 70, currentY += 7.5, PdfSettings.style(textStyles.ReportCenter));
            repo.text(this.Grade, centerX, currentY, PdfSettings.style(textStyles.ReportCenter));
            repo.text(this.Frequency.toString(), 140, currentY, PdfSettings.style(textStyles.ReportCenter));
        }
    })
    repo.setDrawColor(defaultColor.black)
        .rect(70, currentY += 7.5, 2.5, 2.5, 'S')
        .rect(120, currentY, 2.5, 2.5, 'S')
        .text(page1.p4[0], 75, currentY += 2.5, PdfSettings.style(textStyles.ReportLeft))
        .text(page1.p4[1], 125, currentY, PdfSettings.style(textStyles.ReportLeft));

    // repo.text([`${ page1.p5[0] } ${ page1.p5[1] } ${ page1.p5[2] }`,
    //            `${ page1.p5[0] } ${ page1.p5[1] } ${ page1.p5[3] }`,
    //            `${ page1.p5[0] } ${ page1.p5[1] } ${ page1.p5[3] }`,
    //            `${ page1.p5[0] } ${ page1.p5[1] } ${ page1.p5[3] }`,
    //            `${ page1.p5[0] } ${ page1.p5[1] } ${ page1.p5[3] }${ page1.p5[4]}`], 100, fromBottom-35, PdfSettings.style(textStyles.SignatureLeft));

    /* -------------------------------------------------- page 2 -------------------------------------------------- */
    repo.addPage('a4', 'p');
    currentY = paddingY;

    PdfSettings.format(repo, textFormats.sansBoldS)
    repo.text(page2.p1[0], centerX + 90, currentY += 15, PdfSettings.style(textStyles.ReportRight))
        .text(page2.p1[1], centerX - halfCenterX, currentY += 10, PdfSettings.style(textStyles.ReportCenter))

    let cardMaxWidth = 30;
        
    let barcodeAreaWidth = 30
        barcodeAreaHeight = 30
        backPadding = 1.5
        posX = cardMaxWidth - backPadding - barcodeAreaWidth
        posY = backPadding;

    let barcodeSrc = Barcode.getBarcodePath(dataModel.Body.BarcodeNumber.replace("-", ""), { 
        format: barcodeFormat.code128A,
        height: 50});

    repo.addImage(barcodeSrc, 'JPEG', 
                  posX + 10, 
                  posY + 10, 
                  100, 14, '', 'FAST', 0);

    // grade table
    drawOverallScoreTable(repo, currentY += 1, dataModel.Body.GradingFrequencies);

    // grade infomation
    PdfSettings.format(repo, textFormats.sansS);
    let isAddLine = repo.getTextWidth(dataModel.Body.CoursesString) > 74;
    repo.text([` ${ page2.p2[0] }\n`,
               `${ page2.p2[1] }\n`,
               `${ page2.p2[2] }\n`,
               `${ page2.p2[3] }\n ${ isAddLine ? "\n" : "" }`,
               `${ page2.p2[4] }\n`,
               `${ page2.p2[5] }\n`], centerX - 1, currentY, informationConfig);

    PdfSettings.format(repo, textFormats.sansBoldS);
    repo.text([` ${ dataModel.Body.Faculty } \n`,
               `${ dataModel.Body.Department } \n`,
               `${ dataModel.Body.Semester } \n`,
               `${ dataModel.Body.CoursesString } \n`,
               `${ dataModel.Body.SectionString } \n`,
               `${ dataModel.Body.InstructorsString } \n`], centerX + 20, currentY, informationConfig);

    repo.setFillColor(defaultColor.lightGray)
        .setTextColor(defaultColor.black)
        .rect(centerX, currentY += 35, centerX - paddingX, 6, 'F')
        .text(page2.p3[2], centerX + halfCenterX, currentY += 4, PdfSettings.style(textStyles.ReportCenter));

    PdfSettings.format(repo, textFormats.sansBoldS);
    PdfSettings.format(repo, textFormats.sansS);
    repo.text(` ${ page2.p4[1] }`, centerX + halfCenterX - 40, currentY += 7, informationConfig)
        .text(` ${ page2.p4[2] }`, centerX + halfCenterX - 40, currentY += 5, informationConfig)
        .text(` ${ page2.p4[3] }`, centerX + halfCenterX - 40, currentY += 5, informationConfig);

    currentY = 82;
    repo.text(` ${ page2.p4[4] }`, centerX + halfCenterX + 10, currentY += 5, informationConfig)
        .text(` ${ page2.p4[5] }`, centerX + halfCenterX + 10, currentY += 5, informationConfig);

    currentY = 77;
    repo.text(dataModel.Body.ClassStatistics.GPA.toString(), centerX + halfCenterX - 20, currentY += 5, informationConfig)
        .text(dataModel.Body.ClassStatistics.Min.toFixed(2).toString(), centerX + halfCenterX - 20, currentY += 5, informationConfig)
        .text(dataModel.Body.ClassStatistics.Mean.toFixed(2).toString(), centerX + halfCenterX - 20, currentY += 5, informationConfig);
    
    currentY = 82;
    repo.text(dataModel.Body.ClassStatistics.Max.toFixed(2).toString(), centerX + halfCenterX + 30, currentY += 5, informationConfig)
        .text(dataModel.Body.ClassStatistics.SD.toFixed(2).toString(), centerX + halfCenterX + 30, currentY += 5, informationConfig);

    currentY -= 5;

    // grading range
    PdfSettings.format(repo, textFormats.sansBoldS);
    repo.setDrawColor(defaultColor.black)
        .line(centerX, currentY + 14, centerX * 2-paddingX, currentY + 14)
        .text(page2.p5, centerX, currentY += 18, PdfSettings.style(textStyles.ReportLeft))
        .line(centerX, currentY + 1, centerX * 2-paddingX, currentY + 1);
    let gradeRangeTable = drawGradeRangeTable(repo, currentY += 2, dataModel.Body.GradingRanges);
    currentY = gradeRangeTable;

    // confirm
    repo.setFillColor(defaultColor.lightGray)
        .setTextColor(defaultColor.black)
        .rect(centerX, currentY += 2, centerX - paddingX, 6, 'F')
        .text(page2.p6[0], centerX + halfCenterX, currentY += 4, PdfSettings.style(textStyles.ReportCenter));

    PdfSettings.format(repo, textFormats.sansS)
    repo.text(page2.p6[1], centerX + halfCenterX, currentY += 5, PdfSettings.style(textStyles.SignatureCenter))
        .addImage(histogramUrl, 'jpeg', centerX, currentY += 1, 100, 50);

    // grade chart
    currentY = fromBottom - 60;
    repo.addImage(chartUrl, 'jpeg', paddingX, currentY, defaultSetting.A4MaxWidth, 60);

    /* -------------------------------------------------- page 3 -------------------------------------------------- */
    if (dataModel.Body.SectionMarkAllocations.length != 0)
    {
        $(dataModel.Body.SectionMarkAllocations).each( function() {
            var sizeYforDrawTable = 0;
            if (this.SectionAllocations.length > 2 || this.MarkAllocaiton.length >2)
            {
                if (this.SectionAllocations.length > this.MarkAllocaiton.length)
                {
                    sizeYforDrawTable = this.SectionAllocations.length - 2;
                }
                else {
                    sizeYforDrawTable = this.MarkAllocaiton.length - 2;
                }
            }

            var tableRow = [];
            var downRowSize = this.MarkAllocaiton.length > this.SectionAllocations.length ? this.MarkAllocaiton.length - 1 : this.SectionAllocations.length - 1;
            const rowSize = 40 - downRowSize;
            for (let i = 0; i < this.StudentScoreAllocations.length; i += rowSize) {
                tableRow.push(this.StudentScoreAllocations.slice(i, i + rowSize));
            }

            for (var i = 0; i < tableRow.length; i++) {
                repo.addPage('a4', 'p');
                currentY = paddingY;
                if (i == 0) {
                    PdfSettings.format(repo, textFormats.sansBoldHeader);
                    repo.text(dataModel.Body.CoursesString, paddingX, currentY += 20, PdfSettings.style(textStyles.ReportLeft));
            
                    PdfSettings.format(repo, textFormats.sansBoldN);
                    repo.setFontSize(16)
                        .text("Instructor : ", paddingX, currentY += 5, PdfSettings.style(textStyles.ReportLeft))
                        .setFontSize(14)
                        .text(this.InstructorFullNameEns, paddingX + 22, currentY, PdfSettings.style(textStyles.ReportLeft))
                        .setFontSize(16)
                        .text("Sections", paddingX, currentY += 5, PdfSettings.style(textStyles.ReportLeft))
                        .text(page3, paddingX + 110, currentY, PdfSettings.style(textStyles.ReportLeft));
                        
                    if(this.MarkAllocaiton.length > this.SectionAllocations.length)
                    {
                        drawSectionTable(repo, currentY, this.SectionAllocations);
                        drawAllocationTable(repo, currentY, this.MarkAllocaiton);
                    }
                    else
                    {
                        drawAllocationTable(repo, currentY, this.MarkAllocaiton);
                        drawSectionTable(repo, currentY, this.SectionAllocations);
                    }
                }

                if (i == 0) {
                    drawFullReportTable(repo, repo.previousAutoTable.finalY, tableRow[i], i);
                } else {
                    drawFullReportTable(repo, currentY, tableRow[i], i * rowSize);
                }
                
                currentY = fromBottom - 55;
                repo.setDrawColor(0,162,232)
                    .setDrawColor(0,0,0)
                    .setLineWidth(0.1)
                    .line(115, currentY + 33, 185, currentY + 33)

                repo.setFontSize(14)
                    .text(`Date: ` + dataModel.Body.PrintedAt, centerX + 45, currentY += 40, PdfSettings.style(textStyles.CertificateCenter))
                    .setFontSize(16)
                    .text(`Lecturer`, centerX + 46, currentY += 7, PdfSettings.style(textStyles.CertificateCenter))
            }
        });
    } else {
        repo.addPage('a4', 'p');
        currentY = paddingY;
        var sizeYforDrawTable = 0;
        if (dataModel.Body.SectionAllocations.length > 2 || dataModel.Body.MarkAllocaiton.length >2)
        {
            if (dataModel.Body.SectionAllocations.length > dataModel.Body.MarkAllocaiton.length)
            {
                sizeYforDrawTable = dataModel.Body.SectionAllocations.length - 2;
            }
            else {
                sizeYforDrawTable = dataModel.Body.MarkAllocaiton.length - 2;
            }
        }
        PdfSettings.format(repo, textFormats.sansBoldHeader);
        repo.text(dataModel.Body.CoursesString, paddingX, currentY += 20, PdfSettings.style(textStyles.ReportLeft));

        PdfSettings.format(repo, textFormats.sansBoldN);
        repo.setFontSize(16)
            .text(page3, paddingX, currentY += 5, PdfSettings.style(textStyles.ReportLeft));

        drawSectionTable(repo, currentY, dataModel.Body.SectionAllocations);
        drawAllocationTable(repo, currentY, dataModel.Body.MarkAllocaiton);
        drawFullReportTable(repo, repo.previousAutoTable.finalY + 2, dataModel.Body.StudentScoreAllocations);

        currentY = fromBottom - 55;
        repo.setDrawColor(0,162,232)
            .setDrawColor(0,0,0)
            .setLineWidth(0.1)
            .line(20, currentY, 95, currentY)
            .line(115, currentY, 185, currentY)
            .line(70, currentY + 33, 145, currentY + 33)

        repo.setFontSize(14)
            .text(`Date: ...........................................`, paddingX + halfCenterX, currentY += 10, PdfSettings.style(textStyles.CertificateCenter))
            .text(`Date: ...........................................`, centerX + halfCenterX, currentY, PdfSettings.style(textStyles.CertificateCenter))
            .setFontSize(16)
            .text(`Lecturer`, paddingX + halfCenterX, currentY += 7, PdfSettings.style(textStyles.CertificateCenter))
            .text(`Division Chair`, centerX + halfCenterX, currentY, PdfSettings.style(textStyles.CertificateCenter))
            .setFontSize(14)
            .text(`Date: ...........................................`, centerX, currentY += 25, PdfSettings.style(textStyles.CertificateCenter))
            .setFontSize(16)
            .text(`Associate Dean for Academic Affairs and Research`, centerX, currentY += 7, PdfSettings.style(textStyles.CertificateCenter))
    }
    $('#js-print-preview').attr('src', repo.output('datauristring'));
};

function generateChartImage() {
    let chartUrl = document.getElementById('js-temporary-canvas').toDataURL("image/jpeg", 1.0);
    let histogramUrl = document.getElementById('js-histogram-canvas').toDataURL("image/jpeg", 1.0);
    generateGradeReport(dataModel, chartUrl, histogramUrl);
}

$(function() {
    renderChartImage(dataModel.Body);
    renderHistogramImage(dataModel.Body);
})