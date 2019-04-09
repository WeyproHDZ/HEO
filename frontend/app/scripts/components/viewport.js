//Doing this just for window detection
jQuery.browser = {};
(function() {
    jQuery.browser.msie = false;
    jQuery.browser.version = 0;
    if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
        jQuery.browser.msie = true;
        jQuery.browser.version = RegExp.$1;
    }
})();

//SET VIEW PORT HEIGHT Functionally
function getBrowserHeight() {
    if ($.browser.msie) {
        return document.compatMode == 'CSS1Compat' ? document.documentElement.clientHeight :
            document.body.clientHeight;
    } else {
        return self.innerHeight;
    }
}

function getBrowserWidth() {
    if ($.browser.msie) {
        return document.compatMode == 'CSS1Compat' ? document.documentElement.clientWidth :
            document.body.clientWidth;
    } else {
        return self.innerWidth;
    }
}
