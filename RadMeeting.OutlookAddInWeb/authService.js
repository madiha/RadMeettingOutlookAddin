
var authService = {};

authService.getEmailAddress = function() {
    return Office.context.mailbox.userProfile.emailAddress;
};

authService.buildAuthUrl = function (isHidden) {
    // Generate random values for state and nonce
    storageService.setItem('authState', commonService.guid());
    storageService.setItem('authNonce', commonService.guid());
    var redirectUri = isHidden ? configs.redirectHiddenUri : configs.redirectUri;
    var authParams = {
        response_type: 'code',
        client_id: configs.appId,
        redirect_uri: redirectUri,
        scope: configs.scopes,
        state: storageService.getItem('authState') + '~' + authService.getEmailAddress(),
        nonce: storageService.getItem('authNonce'),
        response_mode: 'query'
    };
    return configs.authEndpoint + $.param(authParams);
};


authService.parseHashParams = function (hash) {
    var params = hash.split('&');

    var paramarray = {};
    params.forEach(function (param) {
        param = param.split('=');
        paramarray[param[0]] = param[1];
    });

    return paramarray;
};


    

authService.handleTokenResponse = function (hash, callback) {

    // If this was a silent request remove the iframe
    $('#auth-iframe').remove();

    var tokenresponse = authService.parseHashParams(hash);

    // Check that state is what we sent in sign in request
    if (tokenresponse.state != storageService.getItem('authState')) {
        storageService.removeItem('authState');
        storageService.removeItem('authNonce');

        if (callback) {
            callback(undefined, 'The state in the authorization response did not match the expected value. Please try signing in again.');
            return;
        }

        // Report error
        callback(undefined, 'title=Invalid state&error_description=The state in the authorization response did not match the expected value. Please try signing in again.');
        return;
    }

    storageService.setItem('authState', '');
    storageService.setItem('accessToken', tokenresponse.access_token);

    // Get the number of seconds the token is valid for,
    // Subract 5 minutes (300 sec) to account for differences in clock settings
    // Convert to milliseconds
    var expiresin = (parseInt(tokenresponse.expires_in) - 300) * 1000;
    var now = new Date();
    var expireDate = new Date(now.getTime() + expiresin);
    storageService.setItem('tokenExpires', expireDate.getTime());
    callback(storageService.getItem('accessToken'));
};

authService.clearUserState = function () {
    // Clear session
    storageService.clear();
};

authService.makeSilentTokenRequest = function (callback) {

    try {
        // Build up a hidden iframe
        var iframe = $('<iframe/>');
        iframe.attr('id', 'auth-iframe');
        iframe.attr('name', 'auth-iframe');
        iframe.appendTo('body');
        iframe.hide();

        iframe.load(function () {
            try {
                var hash = this.contentDocument.location.hash.replace('#', '');
                if (hash.indexOf('error=login_required') !== -1) {
                    callback(undefined, 'login_required');
                    return;
                }

                authService.handleTokenResponse(hash, callback);
            } catch (error) {
                callback(undefined, 'login_required');
            }
        });

        iframe.attr('src', authService.buildAuthUrl(true) + '&prompt=none&domain_hint=' +
            storageService.getItem('userDomainType') + '&login_hint=' +
            storageService.getItem('userSigninName'));
    } catch(err) {
        callback(undefined, 'login_required');
    }
};

authService.getAccessToken = function (callback) {
    var token = storageService.getItem('accessToken');
    if (!token || token.length === 0) {
        callback(undefined, 'Token not found. Try signing in again.');
        return;
    }

    var now = new Date().getTime();
    var isExpired = now > parseInt(storageService.getItem('tokenExpires'));
    // Do we have a token already?
    if (token && !isExpired) {
        // Just return what we have
        if (callback) {
            callback(token);
        }
    } else {
        // Attempt to do a hidden iframe request
        authService.makeSilentTokenRequest(callback);
    }
};