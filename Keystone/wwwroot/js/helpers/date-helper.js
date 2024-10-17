/*
    document
    - https://www.w3schools.com/jsref/jsref_obj_date.asp
*/

// reference: https://stackoverflow.com/questions/6117814/get-week-of-year-in-javascript-like-in-php?answertab=votes#tab-top
Date.prototype.getWeekOfYear = function() {
  var d = new Date(Date.UTC(this.getFullYear(), this.getMonth(), this.getDate()));
  var dayNum = d.getUTCDay() + 1 || 7;
  d.setUTCDate(d.getUTCDate() + 4 - dayNum);
  var yearStart = new Date(Date.UTC(d.getUTCFullYear(),0,1));
  return Math.ceil((((d - yearStart) / 86400000) + 1)/7)
}

// reference: https://stackoverflow.com/questions/563406/add-days-to-javascript-date
Date.prototype.addDay = function(days) { // add day
    var date = new Date(this.valueOf())
    date.setDate(date.getDate() + days)
    return date
}

function getDayTitleOfWeek(day) {
    return ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"][day]
}

function changeDateFormat(dateString) {
    var date = new Date(dateString);
    var dd = date.getDate();
    var mm = date.getMonth() + 1; //January is 0!
    var yyyy = date.getFullYear();
    return `${ dd }/${ mm }/${ yyyy }`; // return dd/MM/yyyy
}

function changeTimeFormat(time) {
    var timeSplit = time.split(':')
    return `${ timeSplit[0] }:${ timeSplit[1] }` // return hh:mm
}

function getMonthNames(language) {
    var monthNamesEn = ["January", "February", "March", "April", "May", "June",
                        "July", "August", "September", "October", "November", "December"]

    var monthNamesTh = ["มกราคม","กุมภาพันธ์","มีนาคม","เมษายน","พฤษภาคม","มิถุนายน",
                        "กรกฎาคม","สิงหาคม","กันยายน","ตุลาคม","พฤษจิกายน","ธันวาคม"]

    if (language == 'th') {
        return monthNamesTh;
    } else {
        return monthNamesEn;
    }
}

function getDateText(language, fullDate, dateMonthYear) {
    var date = fullDate.getDate();
    var monthName = getMonthNames(language)[fullDate.getMonth()];
    var year = fullDate.getFullYear();

    if (language == 'th') {
        return `${ date } ${ monthName } ${ year + 543 }`;
    } else {
        if (dateMonthYear == "dmy") {
            return `${ date } ${ monthName } ${ year }`;
        } else {
            return `${ monthName } ${ date }, ${ year }`;
        }
    }
}

function getMonthText(language, month) {
    var monthName = getMonthNames(language)[month]
    return monthName;
}