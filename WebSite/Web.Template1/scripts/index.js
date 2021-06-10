/// <reference path="common.js" />
!function () {
    if (!sessionStorage.getItem("PC") && /ios|android/i.test(Browser.platform)) {
        location.href = "mobile/";
        return;
    }
}();

var loginSuccess = function (diag) {
    var box = new Element("div", {
        "class": "login-success-box"
    });
    var boxList = new Array();
    for (var i = 0; i < 10; i++) {
        for (var n = 0; n < 10; n++) {
            boxList[i * 10 + n] = new Element("em", {
                "styles": {
                    "left": i * 64,
                    "top": n * 47,
                    "background-position": (i * 64 * -1) + "px " + (n * 47 * -1) + "px"
                }
            });
            boxList[i * 10 + n].inject(box);
        }
    }
    box.inject(diag);

    boxList.each(function (item) {
        item.setStyles({
            "left": Math.random() * UI.getSize().x * (Math.floor(Math.random() * 1000) % 2 == 0 ? 1 : -1),
            "top": Math.random() * UI.getSize().y * (Math.floor(Math.random() * 1000) % 2 == 0 ? 1 : -1),
            "opacity": 0
        });
    });
}

var USER_ISLOGIN = false;

BW.callback["login"] = function (result) {
    var obj = this.dom.element;
    var diag = obj.getParent(".diag");
    if (result.success) {
        diag.addClass("login-success");
        loginSuccess(diag);
        (function () {
            location.href = "game.html?token=" + new Date().getTime().toString(32);
        }).delay(1500);

    } else {
        new BW.Tip(result.msg, {
            "callback": function () {
                var code = obj.getElement("img.vcode");
                updateValidCode(code);
            }
        });
    }
}

window.addEvent("domready", function () {

    // 游戏类型的鼠标经过效果
    (function () {
        var list = $$(".menu > a[data-type]");

        list.addEvents({
            "mouseover": function () {
                var obj = this;

                list.each(function (item) {
                    if (obj != item) {
                        item.addClass("mask");
                    }
                });
            },
            "mouseout": function () {
                list.each(function (item) { item.removeClass("mask"); });
            },
            "click": function () {
                if (window["USER_ISLOGIN"]) {
                    location.href = "game.html#" + this.get("data-type");
                } else {
                    if (this.get("data-type") === "news") {
                        var list = $$(".activity-news li");
                        $("activity-news").toggleClass("show");
                        list.each(function (item, index) {
                            item.addClass.delay(index * 500, item, ["show"]);
                        });
                    } else {
                        $$("[data-diag-name=loginbox]").getLast().click();
                    }
                }
            }
        });
    })();

    // 加载站点名字
    (function () {
        new Request.JSON({
            "url": "/handler/site/info/get",
            "onSuccess": function (result) {
                GolbalSetting.Site = result.info;
                document.title = result.info.Name;
            }
        }).post();
    })();
});

BW.callback["index-activity-news"] = function (res) {
    var t = this;
};