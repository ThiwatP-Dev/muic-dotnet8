$('#IdCardCreatedDate').on('change', function() {
    var createdDate = $("#IdCardCreatedDate").val()
    var yearEnd = parseInt(createdDate.substr(6, 4)) + 6; 
    var expiredDate = createdDate.substr(0,6) + yearEnd;
    $("#IdCardExpiredDate").val(expiredDate);
    
    // remove expired date error message
    $("#IdCardtimeCorrection").html(' ');
});

$('#IdCardExpiredDate').on('change', function() {
    $("#IdCardtimeCorrection").html(' ');
    var createdDate = $("#IdCardCreatedDate").val()
    var expiredDate = $("#IdCardExpiredDate").val();
    createdDate = createdDate.substring(3, 6)+createdDate.substring(0, 3)+createdDate.substring(6, 10);
    expiredDate = expiredDate.substring(3, 6)+expiredDate.substring(0, 3)+expiredDate.substring(6, 10);

    var result = KSDateTime.compareExpiredDateTime(createdDate, expiredDate);

    $("#IdCardtimeCorrection").html(result);
});