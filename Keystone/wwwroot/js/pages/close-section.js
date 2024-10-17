var sectionType = '.js-master-joint-select';
var studentListTable = '.js-student-list';

$(document).ready(function() {
    let seatUsed = $('.js-seat-used');
    seatUsed.each(function() {
        if($(this).html() < 1) {
            let transferButton = $(this).parents('tr').find('.js-student-transfer');
            transferButton.empty();
        }
    })
    
    $(document).on('click', '.js-alert-danger', function() {
        let thisSection = $(this).parents('tr').find('.js-section').html()
        Alert.renderAlert("Unable to transfer",`No any student in Section ${ thisSection }`,"error")
    })
})

$(document).on('change', sectionType, function() {
    var sectionTypeValue = $(this).val().toUpperCase()
    tr = $(studentListTable).find("tr");
    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[3];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf(sectionTypeValue) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
});