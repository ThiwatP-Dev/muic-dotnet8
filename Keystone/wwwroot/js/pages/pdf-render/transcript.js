var dataModel = $('#js-pdf-preview').data('model');
var universityNameEn = $('body').data('uname-en');
var universityNameTh = $('body').data('uname-th');
var textEn = {
    title1 : ["NAME", "ID", "BIRTHDATE", "NATIONALITY", "MAJOR"],
    title2 : ["REGISTRAR", "DATE", "NOT VALID WITHOUT UNIVERSITY SEAL"],
    title3 : ["NAME", "ID NO."],
    rowDat : ["TRIMESTER-GPA", "CUM-GPA", "TOTAL CREDITS EARNED", "TOTAL CREDITS AUDITED", "TOTAL CREDITS EXEMPTED",
              "TOTAL CREDITS TRANSFERRED", "TOTAL FOUNDATION COURSE CREDITS", "TOTAL INDEPENDENT STUDY COMPLETED",
              "TOTAL PH.D DISSERTATION COMPLETED", "TOTAL THESIS COMPLETED", "TOTAL CREDITS REGISTERED", 
              "ENTRY BELOW THIS LINE IS INVALID", "COURSE REQUIREMENTS FULLFILLED", "CREDITS TRANSFERRED",
              "TRANSFERRED FROM GENERAL EDUCATION", "DEPARTMENT, YEAR", "", "THESIS TITLE", "DISERTATION TITLE"],
    footer : ["TRANSCRIPT GUIDE PRINTED ON REVERSE", "DATE ISSUED:", "CERTIFIED TRUE COPY", "NOT VALID WITHOUT SEAL", "UNIVERSITY REGISTRAR"],
    dashLine : ["----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"]
};
var textTh = {
    title1 : ["รหัสประจำตัว", "ชื่อ", "นามสกุล", "วันที่เกิด", "วันเข้าศึกษา", "ระดับปริญญา"],
    title2 : ["สาขาวิชา", "กลุ่มวิชาเอกเลือก", "กลุ่มวิชาโทเลือก", "วันที่จบการศึกษา", "ปริญญา", ""],
    column : ["รหัสวิชา", "ชื่อวิชา", "หน่วยกิต", "เกรด",],
    rowDat : ["ภาคเรียน", "สะสม    ", "หน่วยกิตสะสมรวม", "หน่วยกิตวิชาร่วมฟังรวม", "หน่วยกิตได้รับการยกเว้นรวม",
              "หน่วยกิตเทียบโอนรวม", "หน่วยกิตวิชาพื้นฐานรวม", "สารนิพนธ์เสร็จสมบูรณ์รวม",
              "วิทยานิพนธ์เสร็จสมบูรณ์รวม", "วิทยานิพนธ์เสร็จสมบูรณ์รวม", "หน่วยกิตลงทะเบียนรวม", 
              "ข้อมูลใต้เส้นไม่มีผลบังคับใช้", "ข้อมูลใต้เส้นไม่มีผลบังคับใช้", "หน่วยกิตเทียบโอน",
              "เทียบโอนจาก หมวดวิชาศึกษาทั่วไป", "ปี", "หน่วย", "THESIS TITLE", "DISERTATION TITLE"],
    footer : ["คำอธิบายการวัดผลการศึกษาแสดงด้านหลัง", "ออกให้ ณ วันที่", " ", "เอกสารจะไม่สมบูรณ์หากไม่มีตราประทับของมหาวิทยาลัย", "นายทะเบียน"]
};

function switchFormat(pdf, format, language) {
    if (language === "en") {
        switch (format) {
            case "bold":
                PdfSettings.format(pdf, textFormats.sansBoldXSEn)
                break;
    
            case "normal":
                PdfSettings.format(pdf, textFormats.sansXSEn)
                break;
        }
    } else if (language === "th") {
        switch (format) {
            case "bold":
                PdfSettings.format(pdf, textFormats.sansBoldXS)
                break;
    
            case "normal":
                PdfSettings.format(pdf, textFormats.sansXS)
                break;
        }
    }
}

