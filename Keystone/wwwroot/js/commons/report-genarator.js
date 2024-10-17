var ReportGenerator = (function(){

    var PreviewReport = function(form, type) {
      switch(type){
        case '0' : {
          ReportGenerator.idCardSubstituteReport(form)
        }
        case '1' : {
          ReportGenerator.invoiceReport(form)
        }
      }
    }
  
    var InvoiceReport = function(form) {
      let images = $(form).find('img');
  
      var doc = new jsPDF('p', 'mm', 'a4'),
          sizes = [25,18,12,7,14], 
          paragraphOptionThai = {
            align: 'left',
            lineHeightFactor: 0.95,
          },
          paragraphOptionEng = {
            align: 'left',
            lineHeightFactor: 1.45,
          }
          paragraphLeftEng = {
            align: 'left',
            lineHeightFactor: 0.75,
          }
          paragraphCenterEng = {
            align: 'center',
            lineHeightFactor: 1.5,
          };
  
      doc.setProperties({
          title: 'Invoice',
          subject: "Student's invoice",
          author: 'Keystone',
          creator: 'Keystone'
      });
  
      // section 1.2
      doc.setDrawColor(0,162,232)
          .setFillColor(200, 191, 231)
          .setLineWidth(0.5)
          .rect(0, 0, 210, 99, 'F')
          .line(50, 0, 50, 99)
          .setFillColor(235, 232, 248)
          .rect(55, 18, 70, 50, 'F')
          .setFillColor(0,162,232)
          .roundedRect(55, 87, 70, 7, 3, 3, 'F');
  
      doc.setFontSize(sizes[1])
          .setFont('helvetica', 'bold')
          .setTextColor(163,73,164)
          .text('REMARKS', 90, 15, 'center')
          .setFontSize(sizes[3])
          .setFont('helvetica', 'normal')
          .setTextColor(0,0,0)
          .text(['This temporary I.D. card is the property of Mahidol University.'
          ,'It must be presented to any ABAC officer upon request.'
          ,'If you lose it, a replacement will cost you 200 baht.']
          , 90, 77, paragraphCenterEng)
          .setFont('helvetica', 'bold')
          .setTextColor(255,225,255)
          .text(['www.au.edu  abac@au.edu  Tel 02-7191919, 02-7232323'
          ,'www.facebook.com/']
          , 90, 90, 'center');
  
      doc.setFontSize(sizes[2])
          .setTextColor(0,0,0)
          .setFont('sarabun', 'bold')
          .text(['Additional required documents','(เอกสารที่ต้องส่งเพิ่มเติม)'], 90, 24, 'center')
          .setFont('sarabun', 'normal')
          .text(['- OFFICIAL TRANSCRIPT','- DIPLOMA'], 57.5, 33, 'left')
  
      //section 1.3
      doc.addImage($(images)[1], 'png', 130, 5, 15, 15)
          .setFontSize(sizes[2])
          .setFont('helvetica', 'bold')
          .setTextColor(163,73,164)
          .text(['MAHIDOL UNIVERSITY','Temporary ID Card'], 175, 12.5, 'center');
      
      doc.addImage($(images)[0], 'png', 155, 22.5, 35, 35)
          .setFontSize(sizes[2])
          .setTextColor(0,0,0)
          .setFont('times', 'normal')
          .text(['Adm. No.','Firstname','Lastname','Faculty','Department','Date issued','Expires on'], 135, 65, 'left')
          .setFont('helvetica', 'normal')
          .text(['1234567','NAMAEWA','SAECLANSAKUL','INFORMATION TECHNOLOGY','INFORMATION TECHNOLOGY','19/06/2018','November 30,2018'], 160, 65, 'left')
  
      //section 2
      doc.setDrawColor(0,162,232)
          .setFillColor(200, 191, 231)
          .setLineWidth(0.25)
          .roundedRect(49, 105, 112, 8, 3, 3, 'DF')
          .rect(8, 125, 117, 60, 'F')
          .setFillColor(128, 128, 255)
          .rect(8, 185, 194, 10, 'F')
          .setFillColor(255, 255, 255)
          .setDrawColor(0,0,0)
          .rect(8, 195, 194, 10, 'DF')
          .line(125, 185, 125, 205)
          .setLineWidth(0.1)
          .setLineDash([1,1])
          .line(13, 130, 120, 130)
          .line(13, 135, 120, 135)
          .line(13, 140, 120, 140)
          .line(13, 145, 120, 145)
          .line(13, 150, 120, 150)
          .line(13, 155, 120, 155)
          .line(13, 160, 120, 160)
          .line(13, 165, 120, 165)
          .line(13, 170, 120, 170)
          .line(13, 175, 120, 175)
          .line(13, 180, 120, 180);
  
      doc.setFontSize(sizes[1])
          .setTextColor(0,162,232)
          .setFont('helvetica', 'bold')
          .text('Mahidol University of Thailand', 105, 111, 'center')
          .setFontSize(sizes[2])
          .setTextColor(0,0,0)
          .text(['Admission No.:','Name:'], 130, 120, 'left')
          .text('Enrollment and Intensive Course Fee Details', 10, 120, 'left')
          .setFontSize(sizes[3])
          .text('Remark:', 130, 160, 'left')
          .setFont('helvetica', 'normal')
          .text('* Fees will be charged without advance notice.', 10, 123, 'left')
          .text(
            ['Student who fail to make a payment by the official due date are '
            ,'requested toappear in person at AU Office of the University'
            ,'Registrar to make their payment in cash or by credit or debit'
            ,'card. Inclusive of the card processing fee according to the'
            ,'regulationsset by the Bank of Thailand'], 130, 164, paragraphOptionEng)
          .setFontSize(sizes[2])
          .setFont('sarabun', 'bold')
          .text('หมายเหตุ:', 130, 130, 'left')
          .setFont('sarabun', 'normal')
          .text(
            ['นักศึกษาที่ไม่ได้ชำระเงินตามกำหนดกรุณาติดต่อชำระเงินได้ที่'
            ,'สำนักทะเบียนและประมวลผลมหาวิทยาลัยอัสสัมชัญ โดย'
            ,'สามารถชำระได้ด้วยเงินสด, บัตรเครดิต หรือเดบิต ทั้งนี้'
            ,'การคิดค่าธรรมเนียมเป็นไปตามระเบียบของธนาคารแห่ง'
            ,'ประเทศไทย'], 130, 135, paragraphOptionThai)
          .setFontSize(sizes[1])
          .setFont('sarabun', 'bold')
          .text('ยอดเงินที่ต้องชำระ (บาท) TOTAL FEES (BAHT)', 66, 192, 'center')
          .text('กำหนดชำระเงิน PAYMENT DUE DATE', 163.5, 192, 'center');
  
      // section 2 input
      doc.setFont('sarabun', 'normal')
          .text('Subject', 14, 129, 'left')
          .text('Subject', 14, 134, 'left')
          .text('Subject', 14, 139, 'left')
          .text('Subject', 14, 144, 'left')
          .text('Subject', 14, 149, 'left')
          .text('Subject', 14, 154, 'left')
          .text('Subject', 14, 159, 'left')
          .text('Subject', 14, 164, 'left')
          .text('Subject', 14, 169, 'left')
          .text('Subject', 14, 174, 'left')
          .text('Subject', 14, 179, 'left');
  
      doc.setFont('sarabun', 'bold')
          .text('Price', 119, 129, 'right')
          .text('Price', 119, 134, 'right')
          .text('Price', 119, 139, 'right')
          .text('Price', 119, 144, 'right')
          .text('Price', 119, 149, 'right')
          .text('Price', 119, 154, 'right')
          .text('Price', 119, 159, 'right')
          .text('Price', 119, 164, 'right')
          .text('Price', 119, 169, 'right')
          .text('Price', 119, 174, 'right')
          .text('Price', 119, 179, 'right');
  
      doc.setFont('helvetica', 'bold')
          .setFontSize(sizes[2])
          .text('6110005', 165, 120, 'left')
          .text('NAMAEWA SAECLANSAKUL', 144, 125, 'left')
          .text('19,999.00', 66, 202, 'center')
          .text('June 20, 2019', 163.5, 202, 'center');
      
      // section 3.1
      doc.setFontSize(sizes[4])
          .setTextColor(0,0,0)
          .setFont('sarabun', 'bold')
          .text('Pay-in Slip(สำหรับธนาคาร)', 8, 215, 'left')
          .text('สาขาผู้รับฝาก/Office', 8, 220, 'left')
          .setLineDash([0])
          .line(40, 220, 125, 220)
          .text('ชำระเงินวันที่/Date', 130, 220, 'left')
          .line(160, 220, 202, 220)
          .setFontSize(sizes[0])
          .text('MAHIDOL UNIVERSITY', 8, 230, 'left')
          .setFontSize(sizes[4])
          .text(['รหัสนักศึกษา / Admission No.:','ชื่อ - สกุล / Student Name:','ที่อยู่ / Address:'], 8, 240, 'left');
  
      // section 3.2
      doc.setDrawColor(0,0,0)
          .setFillColor(255, 255, 255)
          .rect(8, 265, 194, 10, 'DF')
          .setFillColor(200, 191, 231)
          .rect(8, 275, 194, 8, 'DF')
          .line(125, 265, 125, 283)
          .setFontSize(sizes[2])
          .text('จำนวนเงินฝากเป็นตัวอักษร / Amount in words', 66, 269, 'center')
          .text('จำนวนเงินฝากเป็นตัวเลข / Amount in figures', 163.5, 269, 'center')
          .text(['สำหรับเจ้าหน้าที่ธนาคาร','For Bank Use Only'], 10, 279, paragraphLeftEng)
          .text(['ลายมือชื่อเจ้าหน้าที่ธนาคาร','Authorized Signature'], 127, 279, paragraphLeftEng)
          .text('* ธนาคาร( สำนักงาน/สาขา )ที่รับชำระ กรุณาใส่รหัสนักศึกษาให้ถูกต้องทุกครั้ง รับเฉพาะเงินสด', 202, 289, 'right');
  
      // section 3 input    
      doc.setFontSize(sizes[4])
          .text(['6110005','NAMAEWA SAECLANSAKUL',
          '57/1 MOO.2, BANNRAIMORSRI RD.,','SAM PHARN, RAI KHING','NAKHON PATHOM 73210'], 55, 240, 'left')
          .text('หนึ่งหมื่นเก้าพันเก้าร้อยเก้าสิบเก้าบาทถ้วน', 66, 274, 'center')
          .text('19,999.00', 163.5, 274, 'center');
          
      $('#js-preview').attr('src', doc.output('datauristring'));
    }
  
    var IdCardSubstituteReport = function(form) {
      /*let uploadedImg = new Image();
      uploadedImg.src = $('#pdf-image').val();*/
      let uploadedImg = $(form).find('img');
      let studentName = $('#pdf-student-name').val();
      let admNumber = $('#pdf-adm-number').val();
      let validTime = $('#pdf-valid-time').val();
      let contactTime = $('#pdf-contact-time').val();
  
      // doc.html(htmlSrc) // if want to use whole HTML, try to use this (must send HTML string only)
      // default A4 size is 210 x 297 mm.
      // new jsPDF parameter => orientation, unit, canvas size etc.
      var doc = new jsPDF('l', 'mm', 'a5'),
          sizes = [24,16,12], // pt. unit ?
          margin = 8,
          paragraphOptionThai = {
            align: 'left',
            lineHeightFactor: 1.25,
            maxWidth: 174
          },
          paragraphOptionEng = {
            align: 'left',
            lineHeightFactor: 1,
            maxWidth: 174
          };
  
      doc.setProperties({
          title: 'ID Card Substitute',
          subject: 'ID card substitute for student',
          author: 'Keystone',
          creator: 'Keystone'
      });
  
      doc.setDrawColor(0,0,0)
          .setLineWidth(0.0125) // mm. unit [?]
          /* .rect parameters (x,y,w,h,st) 
          => position[default=top-left]: x,y (mm.) 
          => size: w,h (mm.) 
          => style: [optional]*/
          .rect(margin, margin, 194, 132.5) 
          //.rect(margin, margin + 140.5, 194, 140.5);
      
  
      doc.setFontSize(sizes[0])
          .setFont('times', 'normal')
          /* .text parameters (tx,x,y,opt,trf)
          => text: string,array of string 
          => position: x,y (mm.) *default text y position reference is its bottom line while x is depend on alignment*
          => option: [optional]
          => transform: [optional]*/
          .text('ID CARD SUBSTITUTE', 105, margin + 9, 'center');
  
      doc.setLineWidth(0.5)
          /* .line parameters (x1,y1,x2,y2) 
          => start position: x,y (mm.) 
          => end position: x,y (mm.)*/
          .line(margin + 50, 20, 210-58, 20);
  
      doc.setFontSize(sizes[1])
          .text('OFFICE OF THE UNIVERSITY REGISTRAR', 105, 27, 'center');
      
      doc.setLineWidth(0.0125)
          .line(margin, 30, 210-8, 29);
  
      /* .img parameters (img,type,x,y,w,h)
      => image: HTML String only <img src="">
      => type: png,jpg
      => position[default=top-left]: x,y (mm.) 
      => size: w,h (mm.?) */
      //doc.addImage($(htmlImg)[0], 'png', margin + 5, 35, 45, 45);
      doc.addImage($(uploadedImg)[0], 'png', margin + 5, 35, 45, 45);
  
      doc.text('Name: ' + studentName, 63, 40)
          .text('Adm. No.: ' + admNumber, 148, 40)
          .text('Valid on: ' + validTime, 63, 50);
  
      doc.setLineWidth(0.5)
          .line(63, 55, 210-13, 55);
  
      doc.setFontSize(sizes[2])
          .text('Contact time: ' + contactTime, 68, 62)
          .text('Printed time: 1/5/2019 12:48', 138, 62);
      
      doc.setLineWidth(0.0125)
          .line(103, 75, 153, 75)
          .text("Student's signature", 128, 80, 'center');
  
      doc.setLineWidth(0.25)
          .line(margin + 5, 88, 210-13, 88)
          .line(margin + 5, 90, 210-13, 90);
  
      doc.setFontSize(sizes[0])
          .setFont('sarabun','bold')
          .text('ATTENTION : ผู้ควบคุมการสอบ / Proctor', margin + 10, 98);
  
      doc.setFontSize(sizes[1])
          .setFont('sarabun','bold')
          .text(['"บัตรนี้ใช้ได้เฉพาะการเข้าสอบตามวัน และเวลาตามที่ระบุข้างต้นเท่านั้น กรุณาเก็บบัตรนี้หลังการเซ็นชื่อ'
          ,'ของนักศึกษาในใบรายชื่อผู้เข้าสอบ และส่งคืนสำนักทะเบียนและประมวลผลพร้อมข้อสอบหลังการสอบเสร็จสิ้น"']
          , margin + 10, 105, paragraphOptionThai)
          .text(['"This ID card substitute is valid only on the date and time specified above.'
          ,'You are requested to kindly collect the card from the student after he/she has'
          ,'surrendered his/her signature on the seating list, And submit it along with'
          ,'examination papers to the office of the university registrar at the end of'
          ,'the examination as usual."']
          , margin + 10, 120, paragraphOptionEng)
  
      //doc.output('dataurlnewwindow');
      $('#js-preview').attr('src', doc.output('datauristring'));
    }
  
    return {
      previewReport : PreviewReport,
      idCardSubstituteReport : IdCardSubstituteReport,
      invoiceReport : InvoiceReport
    };
  
  })();
  
  $(document).on('click', '#js-generate-report', function() {
    let formParent = $(this).parents('form');
    let formTypeId = $('#js-select-form').val();
  
    ReportGenerator.previewReport(formParent, formTypeId);
  })
  
  /*$(document).on('change', '#js-select-form', function() {
    let formTypeId = $(this).val();
    let targetForm = $('#js-pdf-form');
    let isHasForm = $(targetForm).find('partial')
    
    if(isHasForm.length > 0){
      isHasForm.remove() // not test yet ; must remove old partial
    } 
  
    switch(formTypeId){
      case '0' : {
        targetForm.load('@Url.Action("RenderPartial","User")');
        $.get('@Url.Action("RenderPartial","user", new { id = Model.ID } )', function(data) {
          targetForm.replaceWith(data);         
        });
      }
    }
  })*/