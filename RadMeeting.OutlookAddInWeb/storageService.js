
var storageService = {};
storageService.keys = {
    accessToken: 'accessToken',
    authState: 'authState',
    authNonce: 'authNonce',
    idToken: 'idToken',
    tokenExpires: 'tokenExpires',
    userDisplayName: 'userDisplayName',
    userSigninName: 'userSigninName',
    userDomainType: 'userDomainType',
}

storageService.getItem = function (key) {
    return getCookie(key);
};

storageService.setItem = function (key, value) {
    setCookie(key, value);
}


storageService.clear = function () {
    deleteAllCookies();
};

storageService.removeItem = function (key) {
    setCookie(key, '', -1);
};

function setCookie(cname, cvalue, exdays) {

    if (!exdays) exdays = 7;
    var d = new Date();

    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function deleteAllCookies() {
    var cookies = document.cookie.split(";");

    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i];
        var eqPos = cookie.indexOf("=");
        var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
        document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }
}