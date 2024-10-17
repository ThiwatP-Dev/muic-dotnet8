var formId = '#export-excel-form';
var buttonCopyTestId ="#copy-text"

$(formId).on('submit', function() {
    $('.loader').fadeOut();
    $("#preloader").delay(1000).fadeOut("slow");
})

$(buttonCopyTestId).on("click", function(){
    navigator.clipboard.writeText($(".js-text").val());
    Alert.renderAlert("Success", "Copy email success.", "success");
});