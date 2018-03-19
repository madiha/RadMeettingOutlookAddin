var app = {};
app.adUsers = [];
app.cacheToken = undefined;
app.selectedMapAttendeeControl = undefined;
app.loadingADUserIsInProgress = false;
app.graphAccessErrorMessage = '';
app.graphAccessStatus = undefined;

app.initialize = function () {

    $(document).ready(function () {
        authService.getAccessToken(function (token, error) {
            $('#loading').hide();
            if (error) {
                $('#welcomePanel').show();
                $('#homePanel').hide();
                return;
            }

            app.cacheToken = token;
            $('#welcomePanel').hide();
            $('#homePanel').show();
            app.loadADUsers();
        });

        $('#meetingType>li').click(function () {
            $('#meetingType>li').removeClass('active');
            $(this).addClass('active');
        });

        $('#btnContinue').click(function () {
            app.openAuthenticationWindow();
        });

        $('.map-attendees', '#homePanel').click(function (event) {

            event.preventDefault();
            app.selectedMapAttendeeControl = $(this);

            $('#ad-users-modal').modal();
            $('#ad-users-modal').on('shown.bs.modal', function (e) {
                $('#user-loading-status').show();
                $('#modal-body-content-wrapper').hide();
                var timer = setInterval(function () {
                    if (!app.loadingADUserIsInProgress) {

                        if (app.graphAccessStatus === 401) {
                            $('#user-loading-status').html('You are not authorized to access Active Directory user list.').css('color','#ff3300');
                            clearInterval(timer);
                            $('#modal-body-content-wrapper').hide();
                            return;
                        }

                        $('#user-loading-status').hide();
                        $('#modal-body-content-wrapper').show();
                        app.renderADUserList();
                        clearTimeout(timer);
                    }
                }, 500);
            });
        });
    });
};

app.openAuthenticationWindow = function () {
    var authUrl = authService.buildAuthUrl();
    window.open(authUrl, "_blank", "resizable=yes, scrollbars=yes, titlebar=false, width=400, height=500, top=10, left=10");
}

app.loadADUsers = function () {

    app.loadingADUserIsInProgress = true;
    var requestUri = configs.graphApi.endPoints.getUsers;
    $.ajax({
        type: "GET",
        url: requestUri,
        dataType: 'json',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Bearer " + app.cacheToken);
        },
        success: function (data) {
            app.adUsers = data.value;
            app.loadingADUserIsInProgress = false;
        }, error: function (xhr, ajaxOptions, throwError) {
            if (xhr.status == 401) {
                app.graphAccessErrorMessage = xhr.statusText;
                app.graphAccessStatus = xhr.status;
            }

            console.log(throwError);
            app.loadingADUserIsInProgress = false;
        }
    });

}

app.renderADUserList = function (event) {

    var containerElm = $('.modal-body ul', '#ad-users-modal');
    containerElm.empty();
    $('#search-user').val('');
    $.each(app.adUsers, function (index, item) {
        var customCheckBox = $('<div>');
        customCheckBox.addClass('custom-control custom-checkbox user-item')
            .append($('<input>')
                .attr('type', 'checkbox')
                .addClass('custom-control-input')
                .attr('id', 'chk-' + index)
                .attr('data-userId', item.id))
            .append($('<label>')
                .addClass('custom-control-label')
                .attr('for', 'chk-' + index).html(item.displayName))

        containerElm.append(customCheckBox);
    });

    $('#search-user').off('keyup');
    $('#search-user').on('keyup', function () {
        var val = $(this).val().toLowerCase();
        if (!val || val.length < 3) {
            return;
        }

        $(".custom-checkbox.user-item").filter(function () {
            var userTxt = $(this).find('.custom-control-label').html();
            $(this).toggle(userTxt.toLowerCase().indexOf(val) > -1)
        });
    });

    $('#btn-select-users').off('click');
    $('#btn-select-users').on('click', function () {
        //app.selectedMapAttendeeControl
        var displayNameStr = '';
        var userIds = '';
        $(".custom-checkbox.user-item").each(function (index, item) {
            var checkbox = $(this).find('.custom-control-input')[0];
            var label = $(this).find('.custom-control-label')[0];
            if (checkbox.checked) {
                displayNameStr += $(label).html() + ', ';
                userIds += $(checkbox).attr('data-userId') + ',';
            }
        });

        if (displayNameStr !== '') {
            app.selectedMapAttendeeControl.html('(' + displayNameStr.substr(0, displayNameStr.length - 2) + ')');
            var input = app.selectedMapAttendeeControl.parent().parent().find('.user-mapping-value')[0];
            $(input).val(userIds.substr(0, userIds.length - 1));
        }

        $('#ad-users-modal').modal('hide');
    });
}


if (Office) {
    Office.initialize = function (reason) {
        app.initialize();
    };
}

if (!Office) {
    app.initialize();
}