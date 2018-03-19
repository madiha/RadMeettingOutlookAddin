var commonService = {

    guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
            s4() + '-' + s4() + s4() + s4();
    },

    createMailBoxNotification(icon, key, text) {
        Office.context.mailbox.item.notificationMessages.replaceAsync("status", {
            key: key,
            type: Office.MailboxEnums.ItemNotificationMessageType.InformationalMessage,
            icon: icon,
            message: text,
            persistent: false
        });
    }
}