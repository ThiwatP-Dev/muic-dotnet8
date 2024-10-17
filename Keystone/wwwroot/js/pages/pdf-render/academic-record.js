var dataModel = $('#js-pdf-preview').data('model');
var text = {
    title : ["MAHIDOL UNIVERSITY", "GRADE REPORT"],
    header : ["ADM. NO.", "NAME:", "DIVISION:", "MAJOR:", "CURRICULUM VERSION:", "MINOR:", "CONCENTRATION:",
              "GRADUATION:", "DATE OF GRADUATION:", "GRADUATED CLASS:", "GRADUATED TERM:", "HONOR:", "TR:"]
}

function drawAcadmeicRecordTable(report, currentY, data, codeAndName, type) {
    let examTable = [],
        index = 1,
        spXL = "         ",
        spL = "      ",
        spM = "    ";
        spS = "  ";
    let headerTable = type == "t"  ? [{content: `Tranfer`, colSpan: 2}, {content: `${ codeAndName }`, colSpan: 4}] : 
                      type == "tg" ? [{content: `Tranfer With Grade`, colSpan: 2}, {content: `${ codeAndName }`, colSpan: 4}]
                                   : [{content: `Year/Semester ${ data.AcademicYear }/${ data.AcademicTerm }`, colSpan: 2}, {content: `${ codeAndName }`, colSpan: 4}];
    let columnName = type == "t" ? ["No.", "Course Code", "Course Name", "", " Credit", "Grade"]
                                 : ["No.", "Course Code", "Course Name", spXL + "Section", " Credit", "Grade"];
    $(data.TranscriptCourses).each( function() {
        examTable.push([index, spS + this.CourseCode, "  " + this.CourseNames.join(' '), this.Section, this.Credit, spM + this.Grade]);
        ++index;
    })

    let semesterSummary = type == "t"  ? "TRANSFERRED " + data.TotalCreditString + " Credit" :
                          type == "tg" ? `TRANSFERRED:${ spL }${ additionZero(data.TotalCreditString, 2) }${ spS }CR.${ spL }${ additionZeroDecimal(data.GPTSString, 2) }${ spS }GPTS.${ spL }${ data.TermGPATranscriptString }${ spS }GPA.${ spL }CUMULATIVE ${ additionZero(0, 3) }${ spS }CR.${ spL }${ additionZeroDecimal(data.CumulativeGPTSString, 3) }${ spS }GPTS.${ spL }${ data.CumulativeGPAString }${ spS }GPA.${ spL }${ additionZero(data.CumulativeCreditCompString, 3) }${ spS }CR. COMPL.`
                                       : `SEMESTER:${ spL }${ additionZero(data.TotalCreditString, 2) }${ spS }CR.${ spL }${ additionZeroDecimal(data.GPTSString, 2) }${ spS }GPTS.${ spL }${ data.TermGPATranscriptString }${ spS }GPA.${ spL }CUMULATIVE ${ additionZero(data.CumulativeCreditRegisString, 3) }${ spS }CR.${ spL }${ additionZeroDecimal(data.CumulativeGPTSString, 3) }${ spS }GPTS.${ spL }${ data.CumulativeGPAString }${ spS }GPA.${ spL }${ additionZero(data.CumulativeCreditCompString, 3) }${ spS }CR. COMPL.`
                          ;
    examTable.push([{content: semesterSummary, colSpan: 6}]);
    
    report.autoTable({
        head: [headerTable, columnName],
        body: examTable,
        styles: {
            halign: 'center',
            font: 'sarabun',
            fontSize: 12,
            textColor: defaultColor.black,
            lineWidth: 0,
            cellPadding: 0
        },
        headStyles: {
            halign: 'left',
            fontStyle: 'bold',
            fillColor: defaultColor.lightGray,
            cellPadding: {top: 0, bottom: 0.25},
        },
        columnStyles : { 
            0: { cellPadding: { top: 0.1, left: 2, bottom: 0.25 }, halign: 'left', cellWidth: 10 },
            1: { halign: 'left', cellWidth: 35 },
            2: { halign: 'left'},
            3: { cellWidth: 30 },
            4: { cellWidth: 15 },
            5: { halign: 'left', cellWidth: 12 },
        },
        margin: {left: defaultSetting.paddingX},
        startY: currentY,
        tableWidth: defaultSetting.A4MaxWidth,
        didParseCell: function (data) {
            if (data.row.index === index-1) {
                data.cell.styles.fontStyle = 'bold';
                data.cell.styles.halign = 'left';
                data.cell.styles.cellPadding = {top: 0, bottom: 0};
                data.cell.styles.fillColor = defaultColor.lightGray;
            }
        }
    })

    return report.previousAutoTable.finalY;
}

