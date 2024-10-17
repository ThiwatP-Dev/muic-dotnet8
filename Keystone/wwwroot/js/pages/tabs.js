/*------------------------------------
  set tabar scroll
------------------------------------*/

var hidWidth;
var scrollBarWidths = $(this).width() - 576;
var saveFunction = $('#js-save-btn').attr('onclick');

var widthOfList = function() {
  var itemsWidth = 0;
  $('.ks-tabs li').each(function() {
    var itemWidth = $(this).outerWidth();
    itemsWidth+=itemWidth;
  });

  return itemsWidth;
};

var widthOfHidden = function() {
  return (($('.nav-tabs-wrapper').outerWidth())-widthOfList()-getLeftPosi())-scrollBarWidths;
};

var getLeftPosi = function() {
  return $('.ks-tabs').position().left;
};

var reAdjust = function() {
  let tabsWidth = -40;
  $('.ks-tabs').find('.nav-item').each( function() {
    tabsWidth += $(this).width() + 40;
  })
  
  if (tabsWidth >= $(this).width()-285) {
    $('.tabs-scroller.float-right').show();
  } else {
    $('.tabs-scroller.float-right').hide();
  }
  
  if (getLeftPosi() < 0) {
    $('.tabs-scroller.float-left').show();
  } else {
  	$('.tabs-scroller.float-left').hide();
  }
}

function getUrlParameter(name) {
  name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
  var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
  var results = regex.exec(location.search);
  return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
};

function disableSaveBtn(currentTabLink) {
  let isSavable = currentTabLink.data('savable');

  if (isSavable) {
    $('#js-save-btn').attr('onclick', saveFunction).removeClass('disable-item');
  } else {
    $('#js-save-btn').attr('onclick', 'javascript:void(0)');

    if (!currentTabLink.hasClass('disable-item')) {
      $('#js-save-btn').addClass('disable-item');
    }
  }
}

$( function() {
  reAdjust();
  disableSaveBtn($('.nav-link.active'));
})

$(document).on('click', '.nav-link', function() {
  disableSaveBtn($(this));
})

$(window).on('resize', function(e) {  
  reAdjust();
});

$('.tabs-scroller.float-right').click(function() {
  
  $('.tabs-scroller.float-left').fadeIn('slow');
  $('.tabs-scroller.float-right').fadeOut('slow');
  
  $('.ks-tabs').animate({left:"+="+widthOfHidden()+"px"}, 'slow', function(){

  });
});

$('.tabs-scroller.float-left').click(function() {
  
	$('.tabs-scroller.float-right').fadeIn('slow');
	$('.tabs-scroller.float-left').fadeOut('slow');
  
  	$('.ks-tabs').animate({left:"-="+getLeftPosi()+"px"}, 'slow', function(){
  	
  	});
});    