let channel = "#js-cascade-channel";
let topic = "#js-cascade-topic";

function cascadeTopics(channelId) {

    var ajax = new AJAX_Helper(
        {
            url: SelectListURLDict.GetTopicByChannelId,
            data: {
                id: channelId,
            },
            dataType: 'json'
        }
    );

    ajax.POST().done(function (response) { 
        $(topic).empty();

        response.forEach((item) => {
            $(topic).append(getSelectOptions(item.id, item.nameEn));
        });

        $(topic).trigger("chosen:updated");
    })
    .fail(function (jqXHR, textStatus, errorThrown) { 
        ErrorCallback(jqXHR, textStatus, errorThrown);
    });
}

$(channel).on('change', function() {
    let channelId = $(this).val();

    if ($(topic).length > 0) {
        cascadeTopics(channelId);
    }
})