var GAME = "ChungKing";



BW.callback["regionlist"] = function (result) {
    var t = this;
    if (!t.dom.element.get("data-list")) {
        t.dom.element.set("data-list", true);
        BW.callback["list"].apply(t, [result]);
        var list = t.dom.element.getElements("[data-id]");
        var content = $("content");
        var current = null;
        list.addEvent("click", function () {
            var id = this.get("data-id");
            if (current) current.removeClass("current");
            current = this;
            current.addClass("current");
            new Request.JSON({
                "url": "../handler/site/info/regioninfo",
                "onRequest": function () {
                    content.addClass("loading");
                },
                "onComplete": function () {
                    content.removeClass("loading");
                },
                "onSuccess": function (result) {
                    current.removeClass("new");
                    content.set("html","<p class=\"updatetime\">更新时间：" + result.info.CreateAt + "</p>" + result.info.Content);
                }
            }).post({
                "ID": id
            })
        });
        if (list.length > 0) list[0].fireEvent("click", list[0]);
    } else {

        var obj = new Object();
        t.dom.element.getElements("[data-id]").each(function (item) {
            obj[item.get("data-id")] = item;
        });

        result.info.list.each(function (item) {
            var id = item.ID;
            if (!obj[id]) return;
            if (obj[id].get("data-time") != item.TimeStamp) {
                obj[id].set("data-time", item.TimeStamp);
                obj[id].addClass("new");
                if (obj[id].hasClass("current")) {
                    obj[id].fireEvent("click", obj[id]);
                }
            } else {
                obj[id].removeClass("new");
            }
        });
    }
};

BW.callback["siteinfo"] = function (result) {
    var t = this;
    !function () {
        var domain = t.dom.element.getElement("[data-dom=domain]");
        if (!domain) return;
        domain.set("text", location.host);
    }();
};

window.addEvent("domready", function () {
    // 获取当前游戏
    !function () {
        var search = location.search;
        var regex = /^\?(\w+)/i;
        if (regex.test(search)) GAME = regex.exec(search)[1];

        new Request.JSON({
            "url": "../handler/game/lottery/gameinfo",
            "onSuccess": function (result) {
                $("icon").addClass(GAME);
                $$("[data-dom=gamename]").each(function (item) {
                    item.set("text", result.info.Name);
                });
            }
        }).post({
            "Game": GAME
        });
    }();

    // 奖期定时器
    !function () {
        var dom = new Object();
        $$("header [data-dom]").each(function (item) {
            dom[item.get("data-dom")] = item;
        });
        var query = function () {
            new Request.JSON({
                "url": "../handler/game/lottery/index",
                "onComplete": function () {
                    query.delay(1000);
                },
                "onSuccess": function (result) {
                    if (!result.success) return;
                    dom["openindex"].set("text", result.info.OpenIndex);
                    if (result.info.OpenNumber) {
                        if (dom["opennumber"].hasClass("loading")) {
                            dom["opennumber"].removeClass("loading");
                            result.info.OpenNumber.split(',').each(function (item) {
                                new Element("strong", {
                                    "text": item
                                }).inject(dom["opennumber"]);
                            });
                        }
                    } else {
                        dom["opennumber"].empty();
                        dom["opennumber"].addClass("loading");
                    }

                    dom["nextindex"].set("text", result.info.BetIndex);
                    dom["minute"].set("text", Math.floor(result.info.OpenTime / 60).toString().padLeft(2, '0'));
                    dom["second"].set("text", (result.info.OpenTime % 60).toString().padLeft(2, '0'));
                }
            }).post({
                "Game": GAME
            });
        }
        query.apply();
    }();

    // 菜单栏
    !function () {
        var menu = $("menu");
        menu.set("data-bind-data", "Name=" + GAME);
        menu.set("data-bind-action", menu.get("data-bind-action-delay"));
        var t = new BW.Bind(menu);

        setInterval(function () {
            t.fire();
        }, 10 * 1000);
    }();
});