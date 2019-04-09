var sideBar;

$(document).ready(function() {
  
  $('.retina').retina();
  
  // Sticky
  sticky();

  function sticky() {
    $('#header').sticky({
      topSpacing: 0,
      zIndex: 99,
    });
  }

  

  // Sidebar
  $('#btn_menu').on('click', function(event) {
    event.preventDefault();
    if (!$(this).hasClass('active')) {
      $('#sideBar').stop().fadeIn(function() {
        $(this).addClass('active');
      });
      $(this).addClass('active');
    } else {
      $('#sideBar').stop().fadeOut(function() {
        $(this).removeClass('active');
      });
      $(this).removeClass('active');
    }
  });
  $('#sideBar').on('click', '.btn_close', function (event) {
    event.preventDefault();
    $('#sideBar').stop().fadeOut(function () {
      $(this).removeClass('active');
    });
    $('#btn_menu').removeClass('active');
  });

  // Support menu
  $('.opener').on('click', function() {
    $('.all-menu').fadeToggle('fast');
    $('span', this)
      .toggleClass('glyphicon-menu-up')
      .toggleClass('glyphicon-menu-down');
  });

  
  // Captcha Size
  if($('.captcha').length > 0) {
    scaleCaptcha();
  }

  // Member Level
  $('#swich_instructions').on('click', function() {
    $(this).toggleClass('active');
    $('#level_instructions').stop().slideToggle();
  });

  $('.ajax-popup-link').magnificPopup({
    type: 'ajax'
  });


  // Back to top
  if($('#backTop').length > 0) {
    $('#backTop').on('click', function(event) {
      event.preventDefault();
      $('html, body').stop().animate({
        scrollTop: 0
      }, 500, 'swing');
    });
    $(window).on('scroll', function () {
      if ($(window).scrollTop() > 100) {
        $('#backTop').stop().fadeIn(400, function() {
          $(this).css('display', 'block');
        });
      } else {
        $('#backTop').stop().fadeOut(400);
      }
    });
  }

  // Block Link
  $('a[href*="#"]')
    .not('[href="#"]')
    .not('[href="#0"]')
    .on('click', function (event) {
    if (
      location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') &&
      location.hostname == this.hostname
    ) {
      var hash = $(this).attr('href');
      var hashName = hash.split('#')[1];
      var $target = $('section[data-anchor="' + hashName + '"]');

      if ($target.length > 0) {
        $('html, body').stop().animate({
          scrollTop: $target.offset().top - $('#header').outerHeight()
        }, 800, 'swing', function(e) {
          window.location.hash = hash;
        });
      }
    }
  });
});

// Resize
var windowWidth = $(window).width();
$(window).on('resize', function() {
  if (windowWidth != $(window).width()) {
    $('#header').unstick();
    sticky();

    if($('.captcha').length > 0) {
      scaleCaptcha();
    }

    windowWidth = $(window).width();
  }
});

// Load
$(window).on('load', function() {
  // Link
  var url = window.location.toString();
  var hashName = url.split('#')[1];
  var $target = $('[data-anchor="' + hashName + '"]');

  if( $target.length > 0 ) {
    // Reset where animation starts.
    $('html, body').scrollTop(0);
    // Animate to
    $('html, body').stop().animate({
      scrollTop: $target.offset().top - $('#header').outerHeight()
    }, 800);
  }
});

// Hashchange
$(window).on('hashchange', function () {
  var hash = window.location.hash;
  var hashName = hash.split('#')[1];
  var $target = $('section[data-anchor="' + hashName + '"]');

  if( $target.length > 0 ) {
    $('html, body').stop().animate({
      scrollTop: $target.offset().top - $('#header').outerHeight()
    }, 800);
  }
});





//use width google reCaptcha
function scaleCaptcha() {
  var reCaptchaWidth = 304;
  var containerWidth = $('.captcha').width();

  if (reCaptchaWidth > containerWidth) {
    var captchaScale = containerWidth / reCaptchaWidth;
    $('.captcha_inner').css({
      'transform': 'scale(' + captchaScale + ')'
    });
  } else {
    $('.captcha_inner').css({
      'transform': ''
    });
  }
}