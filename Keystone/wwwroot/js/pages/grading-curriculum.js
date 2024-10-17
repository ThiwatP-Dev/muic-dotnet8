$(document).ready(function() {
    $('div.block__title.collapsed').each(function(){
        $(this).removeClass('collapsed');
        $(this).attr('aria-expanded', 'true');
        var id = $(this).data('target');
        $(id).addClass('show');
    });
});