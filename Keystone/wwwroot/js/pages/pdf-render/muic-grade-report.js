var dataModel = $('#js-pdf-preview').data('model');
var universityNameEn = $('body').data('uname-en');
var universityNameTh = $('body').data('uname-th');
var textEn = {
    title1 : ["STUDENT ID :", "SEX :", "BIRTHDATE :", "NATIONALITY :", "GRADUATED :"],
    title2 : ["MAHIDOL UNIVERSITY", "FACULTY :", "PROGRAM :", "CONCENTRATION :", "MINOR :"],
    title3 : ["MUIC-ICSTUDENT", "GRADE REPORT", "Printed on"],
    rowDat : ["TRIMESTER-GPA", "CUM-GPA", "TOTAL CREDITS EARNED", "TOTAL CREDITS AUDITED", "TOTAL CREDITS EXEMPTED",
              "TOTAL CREDITS TRANSFERRED", "TOTAL FOUNDATION COURSE CREDITS", "TOTAL INDEPENDENT STUDY COMPLETED",
              "TOTAL PH.D DISSERTATION COMPLETED", "TOTAL THESIS COMPLETED", "TOTAL CREDITS REGISTERED", 
              "ENTRY BELOW THIS LINE IS INVALID", "COURSE REQUIREMENTS FULLFILLED", "CREDITS TRANSFERRED",
              "TRANSFERRED FROM GENERAL EDUCATION", "DEPARTMENT, YEAR", "", "THESIS TITLE", "DISERTATION TITLE"],
    footer : ["TRANSCRIPT GUIDE PRINTED ON REVERSE", "DATE ISSUED:", "CERTIFIED TRUE COPY", "NOT VALID WITHOUT SEAL", "UNIVERSITY REGISTRAR"],
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
    let transfer = data.Transfer;
    let transferWithGrade = data.TransferWithCourse;
    let transcriptTable = [];
    let fullTable = [];
    let sumCredit = 0;
    let currentX = 15;
    let pageCount = 1;

    var checkLength;
    switch (language) {
        case "en":
            var cellFont = 'arial',
                cellFontSize = 6.5,
                creditColumeWidth = 4,
                NameCourseColumeWidth = 58,
                lineLimit = 38,
                dashLine = "END OF GRADE REPORT";
            break;

        case "th":
            var cellFont = 'sarabun',
                cellFontSize = 11,
                creditColumeWidth = 10,
                NameCourseColumeWidth = 52,
                lineLimit = 55,
                dashLine = "--------------------------------------------------------------------------------";
            break;
    }

    $(transfer).each( function() {
        if (!this.IsTransfer) {
            let termCredit = additionZero(this.TermCredit, 3);

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
                switch (language) {
                    case "en":
                        var yearText = `    TRANSFER    ` + (data.TransferedUniversity.NameEn ? `FROM ${ data.TransferedUniversity.NameEn }` : "");
                        var cumulativeText = `${ text.rowDat[5] }   ${ this.TotalCreditString }`;
                        break;
            
                    case "th":
                        var yearText = `ภาคเรียนที่ ${ this.AcademicTerm } ปีการศึกษา ${ this.AcademicYear }`,
                            semesterText = `${ text.rowDat[0] }   ${ termCredit } หน่วย ค่าระดับ ${ gPts } เฉลี่ย ${ this.TermGPAString }`,
                            cumulativeText = `${ text.rowDat[1] }   ${ cumulativeCredit } หน่วย ค่าระดับ ${ cumulativegPts } เฉลี่ย ${ this.CumulativeGPAString }`;
                        break;
                }
                transcriptTable.push([{content: yearText, colSpan: 4}]);

            $(this.TranscriptCourses).each( function() {
                let lineCount = this.CourseNameCount;
                if (transcriptTable.length + lineCount > lineLimit) {
                    checkLength = isReachLimit(transcriptTable.length + lineCount, fullTable, transcriptTable, lineLimit);
                } else {
                    checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
                }

                transcriptTable = checkLength[0];
                fullTable = checkLength[1];

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
            })
            
            if (cumulativeText != null) {
                var courseFooter = `${ cumulativeText }`
                checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
                transcriptTable = checkLength[0];
                fullTable = checkLength[1];
                transcriptTable.push([{content: courseFooter, colSpan: 4}]);
            }
        }
    })

    $(transferWithGrade).each( function() {
        if (!this.IsTransfer) {
            let termCredit = additionZero(this.TermCredit, 3);

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
                switch (language) {
                    case "en":
                        var yearText = `    TRANSFER WITH GRADE` + (data.TransferedUniversity.NameEn ? `FROM ${ data.TransferedUniversity.NameEn }` : "");
                        var semesterText = `${ text.rowDat[0] }   ${ this.TermGPATranscriptString }`,
                            cumulativeText = `${ text.rowDat[1] }   ${ this.CumulativeGPAString }`;
                        break;
            
                    case "th":
                        var yearText = `ภาคเรียนที่ ${ this.AcademicTerm } ปีการศึกษา ${ this.AcademicYear }`,
                            semesterText = `${ text.rowDat[0] }   ${ termCredit } หน่วย ค่าระดับ ${ gPts } เฉลี่ย ${ this.TermGPAString }`,
                            cumulativeText = `${ text.rowDat[1] }   ${ cumulativeCredit } หน่วย ค่าระดับ ${ cumulativegPts } เฉลี่ย ${ this.CumulativeGPAString }`;
                        break;
                }
                transcriptTable.push([{content: yearText, colSpan: 4}]);

            $(this.TranscriptCourses).each( function() {
                let lineCount = this.CourseNameCount;
                if (transcriptTable.length + lineCount > lineLimit) {
                    checkLength = isReachLimit(transcriptTable.length + lineCount, fullTable, transcriptTable, lineLimit);
                } else {
                    checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
                }

                transcriptTable = checkLength[0];
                fullTable = checkLength[1];

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
            })
            
            if (semesterText != null) {
                var courseFooter = `${ cumulativeText }  ${ this.CumulativeCreditCompString }  ${ this.TotalRegistrationCreditFromCreditString }          ${ semesterText }  ${ this.TotalCreditString }  ${ this.TotalRegistrationCreditFromCreditString }`
                checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
                transcriptTable = checkLength[0];
                fullTable = checkLength[1];
                transcriptTable.push([{content: courseFooter, colSpan: 4}]);
            }
        }
    })

    $(transcriptData).each( function() {
        if (!this.IsTransfer) {
            let termCredit = additionZero(this.TermCredit, 3);
            let gPts = this.GPTSString;
            let sumTermCredit = 0;
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
            switch (language) {
                case "en":
                    if (this.IsSummer) {
                        var yearText = `    ${ this.AcademicYear }     SUMMER SESSION`
                    } else {
                        var yearText = `    ${ this.AcademicYear }     ${ this.AcademicTerm }${ this.AcademicTerm == 1 ? "ST" : this.AcademicTerm == 2 ? "ND" : "RD" } TRIMESTER`
                    }
                    var semesterText = `${ text.rowDat[0] }   ${ this.TermGPATranscriptString }`,
                        cumulativeText = `${ text.rowDat[1] }   ${ this.CumulativeGPAString }`;
                    break;
        
                case "th":
                    var yearText = `ภาคเรียนที่ ${ this.AcademicTerm } ปีการศึกษา ${ this.AcademicYear }`,
                        semesterText = `${ text.rowDat[0] }   ${ termCredit } หน่วย ค่าระดับ ${ gPts } เฉลี่ย ${ this.TermGPAString }`,
                        cumulativeText = `${ text.rowDat[1] }   ${ cumulativeCredit } หน่วย ค่าระดับ ${ cumulativegPts } เฉลี่ย ${ this.CumulativeGPAString }`;
                    break;
            }

            //Calculate longer text as new line
            var offSet = 0;
            this.TranscriptCourses.forEach(function (item) {
                if (item.CourseNames.length > 0) {
                    if (item.CourseNames[0].length > 0) {
                        offSet += Math.floor(item.CourseNames[0].length / 50);
                    }
                }
            });
            transcriptTable.forEach(function (item) {
                if (item.length > 3) {
                    if (item[1].length > 0) {
                        offSet += Math.floor(item[1].length / 50);
                    }
                }
            });

            let tableCount = transcriptTable.length + this.TranscriptCoursesCount + 2 + offSet; // 2 line = termtext and GPA Cum
            if (tableCount > lineLimit) {
                for (var i = 0 ; i < (lineLimit - transcriptTable.length); i++) {
                    transcriptTable.push([]);
                }
                checkLength = isReachLimit(tableCount, fullTable, transcriptTable, lineLimit);
            } else {
                checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
            }
            transcriptTable = checkLength[0];
            fullTable = checkLength[1];
            if(this.TranscriptCoursesCount > 0)
            {
                transcriptTable.push([{content: yearText, colSpan: 4}]);
            }
            $(this.TranscriptCourses).each( function() {
                let lineCount = this.CourseNameCount;
                // if (tableCount > lineLimit) {
                //     checkLength = isReachLimit(tableCount, fullTable, transcriptTable, lineLimit);
                // } 
                // else {
                //     checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
                // }

                // transcriptTable = checkLength[0];
                // fullTable = checkLength[1];

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
                
                sumCredit += this.Credit;
                sumTermCredit += this.Credit;
            })
            // if (semesterText != null) {
            if(this.TranscriptCoursesCount > 0)
            {
                var courseFooter = `${ cumulativeText }  ${ concatStringSpace(this.CumulativeCreditCompString) }  ${ concatStringSpace(this.CumulativeCreditRegisString) }       ${ semesterText }  ${ concatStringSpace(this.TotalCreditString)}  ${concatStringSpace(this.TotalRegistrationCreditString) }`
                checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
                transcriptTable = checkLength[0];
                fullTable = checkLength[1];
                transcriptTable.push([{content: courseFooter, colSpan: 5}]);
                if(lineLimit - transcriptTable.length <= 2)
                {
                    for (var i = 0 ; i < (lineLimit - transcriptTable.length); i++) {
                        transcriptTable.push([]);
                    }
                }
            }
            // }
        }
    })

    if ((transcriptData.length + transferWithGrade.length + transfer.length) > 0) {
        checkLength = isReachLimit(transcriptTable.length + 5, fullTable, transcriptTable, lineLimit);
        transcriptTable = checkLength[0];
        fullTable = checkLength[1];
        // transcriptTable.pop();
        transcriptTable.push([{content: dashLine, colSpan: 4}]);

        // let totalCreditCompleted = additionZero(data.TotalCreditCompleted, 3);
        // checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
        // transcriptTable = checkLength[0];
        // fullTable = checkLength[1];
        transcriptTable.push([{content: text.rowDat[2], colSpan: 2}, {content: `= `, colSpan: 1}, {content: `${ data.TotalCreditCompleted } ${ text.rowDat[16] }`, colSpan: 1}]);

        if (data.TotalCreditTransferred != 0) {
            // let totalCreditTransferred = additionZero(data.TotalCreditTransferred, 3);
            // checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
            // transcriptTable = checkLength[0];
            // fullTable = checkLength[1];
            transcriptTable.push([{content: text.rowDat[5], colSpan: 2},{content: `= `, colSpan: 1}, {content: `${ data.TotalCreditTransferred } ${ text.rowDat[16] }`, colSpan: 1}]);
        }

        // let totalCreditEarnd = additionZero(data.TotalCreditEarnd, 3);
        // checkLength = isReachLimit(transcriptTable.length, fullTable, transcriptTable, lineLimit);
        // transcriptTable = checkLength[0];
        // fullTable = checkLength[1];
        transcriptTable.push([{content: text.rowDat[10], colSpan: 2}, {content: `= `, colSpan: 1}, {content: `${ data.TotalCreditEarnd } ${ text.rowDat[16] }`, colSpan: 1}]);
        fullTable.push(transcriptTable);
    }

    $(fullTable).each( function(index) {
        if (index % 3 === 0 && index != 0) {
            currentX = 15;
            transcript.addPage('a4', 'l');
            if (dataModel.Criteria != null && !dataModel.Criteria.IsOfficial) {
                renderWaterMark(transcript);
            }
            
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
            startY: 45,
            tableWidth: 85,
            headStyles : {
                minCellHeight : 5,
            },
            columnStyles : { 
                0: { cellWidth: 15 },
                1: { halign: 'left', cellWidth: NameCourseColumeWidth},
                2: { halign: 'right', cellWidth: creditColumeWidth },
                3: { halign: 'left', cellWidth: 8 } 
            },
            didParseCell: function (data) {
                var rows = data.table.body;
                if (language === 'th') {
                    $(rows).each(function() {
                        if (Object.keys(this.cells).length > 2) {
                            this.cells[2].styles.cellPadding = {
                                top: 0, 
                                right: 1.25, 
                                bottom: 0, 
                                left: 0
                            };

                            this.cells[2].styles.cellPadding = {
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

                            data.row.raw.forEach((item) => {
                                if (item.content != undefined) {
                                    if (item.content.endsWith("TRIMESTER") || item.content.endsWith("SESSION") || item.content.startsWith("    TRANSFER")) {
                                        data.cell.styles.fontStyle = 'bolditalic';
                                    }
                                }
                            });
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

function generateGradeReport(dataModel) {
    var transcript = new jsPDF('l', 'mm', 'a4');
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
            textStyleRight = {
                align: 'right',
                lineHeightFactor: defaultSetting.lineHeightLarge,
                maxWidth: centerX-paddingX
            };
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
            transcript.setDrawColor(0, 0, 0)
                      .setFillColor(255, 255, 255)
                      .rect(37, 15, 107, 25, 'DF');
            
            var image = new Image();            
            image.src = $('#img-student-'+data.StudentCode)[0].src;
            image.crossOrigin = "anonymous";
            
            currentY = defaultSetting.paddingY
            switchFormat(transcript, boldText, language);
            var imgHeight = $('#img-student-'+data.StudentCode)[0].height;
            var imageWidth = $('#img-student-'+data.StudentCode)[0].width;
            var imgRatio = imgHeight / imageWidth
            var imgRatioWidth = 25 * (1 / imgRatio)

            transcript.addImage(image, 'png', 15, 15, imgRatioWidth, 25)
                      .text(text.title1[0], paddingX + 35, currentY += 10, textStyle)
                      .text(text.title1[1], paddingX + 75, currentY, textStyle)
                      .text(`${ (data.FirstName != "") ? data.FirstName.toUpperCase() : "-" } ${ (data.MidName != null) ? data.MidName.toUpperCase() : "" } ${ (data.LastName != "") ? data.LastName.toUpperCase() : "-" }`, paddingX + 35, currentY += 4, textStyle)
                      .text(text.title1[2], paddingX + 35, currentY += 4, textStyle)
                      .text(text.title1[3], paddingX + 35, currentY += 4, textStyle)
                      .text( (data.StudentStatusText == "Studying") ? data.StudentStatusText.toUpperCase()
                                                                    : data.StudentStatusText.toUpperCase() + " : ", paddingX + 35, currentY += 4, textStyle);

            currentY = defaultSetting.paddingY
            var statusX = transcript.getTextWidth(data.StudentStatusText.toUpperCase() + " : ");
            switchFormat(transcript, simpleText, language);
            transcript.text(`${ (data.StudentCode != "") ? data.StudentCode : "-" }`, paddingX + 51, currentY += 10, textStyle)
                      .text(data.Gender, paddingX + 82, currentY, textStyle)
                      .text(`${ (data.BirthDate) ? data.BirthDate.toUpperCase() : "-" }`, paddingX + 51, currentY += 8, textStyle)
                      .text(`${ (data.Nationality) ? data.Nationality.toUpperCase() : "-" }`, paddingX + 53, currentY += 4, textStyle)
                      .text(`${ (data.Award ? data.Award : "")} ${ (data.StudentStatusText == "Studying") ? ""
                                                                                                          : data.StatusAtText == null ? " - "
                                                                                                          : data.Award ? ", " + data.StatusAtText.toUpperCase() 
                                                                                                                       : data.StatusAtText.toUpperCase() }`, paddingX + 35 + statusX , currentY += 4, textStyle);
                      
            transcript.setDrawColor(0, 0, 0)
                      .setLineWidth(0.2)
                      .setFillColor(255, 255, 255)
                      .rect(147, 15, 107, 25, 'DF')
                      .setDrawColor(0, 0, 0)
                      .setLineWidth(0.2)
                      .line(15, 43, 285, 43, 'DF');

            currentY = defaultSetting.paddingY
            switchFormat(transcript, boldText, language);
            transcript.text(text.title2[0], centerX + 45, currentY += 10, textStyle)
                      .text(text.title2[1], centerX + 45, currentY += 4, textStyle)
                      .text(text.title2[2], centerX + 45, currentY += 4, textStyle)
                      .text(text.title2[3], centerX + 45, currentY += 4, textStyle)
                      .text(text.title2[4], centerX + 45, currentY += 4, textStyle);

            currentY = defaultSetting.paddingY
            switchFormat(transcript, simpleText, language);
            transcript.text(`INTERNATIONAL COLLEGE`, centerX + 59, currentY += 14, textStyle)
                      .text(`${ (data.Department != null && data.Department != "") ? data.Department.toUpperCase() : "-" }`, centerX + 60, currentY += 4, textStyle)
                      .text(`${ (data.Concentration != null && data.Concentration != "") ? data.Concentration.toUpperCase() : "-" }`, centerX + 68, currentY += 4, textStyle)
                      .text(`${ (data.Minor != null && data.Minor != "") ? data.Minor.toUpperCase() : "-" }`, centerX + 55, currentY += 4, textStyle);

            currentY = defaultSetting.paddingY
            switchFormat(transcript, boldText, language);
            transcript.text(text.title3[0], centerX + 175, currentY += 10, textStyleRight)
                      .text(text.title3[1], centerX + 175, currentY += 4, textStyleRight);
            
            switchFormat(transcript, simpleText, language);
            transcript.text(`${ text.title3[2] } ${ data.CreatedAtText }`, centerX + 175, currentY += 4, textStyleRight)
                      .text(data.Time != null ? data.Time : "", centerX + 175, currentY += 4, textStyleRight);
        }

        drawLayoutData();
        if (dataModel.Criteria != null && !dataModel.Criteria.IsOfficial) {
            renderWaterMark(transcript);
        }

        let pageCount = drawTranscriptTable(transcript, data, text, language);
        if (pageCount > 1) {
            for (i = 0; i < pageCount; i++) {
                drawLayoutData();
            }
        }

        if (dataSet + 1 < dataModel.Body.length) {
            transcript.addPage('a4', 'l');
        } 
    })

    // get preview
    $('#js-print-preview').attr('src', transcript.output('datauristring'));
};

$( function() {
    generateGradeReport(dataModel);
})

function concatStringSpace(result)
{
    if(result.length == 3)
    {
        return result;
    } else if (result.length == 2)
    {
        return` ${ result }`;
    } else if(result.length == 1)
    {
        return `  ${ result }`;
    }
    else
    {
        return `   `;
    }
}

function renderWaterMark(transcript) {
    transcript.setFontSize(80);
    transcript.setTextColor(defaultColor.lightGray);
    var waterMarkX = defaultSetting.A4Height / 2;
    var waterMarkY = defaultSetting.A4Width / 2;
    transcript.text("U N O F F I C I A L", waterMarkX, waterMarkY, null, null, "center");
}