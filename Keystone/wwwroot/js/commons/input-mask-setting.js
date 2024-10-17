var InputMask = (function() {

    var RenderTimeMask = function() {
        $('.js-time-mask').inputmask({
            mask: '99:99'
        })
    }

    var RenderNumberMask = function() {
        $('.js-number-mask').inputmask({
            mask: '99'
        })
    }

    return {
        renderTimeMask : RenderTimeMask,
        renderNumberMask : RenderNumberMask
    }

})();