window.addEvent("domready", function () {
    var bg = chrome.extension.getBackgroundPage();

    //给表单赋值
    !function () {
        Config = bg.GetConfig();

        $$("input[name]").each(function (item) {
            var name = item.get("name");
            if (Config[name]) {
                item.set("value", Config[name]);
            }
        });
    }();

    // 保存远程网关
    !function () {
        $("btnSave").addEvent("click", function (e) {
            var checkInput = true;
            $$("input[name]").each(function (item) {
                if (!checkInput) return;
                if (item.get("value") == "") {
                    alert("请输入" + item.get("placeholder"));
                    checkInput = false;
                }
                Config[item.get("name")] = item.get("value");
            });

            if (!checkInput) return;

            var url = "http://" + Config["Gateway"] + "/handler/payment/AlipayAccount";
            new Request.JSON({
                "url": url,
                "onRequest": function () {
                    document.body.addClass("loading");
                },
                "onComplete": function () {
                    document.body.removeClass("loading");
                },
                "onError": function (msg) {
                    alert(msg);
                    document.body.removeClass("loading");
                },
                "onFailure": function (xhr) {
                    alert("发生错误:" + xhr.statusText);
                },
                "onSuccess": function (result) {
                    if (result.success) {
                        alert("保存成功");
                        bg.SetConfig(Config);
                    } else {
                        alert(result.msg);
                    }
                }
            }).post(Config);
        });
    }();

});