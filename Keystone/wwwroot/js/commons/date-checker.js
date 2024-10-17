var KSDateTime = (function() {
    
    var compareExpiredDateTime = function(createdDateTime, expiredDateTime) {
        var startTime = new Date(createdDateTime);
        var endTime = new Date(expiredDateTime);

        if(endTime < startTime) {
            return('Expired date is earlier than created date');
        } else {
            return false;
        }
    }

    return {
        compareExpiredDateTime: compareExpiredDateTime,
    };

})();