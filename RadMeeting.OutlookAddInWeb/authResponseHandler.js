function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

$(document).ready(function () {

    var code = getParameterByName('code');
    var session_state = getParameterByName('session_state');

    if (code) {

        var requestUri = 'https://login.microsoftonline.com/common/oauth2/token'

        $.ajax({
            type: "POST",
            url: requestUri,
            dataType: 'json',
            data: {
                'client_id': configs.appId,
                'code': code,
                'redirect_uri': configs.redirectUri,
                'resource': configs.source,
                'client_secret': configs.clientSecret
            },
            beforeSend: function (xhr) {
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded')
                xhr.setRequestHeader('grant_type', 'authorization_code')
            },
            success: function (data) {
                console.log(data);
            }, error: function (xhr, ajaxOptions, throwError) {
                console.log(throwError);
            }
        });
    }
});


