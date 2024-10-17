
$(document).on('click',"#button-logout", function() {
    localStorage.removeItem('ExpiredToken');
    localStorage.removeItem('TokenMenu');
});