function isReachLimit(currentIndex, fullTable, transcriptTable, lineLimit) {

    if (currentIndex > lineLimit) {
        fullTable.push(transcriptTable);
        transcriptTable = [];
    }

    return [transcriptTable, fullTable];
}

function additionZero (numberData, limitLength) {
    let stringNumber = numberData.toString();
    while (stringNumber.length < limitLength) {
        stringNumber = "0" + stringNumber;
    }

    return stringNumber;
}

function drawTranscriptTable(transcript, data, text, language) {
    let transcriptData = data.TranscriptTerms;
    let transcriptTable = [];
    let fullTable = [];
    let currentX = 25;
    let pageCount = 1;
    let totalCreditTransfer = 0;

    var checkLength;
    switch (language) {
        case "en":
            var cellFont = 'arial',
                cellFontSize = 6.5,
                creditColumeWidth = 4,
                lineLimit = 52,
                dashLine = "END OF TRANSCRIPT";
            break;

        case "th":
            var cellFont = 'sarabun',
                cellFontSize = 11,
                creditColumeWidth = 10,
                lineLimit = 55,
                dashLine = "--------------------------------------------------------------------------------";
            break;
    }

    if (data.TransferedUniversity != null) {
        transcriptTable.push([{content: `TRANSFER FROM ${ data.TransferedUniversity.toUpperCase() }`, colSpan: 4}]);
    }

    $(transcriptData).each( function() {
        $(this.TranscriptCourses).each( function() {
            if (this.Grade == "TR") {
                transcriptTable.push([` ${ this.CourseCode }`, this.CourseNames[0], this.Credit, this.Grade]);
                totalCreditTransfer += this.Credit;
            }
        })
    });

    if (totalCreditTransfer != 0) {
        transcriptTable.push([{content: `TOTAL CREDIT TRANSFER`, colSpan: 2}, {content: `   ${ totalCreditTransfer }   ${ text.rowDat[16] }`}]);

        if (transcriptTable.length != 0) {
            transcriptTable.push([]);
        } 
    }

    $(transcriptData).each( function() {
        let termCredit = additionZero(this.TermCredit, 3);
        let totalCreditCourse = 0;

        let gPts = this.GPTSString;
        gPtsValue = gPts.split('.');
        gPtsValue[0] = additionZero(gPtsValue[0], 3);
        gPts = `${ gPtsValue[0] }.${ gPtsValue[1] }`

        let cumulativeCredit = additionZero(this.CumulativeCredit, 3);

        let cumulativegPts = this.CumulativeGPTSString;
        cumulativegPtsValue = cumulativegPts.split('.')
        cumulativegPtsValue[0] = additionZero(cumulativegPtsValue[0], 3);
        cumulativegPts = `${ cumulativegPtsValue[0] }.${ cumulativegPtsValue[1] ? cumulativegPtsValue[1] : "00" }`

        checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
        transcriptTable = checkLength[0];
        fullTable = checkLength[1];

        let isTransferredGrade = this.TranscriptCourses[0].Grade == 'TR';
        if (isTransferredGrade) {
            let totalCreditTransferred = 0;
            $(this.TranscriptCourses).each( function() {
                totalCreditTransferred += this.Credit;
            })

            let totalCreditTransferredRounded = additionZero(totalCreditTransferred, 3)
            
            switch (language) {
                case "en":
                    var yearText = text.rowDat[13],
                        yearText2 = text.rowDat[14],
                        yearText3 = `${ text.rowDat[15] } ${ this.AcademicYear }`,
                        cumulativeText = `${ text.rowDat[3] } ${ totalCreditTransferredRounded }`;

                    transcriptTable.push([{content: yearText, colSpan: 4}]);
                    transcriptTable.push([{content: yearText2, colSpan: 4}]);
                    transcriptTable.push([{content: yearText3, colSpan: 4}]);
                    break;
        
                case "th":
                    var yearText = text.rowDat[13],
                        yearText2 = `${ text.rowDat[14] } ${ text.rowDat[15] } ${ this.AcademicYear }`,
                        cumulativeText = `${ text.rowDat[3] } ${ totalCreditTransferredRounded }`;
                    
                    transcriptTable.push([{content: yearText, colSpan: 4}]);
                    transcriptTable.push([{content: yearText2, colSpan: 4}]);
                    break;
            }
        } else {
            switch (language) {
                case "en":
                    var yearText = `${ this.AcademicYear } - ${ this.AcademicYear + 1 }     ${ this.AcademicTerm }${ this.AcademicTerm == 1 ? "ST" : this.AcademicTerm == 2 ? "ND" : "RD" } TRIMESTER`,
                        semesterText = `${ text.rowDat[0] }   ${ this.TermGPAString }`,
                        cumulativeText = `${ text.rowDat[1] }   ${ this.CumulativeGPAString }`;
                    break;
        
                case "th":
                    var yearText = `ภาคเรียนที่ ${ this.AcademicTerm } ปีการศึกษา ${ this.AcademicYear }`,
                        
                        cumulativeText = `${ text.rowDat[1] }   ${ cumulativeCredit } หน่วย ค่าระดับ ${ cumulativegPts } เฉลี่ย ${ this.CumulativeGPAString }`;
                    if (this.TranscriptCourses.some(x => x.Grade == "W")) {
                        var semesterText = `${ text.rowDat[0] }    ${ termCredit } หน่วย ค่าระดับ ${ gPts } เฉลี่ย ${ this.TermGPAString }`;
                    } else {
                        var semesterText = `${ text.rowDat[0] }    ค่าระดับ ${ gPts } เฉลี่ย ${ this.TermGPAString }`;
                    }
                    break;
            }
            transcriptTable.push([{content: yearText, colSpan: 4}]);
        }

        $(this.TranscriptCourses).each( function() {
            let lineCount = this.CourseNameCount;
            if (transcriptTable.length + lineCount > lineLimit) {
                checkLength = isReachLimit(transcriptTable.length + lineCount, fullTable, transcriptTable, lineLimit);
            } else {
                checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
            }

            transcriptTable = checkLength[0];
            fullTable = checkLength[1];
            
            if (this.Grade != "TR") {
                switch (lineCount) {
                    case 3:
                        transcriptTable.push([` ${ this.CourseCode }`, this.CourseNames[0], "", ""]);
                        transcriptTable.push(["", this.CourseNames[1], "", ""]);
                        transcriptTable.push(["", this.CourseNames[2], this.Credit, this.Grade]);
                        break;
    
                    case 2:
                        transcriptTable.push([` ${ this.CourseCode }`, this.CourseNames[0], "", ""]);
                        transcriptTable.push(["", this.CourseNames[1], this.Credit, this.Grade]);
                        break;
    
                    case 1:
                        transcriptTable.push([` ${ this.CourseCode }`, this.CourseNames[0], this.Credit, this.Grade]);
                        break;
                }
                totalCreditCourse += this.Credit;
            }
        })

        if (semesterText != null) {
            checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
            transcriptTable = checkLength[0];
            fullTable = checkLength[1];
            transcriptTable.push([{content: semesterText, colSpan: 2}, {content: ` ${ totalCreditCourse }`}]);
        }

        checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
        transcriptTable = checkLength[0];
        fullTable = checkLength[1];
        transcriptTable.push([{content: cumulativeText, colSpan: 4}]);
        
        if (transcriptTable.length != 0) {
            transcriptTable.push([]);
        } 
    })

    if (transcriptTable.length > 0) {
        checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
        transcriptTable = checkLength[0];
        fullTable = checkLength[1];
        transcriptTable.pop();
        transcriptTable.push([{content: dashLine, colSpan: 4}]);

        let totalCreditCompleted = additionZero(data.TotalCreditCompleted, 3);
        checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
        transcriptTable = checkLength[0];
        fullTable = checkLength[1];
        transcriptTable.push([{content: text.rowDat[2], colSpan: 2}, {content: `= ${ totalCreditCompleted }   ${ text.rowDat[16] }`, colSpan: 2}]);

        if (data.TotalCreditTransferred != 0) {
            let totalCreditTransferred = additionZero(data.TotalCreditTransferred, 3);
            checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
            transcriptTable = checkLength[0];
            fullTable = checkLength[1];
            transcriptTable.push([{content: text.rowDat[5], colSpan: 2}, {content: `= ${ totalCreditTransferred }   ${ text.rowDat[16] }`, colSpan: 2}]);
        }

        let totalCreditEarnd = additionZero(data.TotalCreditEarnd, 3);
        checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
        transcriptTable = checkLength[0];
        fullTable = checkLength[1];
        transcriptTable.push([{content: text.rowDat[10], colSpan: 2}, {content: `= ${ totalCreditEarnd }   ${ text.rowDat[16] }`, colSpan: 2}]);

        fullTable.push(transcriptTable);
    }

    $(fullTable).each( function(index) {
        if (index % 2 === 0 && index != 0) {
            currentX = 25;
            transcript.addPage('a4', 'p');
            pageCount++;
        }

        transcript.autoTable({
            body: this,
            theme: 'plain',
            styles: {
                halign: 'left',
                font: cellFont,
                fontSize: cellFontSize,
                cellPadding: {
                    top: 0.5, 
                    right: 0.25, 
                    bottom: 0.5, 
                    left: 0.25
                },
            },
            margin: {left: currentX},
            startY: 70,
            tableWidth: 67,
            headStyles : {
                minCellHeight : 5,
            },
            columnStyles : { 
                0: { cellWidth: 15 },
                2: { halign: 'right', cellWidth: creditColumeWidth },
                3: { halign: 'left', cellWidth: 6 } 
            },
            didParseCell: function (data) {
                var rows = data.table.body;

                if (language === 'th') {
                    data.cell.styles.fontStyle = 'bold';
                    $(rows).each(function() {
                        if (Object.keys(this.cells).length > 2) {
                            this.cells[2].styles.cellPadding = {
                                top: 0, 
                                right: 1.25, 
                                bottom: 0, 
                                left: 0
                            };

                            this.cells[3].styles.cellPadding = {
                                top: 0, 
                                right: 0, 
                                bottom: 0, 
                                left: 0.25
                            };
                        } else {
                            data.cell.styles.cellPadding = 0;
                        }
                    })
                } else if (language === 'en') {
                    $(rows).each(function() {
                        if (Object.keys(this.cells).length > 2) {
                            this.cells[2].styles.cellPadding = {
                                top: 0.5, 
                                right: 1, 
                                bottom: 0.5, 
                                left: 0.25
                            };

                            this.cells[3].styles.cellPadding = {
                                top: 0.5, 
                                right: 0.25, 
                                bottom: 0.5, 
                                left: 3
                            };
                        }
                    })
                }
            },
            willDrawCell: function (data) {
                var rows = data.table.body;

                if (language === 'th') {
                    $(rows).each(function() {
                        this.height = 3.5;
                    })
                }
            }
        });

        currentTable = [];
        currentX += 92;
    });

    return pageCount;
}

