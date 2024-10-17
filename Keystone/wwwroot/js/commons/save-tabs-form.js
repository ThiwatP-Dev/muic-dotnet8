let activeTab = $('#student-profile-tab-content .tab-pane.active');
let activeForm = activeTab.children().find('form');

const saveTabForm = () => {
    activeTab = $('#student-profile-tab-content .tab-pane.active');
    activeForm = activeTab.children().find('form')[0];

    //trigger submit event
    activeForm.submit();
}