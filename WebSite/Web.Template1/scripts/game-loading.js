// 游戏页面的加载效果
(function () {
    var loading = $("loading-page");
    var list = $$("[data-bind-action]");
    if (list.length == 0) { loading.dispose(); return; }

    // 获取当前的进度
    var getProcess = function () {
        return list.filter(function (item) { return item.get("data-loading-complete") != null; }).length;
    };

    var width = loading.getSize().x / 10;
    var height = loading.getSize().y / 10;
    var blockList = new Array();
    for (var i = 0; i < 10; i++) {
        for (var n = 0; n < 10; n++) {
            var block = new Element("i", {
                "styles": {
                    "top": i * height,
                    "left": n * width,
                    "width": width,
                    "height": height,
                    "background-size": loading.getSize().x + "px " + loading.getSize().y + "px",
                    "background-position": (n * width * -1) + "px " + (i * height * -1) + "px"
                }
            });
            blockList.push(block);
            block.inject(loading);
        }
    }

    var timer = setInterval(function () {
        var index = Math.round(getProcess() / list.length * 100);
        for (var i = 0; i < index; i++) {
            if (blockList.length > i && !blockList[i].hasClass("hide")) {
                blockList[i].addClass("hide");
                blockList[i].setStyles.delay(i * 5, blockList[i], [{
                    "margin-left": Math.floor(Math.random() * width * 10 * (Math.random() > 0.5 ? 1 : -1)),
                    "margin-top": Math.floor(Math.random() * height * 10 * (Math.random() > 0.5 ? 1 : -1)),
                    "width": 0,
                    "height": 0
                }]);
            }
        }

        if (index >= 100) {
            clearInterval(timer);
            loading.addClass("finish");
            loading.dispose.delay(1500, loading, ["finish"]);

            // 加载自定义事件
            !function () {
                var url = location.href;
                if (!url.contains("#")) return;
                var type = url.substr(url.indexOf("#") + 1);
                switch (type) {
                    case "news":
                        var aside = $$("aside [data-name=News]").getLast();
                        if (aside) aside.click();
                        break;
                    default:
                        var frame = $("container").getElement(".frames-type [data-frame-type=frame-" + type + "]");
                        if (frame) frame.click();
                        break;
                }
            }();
        }
    }, 100);
})();