function generateTranscript(dataModel) {
    var transcript = new jsPDF('p', 'mm', 'a4');
    PdfSettings.property(transcript, dataModel);

    $(dataModel.Body).each( function(dataSet) {
        let paddingX = 5;
        let currentY = defaultSetting.paddingY;
        let centerX = defaultSetting.centerX;
        let boldText = "bold";
        let simpleText = "normal"

        let data = this,
            language = data.Language.toLowerCase(),
            text,
            textStyle;
        switch (language) {
            case "en":
                text = textEn;
                textStyle = {
                    align: 'left',
                    lineHeightFactor: defaultSetting.lineHeightLarge,
                    maxWidth: centerX-paddingX
                }
                break;

            case "th":
                text = textTh;
                textStyle = {
                    align: 'left',
                    lineHeightFactor: defaultSetting.lineHeightSmallTh,
                    maxWidth: centerX-paddingX,
                    charSpace : 0.1
                }
                break;
        
            default:
                text = textEn;
                break;
        }

        function drawLayoutData () {
            switchFormat(transcript, simpleText, language);
            transcript.text(text.title1[0], paddingX + 20, currentY += 30, textStyle)
                      .text(text.title1[1], paddingX + 20, currentY += 4, textStyle)
                      .text(text.title1[2], paddingX + 20, currentY += 4, textStyle)
                      .text(text.title1[3], paddingX + 20, currentY += 12, textStyle)
                      .text(text.title1[4], paddingX + 20, currentY += 4, textStyle)
                      .text(text.dashLine, paddingX + 20, currentY += 4, 'left');

            currentY = defaultSetting.paddingY
            switchFormat(transcript, boldText, language);
            var birthDate = data.BirthDate ? getDateText(language, new Date(data.BirthDate)) : getDateText(language, new Date());
            transcript.text(`${ (data.FirstName) ? data.FirstName.toUpperCase() : "-" } ${ (data.MidName) ? data.MidName.toUpperCase() : "" } ${ (data.LastName) ? data.LastName.toUpperCase() : "-" }`, paddingX + 50, currentY += 30, textStyle)
                      .text(`${ (data.StudentCode) ? data.StudentCode : "-" }`, paddingX + 50, currentY += 4, textStyle)
                      .text(`${ (data.BirthDate) ? birthDate.toUpperCase() : "-" }`, paddingX + 50, currentY += 4, textStyle)
                      .text(`${ (data.Nationality) ? data.Nationality.toUpperCase() : "-" }`, paddingX + 50, currentY += 12, textStyle)
                      .text(`${ (data.Department) ? data.Department.toUpperCase() : "-" }`, paddingX + 50, currentY += 4, textStyle);
                      
            currentY = defaultSetting.paddingY
            switchFormat(transcript, simpleText, language);
            transcript.text(text.title2[0], centerX + 20, currentY += 42, textStyle)
                      .text(text.title2[1], centerX + 20, currentY += 4, textStyle)
                      .text(text.title2[2], centerX + 20, currentY += 8, textStyle)

            currentY = defaultSetting.paddingY
            switchFormat(transcript, boldText, language);
            var createdAt = data.CreatedAt ? getDateText(language, new Date(data.CreatedAt)) : getDateText(language, new Date());
            transcript.text(`${ (data.ApprovedBy) ? data.ApprovedBy.toUpperCase() : "" }`, centerX + 20, currentY += 38, textStyle)
                      .text(`${ (data.CreatedAt) ? createdAt.toUpperCase() : "---" }`, centerX + 35, currentY += 8, textStyle)
        }

        function drawHeaderData () {
            currentY = defaultSetting.paddingY
            switchFormat(transcript, simpleText, language);
            transcript.text(text.title3[0], paddingX + 20, currentY += 54, textStyle)
                      .text(text.title3[1], centerX + 20, currentY, textStyle)
                      

            switchFormat(transcript, boldText, language);
            transcript.text(`${ (data.FirstName) ? data.FirstName.toUpperCase() : "-" } ${ (data.MidName) ? data.MidName.toUpperCase() : "" } ${ (data.LastName) ? data.LastName.toUpperCase() : "-" }`, paddingX + 35, currentY, textStyle)
                      .text(`${ (data.StudentCode) ? data.StudentCode : "-" }`, centerX + 35, currentY, textStyle)
                      
            switchFormat(transcript, simpleText, language);
            transcript.text(text.dashLine, paddingX + 20, currentY + 4, "left");
        }

        drawLayoutData();
        let pageCount = drawTranscriptTable(transcript, data, text, language);
        if (pageCount > 1) {
            for (i = 2; i <= pageCount; i++) {
                transcript.setPage(i);
                drawHeaderData();
            }
        }

        if (dataSet + 1 < dataModel.Body.length) {
            transcript.addPage('a4', 'p');
        } 
    })

    // get preview
    $('#js-print-preview').attr('src', transcript.output('datauristring'));
};

$( function() {
    generateTranscript(dataModel);
})