function academicRecordReport(dataModel) {
    var report = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(report, dataModel);

    let fullWidth = defaultSetting.A4Width,
        paddingX = defaultSetting.paddingX,
        centerX = defaultSetting.centerX,
        lineHeight = 5.5,
        imageSize = [25, 25];

    $(dataModel.Body).each( function(index) {
        
        // data set + page management
        let currentY = defaultSetting.paddingY;
        if (index >= 1) {
            let currentPage = report.internal.getCurrentPageInfo().pageNumber
            report.addPage().setPage(currentPage + 1);
        }

        let data = this;
        let transcriptData = data.TranscriptTerms;
        let transfer = data.Transfer;
        let transferWithGrade = data.TransferWithCourse;
        // draw header
        PdfSettings.format(report, textFormats.serifBoldHeader)
        report.text(text.title[0], centerX, currentY += lineHeight, PdfSettings.style(textStyles.ReportCenter))
              .text(text.title[1], centerX, currentY += defaultSetting.SmallLine, PdfSettings.style(textStyles.ReportCenter));

        // header -> picture & adm. no.
        PdfSettings.format(report, textFormats.serifBoldN)
        report.setFontSize(16)
              .text(`${ text.header[0] } ${ data.StudentCode }`, fullWidth - paddingX, currentY, PdfSettings.style(textStyles.ReportRight));

        let imageLeft = (fullWidth - paddingX) - imageSize[0];
        var imgHeight = $('#js-default-student')[0].height;
        var imageWidth = $('#js-default-student')[0].width;
        var imgRatio = imgHeight / imageWidth
        var imgRatioWidth = imageSize[1] * (1 / imgRatio)
        report.addImage($('#js-default-student')[0], 'png', imageLeft, currentY + 2.5, imgRatioWidth, imageSize[1]); // will use picture from model later

        // header => student's info
        PdfSettings.format(report, textFormats.serifN)
        report.setFontSize(16)
              .text(`${ text.header[1] } ${ data.FirstName.toUpperCase() } ${ data.LastName.toUpperCase() }`, paddingX, currentY += lineHeight, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[2] } ${ data.Faculty.toUpperCase() }`, paddingX, currentY += lineHeight, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[3] } ${ data.Department.toUpperCase() }`, paddingX, currentY += lineHeight, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[4] } ${ data.CurriculumVersion === null ? "---" : data.CurriculumVersion.toUpperCase() }`, paddingX, currentY  += lineHeight, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[5] } ${ data.Minor === null ? "---" : data.Minor.toUpperCase() }`, paddingX, currentY += lineHeight, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[6] } ${ data.Concentration === null ? "---" : data.Concentration.toUpperCase() }`, paddingX, currentY  += lineHeight, PdfSettings.style(textStyles.ReportLeft));

        let triCol = (fullWidth - paddingX) / 3;
        report.text(`${ text.header[7] } ${ data.IsGraduated ? "YES" : "NO" }`, paddingX, currentY += lineHeight, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[8] } ${ data.GraduatedAt === null ? "---" : data.GraduatedAt }`, triCol, currentY, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[9] } ${ data.GraduateClass === null ? "---" : data.GraduateClass }`, paddingX + triCol * 2, currentY, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[10] } ${ data.GraduateTerm === null ? "---" : data.GraduateTerm }`, paddingX, currentY += lineHeight, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[11] } ${ data.Award === null || data.Award === "" ? "---" : data.Award.toUpperCase() }`, triCol, currentY, PdfSettings.style(textStyles.ReportLeft))
              .text(`${ text.header[12] } ${ data.TotalCreditTransferred === null ? "---" : data.TotalCreditTransferred } CR.`, paddingX + triCol * 2, currentY, PdfSettings.style(textStyles.ReportLeft));
        
        currentY += 2.5;
        // draw tables
        $(transfer).each( function() {
            currentY = drawAcadmeicRecordTable(report, currentY, this, `${ data.StudentCode } ${ data.FirstName } ${ data.LastName }`, "t");
            currentY += 2;
        })
        $(transferWithGrade).each( function() {
            currentY = drawAcadmeicRecordTable(report, currentY, this, `${ data.StudentCode } ${ data.FirstName } ${ data.LastName }`, "tg");
            currentY += 2;
        })
        $(transcriptData).each( function() {
            currentY = drawAcadmeicRecordTable(report, currentY, this, `${ data.StudentCode } ${ data.FirstName } ${ data.LastName }`);
            currentY += 2;
        })
    })

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(function() {
    academicRecordReport(dataModel);
})

function additionZero (numberData, limitLength) {
    let stringNumber = numberData.toString();
    while (stringNumber.length < limitLength) {
        stringNumber = "  " + stringNumber;
    }

    return stringNumber;
}

function additionZeroDecimal (numberData, limitLength) {
    let numberString = numberData.split('.');
    let stringNumber = numberString[0];
    while (stringNumber.length < limitLength) {
        stringNumber = " " + stringNumber;
    }

    return `${ stringNumber }.${ numberString[1] }`;
}