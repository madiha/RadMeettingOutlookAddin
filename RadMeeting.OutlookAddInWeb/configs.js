var configs = {
    authEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/authorize?',
    redirectUri: location.protocol + '//' + location.host + '/authorize', 
    redirectHiddenUri: location.protocol + '//' + location.host + '/authorizeHidden.html',
    appId: 'e9f886a6-dec6-4563-bfb2-b05b8054d758',
    scopes: 'openid profile User.ReadWrite User.ReadBasic.All Sites.ReadWrite.All Contacts.ReadWrite People.Read Notes.ReadWrite.All Tasks.ReadWrite Mail.ReadWrite Files.ReadWrite.All Calendars.ReadWrite',
    tenentId: '9188040d-6c67-4c5b-b112-36a304b66dad',
    source: 'https://graph.microsoft.com',
    domainType: {
        consumers: 'consumers',
        organizations: 'organizations'
    },
    graphApi: {
        endPoints: {
            getUsers: 'https://graph.microsoft.com/v1.0/users'
        }
    }
};