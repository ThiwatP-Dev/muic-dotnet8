function minMaxLimit() {
    $("input").on('keyup', function() {
        let minValue = $(this).attr('min');
        let maxValue = $(this).attr('max');

        if (minValue != undefined) {
            minValue = parseInt(minValue)
            if ($(this).val() < minValue) {
                $(this).val(minValue);
            }
        }

        if (maxValue != undefined) {
            maxValue = parseInt(maxValue)
            if ($(this).val() > maxValue) {
                $(this).val(maxValue);
            }
        }
    })
}

$( function() {
    minMaxLimit();
})