var dataModel = $('#js-pdf-preview').data('model');

var page = {
    // time : ["Time/Day", "08:00 - 08:50", "09:00 - 09:50", "10:00 - 10:50", "11:00 - 11:50", "12:00 - 12:50", "13:00 - 13:50", 
    //         "14:00 - 14:50", "15:00 - 15:50", "16:00 - 16:50", "17:00 - 17:50", "18:00 - 18:50", "19:00 - 19:50"],
    time : ["Time/Day", "08:00 - 09:50", "10:00 - 11:50", "12:00 - 13:50", "14:00 - 15:50", "16:00 - 17:50", "18:00 - 19:50"],
    day : ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"],
    remark : "*** MT = Midterm, FN = Final, A = Activity, M = Meeting"
};

function switchFormat(pdf, format) {
    switch (format) {
        case "bold":
            PdfSettings.format(pdf, textFormats.sansBoldXS)
            break;

        case "normal":
            PdfSettings.format(pdf, textFormats.sansXS)
            break;
    }
}

function roomSchedule(dataModel) {
    var report = new jsPDF('l', 'mm', 'a4');
    PdfSettings.property(report, dataModel);
    let centerX = defaultSetting.centerX;
        paddingX = defaultSetting.paddingX
        currentY = 10;
        headerLine = 7;
        contentLine = 5;
        maxWidth = defaultSetting.A4MaxWidth - paddingX;
        boldText = "bold";
        simpleText = "normal";

    var sizes = [12, 14, 18, 11, 8]
    var currentReceiptLine = 10;
    if (dataModel.Preview != null) {
        dataModel.Preview.forEach((item, index) => {
            currentReceiptLine = 10;
            switchFormat(report, boldText);
            report.setFontSize(sizes[2])
                .text(`Room : ${item.Name}   Building : ${item.BuildingName}    Campus : ${item.CampusName}`, paddingX, currentReceiptLine);
            currentReceiptLine += 10;
            if (item.DateString) {
                report.setFontSize(sizes[2])
                    .text(`AcademicYear : ${item.Term}  for ${item.DateString}  as of : ${item.PrintDateString}`, paddingX, currentReceiptLine);
            }
            else {
                report.setFontSize(sizes[2])
                    .text(`AcademicYear : ${item.Term}   as of : ${item.PrintDateString}`, paddingX, currentReceiptLine);
            }

            switchFormat(report, simpleText);
            report.setFontSize(sizes[1])
                .text(page.time[0], paddingX + 5, currentReceiptLine += 10, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.time[1], paddingX += 35, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.time[2], paddingX += 43, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.time[3], paddingX += 41, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.time[4], paddingX += 43, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.time[5], paddingX += 41, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.time[6], paddingX += 43, currentReceiptLine, PdfSettings.style(textStyles.CertificateLeft));

            paddingX = defaultSetting.paddingX
            report.text(page.day[0], paddingX + 7, currentReceiptLine += 15, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.day[1], paddingX + 7, currentReceiptLine += 20, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.day[2], paddingX + 7, currentReceiptLine += 20, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.day[3], paddingX + 7, currentReceiptLine += 20, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.day[4], paddingX + 7, currentReceiptLine += 20, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.day[5], paddingX + 7, currentReceiptLine += 20, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.day[6], paddingX + 7, currentReceiptLine += 20, PdfSettings.style(textStyles.CertificateLeft))
                .text(page.remark, paddingX + 7, currentReceiptLine += 20, PdfSettings.style(textStyles.CertificateLeft));

            report.line(10, 35, 290, 35)
                  .line(10, 53, 290, 53)
                  .line(10, 73, 290, 73)
                  .line(10, 93, 290, 93)
                  .line(10, 113, 290, 113)
                  .line(10, 133, 290, 133)
                  .line(10, 153, 290, 153)
                  .line(10, 173, 290, 173)
                  .line(32, 35, 32, 173)
                //   .line(55, 35, 55, 173)
                  .line(76, 35, 76, 173)
                //   .line(97, 35, 97, 173)
                  .line(118, 35, 118, 173)
                //   .line(139, 35, 139, 173)
                  .line(160, 35, 160, 173)
                //   .line(181, 35, 181, 173)
                  .line(202, 35, 202, 173)
                //   .line(223, 35, 223, 173)
                  .line(244, 35, 244, 173)
                //   .line(265, 35, 265, 173);

            if (item.Schedules != null) {
                let day = 0;
                let startTime = 0;
                let timeBetween = 0;
                let timeRange = 0;
                let textRange = 0;

                var informationConfig = {
                    align: 'center',
                    lineHeightFactor: 5
                }

                item.Schedules.forEach((schedule) => {
                    if (schedule.ScheduleTimes != null) {
                        schedule.ScheduleTimes.forEach((time) => {
                            switch(time.DayOfWeek) {
                                case "SUN":
                                    day = 36;
                                    break;
                                case "MON":
                                    day = 55;
                                    break;
                                case "TUE":
                                    day = 75;
                                    break;
                                case "WED":
                                    day = 95;
                                    break;
                                case "THU":
                                    day = 115;
                                    break;
                                case "FRI":
                                    day = 135;
                                    break;
                                case "SAT":
                                    day = 155;
                                    break;
                            }
                            switch(time.TimeStartHours) {
                                case 8:
                                    startTime = 34;
                                    break;
                                case 9:
                                    startTime = 56;
                                    break;
                                case 10:
                                    startTime = 77;
                                    break;
                                case 11:
                                    startTime = 98;
                                    break;
                                case 12:
                                    startTime = 119;
                                    break;
                                case 13:
                                    startTime = 140;
                                    break;
                                case 14:
                                    startTime = 161;
                                    break;
                                case 15:
                                    startTime = 182;
                                    break;
                                case 16:
                                    startTime = 203;
                                    break;
                                case 17:
                                    startTime = 224;
                                    break;
                                case 18:
                                    startTime = 245;
                                    break;
                                case 19:
                                    startTime = 266;
                                    break;
                            }

                            timeBetween = time.TimeEndHours - time.TimeStartHours;
                            if(time.TimeEndMinutes == 50)
                            {
                                timeBetween += 1;
                            } else {
                                //1 hour = 10 , 1 minutes = 10/60
                                timeRange += time.TimeEndMinutes * (10.0/60.0);
                            }
                            for (var i = 1; i <= timeBetween; i++) {
                                timeRange = 19;
                                switch (i) {
                                    case 1:
                                        textRange = 10;
                                        break;
                                    case 2:
                                        timeRange += 21;
                                        textRange = 21;
                                        break;
                                    case 3:
                                        timeRange += 42;
                                        textRange = 31;
                                        break;
                                    case 4:
                                        timeRange += 63;
                                        textRange = 42;
                                        break;
                                    case 5:
                                        timeRange += 84;
                                        textRange = 53;
                                        break;
                                    case 6:
                                        timeRange += 105;
                                        textRange = 64;
                                        break;
                                    case 7:
                                        timeRange += 126;
                                        textRange = 75;
                                        break;
                                    case 8:
                                        timeRange += 147;
                                        textRange = 86;
                                        break;
                                    case 9:
                                        timeRange += 168;
                                        textRange = 97;
                                        break;
                                    case 10:
                                        timeRange += 189;
                                        textRange = 108;
                                        break;
                                    case 11:
                                        timeRange += 210;
                                        textRange = 119;
                                        break;
                                }

                                if (time.TimeStartMinutes == 30) {
                                    timeRange = timeRange + 10
                                }
                            }

                            report.setFillColor(255, 255, 255)
                                  .roundedRect(startTime, day, timeRange, 16, 3, 3, 'DF');

                            let course = `${ time.CourseCode }(${ time.SectionNumber })` + (time.Type ? ` (${ time.Type })` : "");
                            var splitCourse = report.splitTextToSize(course, 40);
                            var splitInstructor = time.InstructorShortName == null ? "N/A" : time.InstructorShortName.match(new RegExp('.{1,' + 17 + '}', 'g'));
                            if (timeBetween == 1) {
                                report.setFontSize(sizes[0])
                                      .text(startTime + textRange, day + 3, splitCourse, 'center');

                                if (time.InstructorShortName.startsWith('Assoc. Prof. Dr.')) {
                                    report.setFontSize(sizes[4])
                                          .text(startTime + textRange, day + 7, splitInstructor[0], 'center')
                                          .text(startTime + textRange, day + 10, splitInstructor[1], 'center');
                                } else {
                                    report.setFontSize(sizes[4])
                                          .text(startTime + textRange, day + 8, time.InstructorShortName, 'center');
                                }
                                
                                report.setFontSize(sizes[3])
                                      .text(`(${ time.Time })`, startTime + textRange, day + 14, PdfSettings.style(textStyles.CertificateCenter));
                            } else if (timeBetween == 2) {
                                report.setFontSize(sizes[0])
                                      .text(startTime + textRange, day + 3, splitCourse, 'center')
                                      
                                if (report.getTextWidth(time.InstructorNameEn) >= 38) {
                                    report.text(startTime + textRange, day + 8, time.InstructorShortName, 'center')
                                } else {
                                    report.text(startTime + textRange, day + 8, time.InstructorNameEn, 'center')
                                }

                                report.setFontSize(sizes[3])
                                      .text(`(${ time.Time })`, startTime + textRange, day + 14, PdfSettings.style(textStyles.CertificateCenter));
                            } else {
                                report.setFontSize(sizes[0])
                                      .text(course, startTime + textRange, day + 5, PdfSettings.style(textStyles.CertificateCenter))
                                      .text(time.InstructorNameEn, startTime + textRange, day + 9, PdfSettings.style(textStyles.CertificateCenter))
                                
                                report.setFontSize(sizes[3])
                                      .text(`(${ time.Time })`, startTime + textRange, day + 13, PdfSettings.style(textStyles.CertificateCenter));
                            }
                        })
                    }
                })
            }

            currentReceiptLine = currentReceiptLine + 5;
            if (index + 1 < dataModel.Preview.length) {
                report.addPage('a4', 'l');
            }
        })
    };

    $('#js-print-preview').attr('src', report.output('datauristring'));
}

$(document).ready( function() {
    let data = dataModel.Body;
    roomSchedule(data);
})