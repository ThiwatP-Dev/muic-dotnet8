﻿(function (jsPDFAPI) {
var callAddFont = function () {
this.addFileToVFS('arial-bold.ttf', font);
this.addFont('arial-bold.ttf', 'arial', 'bold');
};
jsPDFAPI.events.push(['addFonts', callAddFont])
 })(jsPDF.API);