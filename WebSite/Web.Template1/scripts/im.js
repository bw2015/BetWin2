(function () {
    var loadServer = function () {
        if (!Setting || !Setting.Site || !window["IM"] || !Setting.User) {
            loadServer.delay(500);
            return;
        }
        IM.apply(this, [{
            "siteid": Setting.Site.ID,
            "isgroup": true,
            "notice": true,
            "server": Setting.Site.Setting.CustomerServer,
            "host": Setting.Site.Setting.ServiceServer,
            "find" : "controls/im-find.html"
        }]);
    };
    loadServer.apply();
})();