// 客服工作平台
(function () {
    var loadServer = function () {
        if (!GolbalSetting || !GolbalSetting.Site.Setting || !window["IM"]) {
            loadServer.delay(500);
            return;
        }
        IM.apply(this, [{
            "siteid": GolbalSetting.Site.ID,
            "host": GolbalSetting.Site.Setting.ServiceServer,
            "isgroup": true,
            "uploadimage": "admin/im/uploadimage",
            "reply": true,
            "block": true,
            "chatLog": "controls/im-chatlog.html"
        }]);
    };
    loadServer.apply();
})();

// 客服主动发起会话
BW.callback["im-chat"] = function (result) {
    var t = this;
    if (!result.success) {
        new BW.Tip(result.msg);
        return;
    }

    if (!window["LAYIM"]) {
        new BW.Tip("链接客服服务器失败");
        return;
    }

    LAYIM.chat({
        name: result.info.Name,
        type: 'friend',
        avatar: result.info.FaceShow,
        id: result.info.Key
    });
};