$import("/studio/scripts/ui.menu.js");
// 全局参数设定
var Setting = {
    User: null,
    Site: null,
    // 新消息提醒
    NewMessageTip: null,
    // 自动更新信息
    "UpdateMoney": function () {
        var obj = $("header-userinfo");
        var delay = 500;
        if (obj != null) {
            delay = 10 * 1000;
            var t = obj.getBindEvent();
            if (t != null) t.fire("checklogin,updateusermoney,showmoney,userinfo-messagetip");
        }
        if (Setting.UpdateMoney) Setting.UpdateMoney.delay(delay);
    },
    // 显示倒计时
    "UpdateTimer": function (obj) {
        if (obj.get("tag") == "input") return;
        var time = obj.get("data-timer").toInt();
        if (time < 0) time = 0;
        var timer = time.ToString("HH:mm:ss");
        var list = obj.getElements("em");
        if (list.length == 0) {
            for (var i = 0; i < timer.length; i++) {
                var em = new Element("em");
                em.inject(obj);
                list.push(em);
            }
        }
        for (var i = 0; i < timer.length; i++) {
            list[i].set("class", timer.charAt(i) == ":" ? "colons" : "t" + timer.charAt(i));
        }
    },
    // 系统通知
    "Nofify": function () {
        new Request.JSON({
            "url": "/handler/user/info/notify",
            "onComplete": function () {
                Setting.Nofify.delay(5 * 1000);
            },
            "onSuccess": function (result) {
                if (!result.success || result.info.list.length == 0) return;
                var tip = $("notify-tip");
                if (tip == null) {
                    tip = new Element("div", {
                        "id": "notify-tip",
                        "class": "notify-tip",
                        "html": " <div class=\"notify-title\">系统通知</div>"
                    });
                    new Element("a", {
                        "href": "javascript:",
                        "class": "close",
                        "events": {
                            "click": function () {
                                tip.fade("out");
                                tip.dispose.delay(500, tip);
                            }
                        }
                    }).inject(tip);
                    new Element("div", {
                        "class": "notify-content"
                    }).inject(tip);

                    var closeTip = function () {
                        if (tip.get("data-delay")) {
                            tip.set("data-delay", null);
                            closeTip.delay(10 * 1000);
                            return;
                        }
                        tip.fade("out");
                        tip.dispose.delay(500, tip);
                    };
                    closeTip.delay(5 * 1000);
                }

                var content = tip.getElement(".notify-content");
                result.info.list.each(function (item) {
                    new Element("p", {
                        "html": item.Message.replace(/¥[\-\d\.,]+/igm, function (value, number) {

                            if (value.indexOf("-") != -1) {
                                return "<span class=\"red\">" + value + "</span>";
                            } else {
                                return "<span class=\"green\">" + value.replace("¥", "¥+") + "</span>";
                            }
                        }),
                        "class": item.Type
                    }).inject(content, "top");
                });

                tip.inject(document.body, "bottom");
                tip.set("data-delay", 1);
                UI.Sound("/studio/sound/notify.mp3");


            }
        }).post();
    }
};

// ============== 全局回调函数 ==============

BW.callback["header-userinfo"] = function (result) {
    var t = this;
    if (!result.success) return;
    if (t.dom.element.get("data-event")) return;

    t.dom.element.set("data-event", 1);

    var link = t.dom.element.getElements("[data-submenu]");
    var menu = t.dom.element.getElements(".sub-menu");
    if (link.length <= menu.length) {
        link.each(function (item, index) {
            new UI.Menu(item, {
                "menu": menu[index],
                "width": item.get("data-width") ? item.get("data-width").toInt() : 200
            });
        });
    }

    !function () {
        var sound = t.dom.element.getElement(".sound");
        if (!sound || !UI.SoundState) return;
        var state = UI.SoundState();
        if (!state) sound.addClass("off");

        sound.addEvent("click", function (e) {
            if (UI.SoundSwitch()) {
                sound.removeClass("off");
            } else {
                sound.addClass("off");
            }
        });
    }();
};

// 头部的主题切换按钮
BW.callback["common-header-theme"] = function () {
    var t = this;
    var link = t.dom.element.getElement("a.theme");
    var header = t.dom.element.getElement(".header-userinfo");
    if (!link || !header) return;

    if (localStorage.getItem("THEME")) {
        document.body.getParent("html").addClass(localStorage.getItem("THEME"));
    }

    var submenu = new Element("div", {
        "class": "sub-menu theme-background"
    });

    for (var i = 0; i < 6; i++) {
        new Element("a", {
            "href": "javascript:",
            "class": "theme-background-" + i,
            "data-theme": "theme-bg-" + i,
            "events": {
                "click": function () {
                    var theme = this.get("data-theme");
                    var html = document.body.getParent("html");
                    for (var n = 0; n < 6; n++) {
                        html.removeClass("theme-bg-" + n);
                    }
                    html.addClass(theme);
                    localStorage.setItem("THEME", theme);

                }
            }
        }).inject(submenu);
    }

    submenu.inject(header);

    new UI.Menu(link, {
        "menu": submenu,
        "width": 230,
        "x": -190,
        "y": 10
    });
};

// 金额变化的动画效果 加载当前用户对象
BW.callback["showmoney"] = function (result) {
    if (!result.success) return;
    var t = this;
    Setting.User = result.info;
    if (Setting.Site) {
        Setting.User.InviteRebate = Setting.Site.Setting.IsSameRebate == "true" || Setting.User.Rebate == Setting.Site.Setting.MinRebate ? Setting.User.Rebate : Setting.User.Rebate - 2
    } else {
        Setting.User.InviteRebate = Setting.User.Rebate - 2;
    }

    t.dom.element.getElements(".money-show[data-money]").each(function (item) {
        if (item.get("html") == "") item.set("html", "<em class=\"n0\"></em><em class=\"dot\"></em><em class=\"n0\"></em><em class=\"n0\"></em>");
        var money = item.get("data-money").toFloat();
        if (isNaN(money)) return;
        money = money.ToString("n");
        var em = item.getElements("em");
        for (var i = 0; i < em.length - money.length; i++) {
            em[i].dispose();
        }
        for (var i = 0; i < money.length - em.length; i++) {
            new Element("em").inject(item, "top");
        }
        em = item.getElements("em");
        var length = money.length;
        for (var i = length; i > 0; i--) {
            var n = money.charAt(i - 1);
            var obj = em[i - 1];
            var className = n == "." ? "dot" : "n" + n;
            obj.set.delay((length - i) * 100, obj, ["class", className]);
        }
    });

    t.dom.element.getElements("[data-name]").each(function (item) {
        var name = item.get("data-name");
        if (result.info[name]) {
            item.set("html", result.info[name]);
        }
    });

    if (BW.Task) BW.Task.setTaskbar();

    document.body.set("data-user-type", result.info.Type);

}

// 更新用户的余额
BW.callback["updateusermoney"] = function (result) {
    var t = this;
    t.dom.element.getElements("[data-money-field]").each(function (item) {
        var name = item.get("data-money-field");
        if (result.info[name] != undefined) {
            item.set("data-money", result.info[name]);
        }
    });
}

// 检查当前用户是否登录
BW.callback["checklogin"] = function (result) {
    var t = this;
    if (!result.success) {
        Setting.UpdateMoney = null;
        new BW.Tip(result.msg, {
            "callback": function () {
                location.href = "index.html";
            }
        });
    }
}

// 站内信新消息提示
BW.callback["userinfo-messagetip"] = function (result) {
    var t = this;
    if (!result.success) return;
    var newMessage = result.info.NewMessage.toInt();

    var isChange = false;
    if (Setting.NewMessageTip != newMessage) {
        isChange = true;
        Setting.NewMessageTip = newMessage;
    }

    if (isChange && newMessage > 0) {
        UI.Sound("/studio/sound/message.mp3");
    }

    $$("[data-newmessage=1]").each(function (item) {
        item.set("text", newMessage);
        item.fade(newMessage > 0 ? "show" : "hide");
    });
}

// 检查站点信息
BW.callback["checksiteinfo"] = function (result) {
    if (result.success) {
        Setting.Site = result.info;
    }
}

// 加载彩票
BW.callback["frame-lottery"] = function (result) {
    var t = this;
    if (!result.success) return;
    if (result.info.category && result.info.category.length > 0) {
        t.dom.element.addClass("frame-lottery-category");
        result.info.category.each(function (item) {
            var category = new Element("div", {
                "class": "lottery-category"
            });
            new Element("div", {
                "class": "lottery-category-title",
                "html": "<label class=\"${Code}\">${Name}</label>".toHtml(item)
            }).inject(category);

            var content = new Element("div", {
                "class": "lottery-category-content"
            });

            result.info.list.filter(function (t) { return t.CateID == item.ID; }).each(function (t) {
                if (t.Game == "Second") t.Category = t.Game;
                new Element("div", {
                    "class": "lottery-category-item",
                    "data-name": t.Game,
                    "data-game": t.Game,
                    "data-title": t.Name,
                    "data-src": "lottery/" + t.Category.toLowerCase() + ".html",
                    "data-category": t.Category.toLowerCase(),
                    "data-post": "Game=" + t.Game,
                    "html": "<img src=\"/images/space.gif\" class=\"lottery-icon lottery64 ${Game}\"><label class=\"lottery-name\">${Name}</label>".toHtml(t),
                    "events": {
                        "click": function () {
                            BW.OpenFrame(this);
                        }
                    }
                }).inject(content);
            });
            content.inject(category);
            category.inject(t.dom.element);
        });

        return;
    }
    new BW.Lottery(t.dom.element, {
        "data": result.info.list || result.info
    });
}

// 左侧绑定图标点击打开事件
BW.callback["frame-open"] = function (result) {
    var t = this;
    t.dom.element.getElements("[data-name]").addEvent("click", function () {
        BW.OpenFrame(this);
    });
}

// 绑定tab切换元素
BW.callback["frame-tab"] = function (result) {
    var t = this;

    t.dom.container.getElements("[data-tab-container]").each(function (item) {
        var list = item.getElements("[data-tab-item],a[data-bind-action]");
        var current = list.filter(function (obj) { return obj.hasClass("current"); }).getLast();
        list.addEvent("click", function () {
            if (current == this) return;
            if (current != null) current.removeClass("current");
            current = this;
            current.addClass("current");
        });
        var auto = false;
        (function () {
            var tabName = t.dom.element.get("data-frame-tab");
            if (tabName) {
                var tab = list.filter(function (obj) { return obj.get("data-tab-item") == tabName || obj.get("text") == tabName }).getLast();
                if (tab) {
                    auto = true;
                    tab.fireEvent.delay(100, tab, ["click"]);
                }
            }
            if (!auto) {
                list[0].fireEvent.delay(100, list[0], ["click"]);
            }
        }).delay(100);
    });
};

// 弹出窗口的form提交事件
BW.callback["diag-form"] = function (result) {
    var t = this;
    new BW.Tip(result.msg, {
        "callback": function () {
            if (result.success) {
                var diag = t.dom.element.getParent(".diag");
                var name = diag.get("data-name");
                BW.diagObj[name].target.Close.delay(500, BW.diagObj[name].target);
            }
        }
    })
}

// 标识元素加载已经完成
BW.callback["loading-complete"] = function (result) {
    var t = this;
    t.dom.element.set("data-loading-complete", "1");
}

// 绑定日期选择控件
BW.callback["ui-calendar"] = function (result) {
    var t = this;
    if (!window["UI"] || !UI.Calendar) return;
    t.dom.container.getElements("input[data-type=calendar]").each(function (item) {
        new UI.Calendar(item);
        item.set("readonly", "true");
        if (item.get("data-today") && item.get("value") == "") {
            item.set("value", new Date().ToShortDateString());
        }
    });
};

// 载入第三方游戏
BW.callback["frame-game-enter"] = function (result) {
    var t = this;
    var game = t.dom.element.get("data-game");

    var btn = t.dom.element.getElement(".game-enter");
    var href = "diag/game-" + game.toLowerCase() + "-entry.html";
    if (t.dom.element.hasClass("fish")) href += "?key=6";

    // 未开户
    if (!result.success) {
        btn.addEvent("click", function () {
            if (btn.hasClass("game-register")) return;
            new BW.Tip("您暂未开户，是否立即开户？", {
                "type": "confirm",
                "callback": function () {
                    new Request.JSON({
                        "url": "/handler/game/gateway/register",
                        "onRequest": function () {
                            btn.addClass("game-register");
                            btn.set("disabled", true);
                        },
                        "onComplete": function () {
                            btn.removeClass("game-register");
                            btn.set("disabled", false);
                        },
                        "onSuccess": function (regResult) {
                            new BW.Tip(regResult.msg, {
                                "callback": function () {
                                    if (regResult.success) {
                                        btn.removeEvents("click");
                                        btn.set({
                                            "href": href,
                                            "target": "_blank"
                                        });
                                    }
                                }
                            });
                        }
                    }).post({
                        "Type": game
                    });
                }
            });
        });
    } else {
        btn.removeEvents("click");
        btn.set({
            "href": href,
            "target": "_blank"
        });
    }

};

// 充值的快捷按钮
BW.callback["frame-person-recharge"] = function (result) {
    var t = this;
    var tab = null;
    var loadTab = function () {
        tab = t.dom.container.getElement(".frame-info-tab [data-bind-action=info/person-recharge.html]");
        if (tab == null) {
            loadTab.delay(100);
            return;
        }
        tab.fireEvent.delay(1000, tab, ["click"]);
    };
    loadTab.apply(t);
};

// 提现的快捷按钮
BW.callback["frame-person-withdraw"] = function (result) {
    var t = this;
    var tab = null;
    var loadTab = function () {
        tab = t.dom.container.getElement(".frame-info-tab [data-bind-action=info/person-withdraw.html]");
        if (tab == null) {
            loadTab.delay(100);
            return;
        }
        tab.fireEvent.delay(1000, tab, ["click"]);
    };
    loadTab.apply(t);
};

// 转账的快捷按钮
BW.callback["frame-person-transfer"] = function (result) {
    var t = this;
    var tab = null;
    var loadTab = function () {
        tab = t.dom.container.getElement(".frame-info-tab [data-bind-action=info/person-transfer.html]");
        if (tab == null) {
            loadTab.delay(100);
            return;
        }
        tab.fireEvent.delay(1000, tab, ["click"]);
    };
    loadTab.apply(t);
};

// 优惠活动的快捷按钮
BW.callback["frame-person-activity"] = function (result) {
    var t = this;
    var tab = null;
    var loadTab = function () {
        tab = t.dom.container.getElement(".frame-info-tab [data-bind-action=info/news-activity.html]");
        if (tab == null) {
            loadTab.delay(100);
            return;
        }
        tab.fireEvent.delay(100, tab, ["click"]);
    };
    loadTab.apply(t);
};

// 弹出新闻提示
BW.callback["news-tip"] = function (result) {
    var t = this;
    if (!result.success) return;
    var id = result.info.ID || result.info.data.Tip;
    if (!id || id == "0") return;
    new BW.Diag(this, {
        "name": "info-news-detail",
        "type": "control",
        "src": "info/news-detail.html",
        "mask": true,
        "title": "查看公告",
        "data": "id=" + id
    });
};

// 退出登录
BW.callback["header-logout"] = function (result) {
    var t = this;
    if (result.success) {
        document.body.setStyle("display", "none");
        location.href = "index.html";
    }
};

// ============== 全局初始化函数 ==============

// 绑定日期选择控件
BW.load["ui-calendar"] = function () {
    var t = this;
    if (!window["UI"] || !UI.Calendar) return;
    t.dom.element.getElements("input[data-type=calendar]").each(function (item) {
        new UI.Calendar(item);
    });
};

// 绑定随机选择的按钮事件
BW.load["frame-pt-random"] = function () {
    var t = this;
    var random = t.dom.element.getNext(".slot-randomlist");
    var btn = random.getElement(".slot-random-submit");
    var list = random.getElements("li");
    btn.addEvent("click", function (e) {
        if (random.hasClass("loading")) return;

        new Request.JSON({
            "url": "/handler/game/gateway/gamelistrandom",
            "onRequest": function () {
                random.addClass("loading");
            },
            "onComplete": function () {
                random.removeClass("loading");
            },
            "onSuccess": function (result) {
                list.each(function (item, index) {
                    var info = result.info.list[index];
                    item.empty();
                    var obj = new Element("a", {
                        "href": "javascript:",
                        "html": "<img src=\"images/space.gif\" style=\"background-image:url(${Cover});\" data-game=\"${Code}\"><label>${Name}</label>".toHtml(info)
                    }).inject(item);
                });
            }
        }).post({
            "Type": "PT",
            "Count": 3
        });
    });
};

// 进入PT游戏
BW.load["frame-pt-game"] = function () {
    var t = this;
    !function () {
        if ($(document.body).getStyle("overflow") != "hidden") return;
        var height = UI.getSize().height;
        t.dom.element.setStyle("height", height - 300);
    }();

    var searchbox = $(t.dom.element.get("data-bind-search"));
    var form = {
        "Type": searchbox.getElement("[name=Type]"),
        "Category": searchbox.getElement("[name=Category]")
    };
    searchbox.addEvent("click", function (e) {
        var obj = $(e.target);
        if (obj.get("data-type")) {
            var typeList = searchbox.getElements("a[data-type]");
            typeList.each(function (item) {
                item.removeClass("current");
            });
            form["Type"].set("value", obj.get("data-type"));
            form["Category"].set("value", null);
            obj.addClass("current");
            t.fire();
        }
        if (obj.get("data-category") != null) {
            var current = obj.getParent().getElement("[data-category].current");
            if (current != null) current.removeClass("current");
            obj.addClass("current");
            form["Category"].set("value", obj.get("data-category"));
            t.fire();
        }
    });

    t.dom.element.addEvent("click", function (e) {
        var obj = $(e.target);
        var game = obj.get("data-game");
        if (!game) return;

        var type = form["Type"].get("value");
        if (type == "BBIN") game = "game";
        new BW.Diag(obj, {
            "name": "game-enter",
            "type": "control",
            "src": "controls/game-enter.html",
            "title": "进入游戏",
            "mask": true,
            "width": 420,
            "height": 360,
            "data": "Type=" + type + "&Game=" + game
        });
    });
};

// 设定老虎机游戏的默认第一个类型
BW.callback["frame-slot-type"] = function (result) {
    var t = this;
    var current = t.dom.element.getElement("[data-type]");
    if (!current) return;
    var form = t.dom.element.getParent("form");
    if (!form) return;
    var type = form.getElement("[name=Type]");
    if (!form) return;

    type.set("value", current.get("data-type"));
    current.addClass("current");
};

// 设置分类绑定信息
BW.callback["frame-slot-category"] = function (result) {
    var t = this;
    var searchbox = $(t.dom.element.get("data-bind-search"));
    var category = searchbox.getElement("[name=Category]");
    var categorylist = searchbox.getElement(".category-list");
    var type = searchbox.getElement("[name=Type]").get("value");
    categorylist.empty();
    new Element("a", {
        "href": "javascript:",
        "text": "全部",
        "data-category": "",
        "class": result.info.data.Category == "" ? "current" : ""
    }).inject(categorylist);
    result.info.data.List.each(function (item) {
        new Element("a", {
            "href": "javascript:",
            "text": item,
            "data-category": item,
            "class": result.info.data.Category == item ? "current" : ""
        }).inject(categorylist);
    });

};

// PT游戏列表的翻页
BW.callback["frame-pt-pagesplit"] = function (result) {
    var t = this;
    var frame = t.dom.element.getParent(".frame-pt");
    var pre = frame.getElement(".page-pre");
    var next = frame.getElement(".page-next");

    var pageindex = result.info.PageIndex.toInt();
    var maxpage = result.info.RecordCount.toInt() % result.info.PageSize.toInt() == 0 ?
        result.info.RecordCount.toInt() / result.info.PageSize.toInt() :
        Math.floor(result.info.RecordCount.toInt() / result.info.PageSize.toInt()) + 1;

    pre.fade(pageindex == 1 ? "hide" : "in");
    next.fade(pageindex == maxpage ? "hide" : "in");

    if (!t.dom.element.get("data-page")) {
        t.dom.element.set("data-page", true);
        [pre, next].each(function (item) {
            item.addEvent("click", function () {
                var action = this.get("data-page").toInt();
                pageindex += action;
                pageindex = Math.max(1, Math.min(maxpage, pageindex));
                t.options.pageindex = pageindex;
                t.fire();
            });
        });
    }
};

// 绑定全局参数（Setting变量）
BW.callback["setting-fill"] = function (result) {
    var t = this;
    t.dom.container.getElements("[data-setting-fill]").each(function (item) {
        var html = item.get("html");
        item.set("html", html.toHtml(Setting));
    });
};

// 列表绑定的时候状态的显示
htmlFunction["Status"] = function (value) {
    switch (value) {
        case "true":
        case "1":
            value = "<span class=\"green\">可用</span>";
            break;
        case "false":
        case "0":
            value = "<span class=\"red\">不可用</span>";
            break;
    }
    return value;
};

// 金额的显示
htmlFunction["money"] = function (value) {
    return "￥" + value.toFloat().ToString("n") + "元";
};

// 是否在线
htmlFunction["online"] = function (value) {
    var str = value;
    switch (value) {
        case "true":
        case "1":
            str = "<span class=\"green\">在线</span>";
            break;
        case "false":
        case "0":
            str = "<span class=\"gray\">离线</span>";
            break;
    }
    return str;
};

// 奖金的显示
htmlFunction["money-reward"] = function (value) {
    value = value.toFloat();
    if (value == 0) return "";
    return "<span class=\"green\">" + htmlFunction["money"](value) + "</span>";
}


// =================  系统加载事件 =================
window.addEvent("domready", function () {
    Setting.UpdateMoney();

    $("container").setStyle("height", UI.getSize().y);

    // 游戏类型切换
    !function () {
        var current = null;
        var list = $$("#container .frames-type a[data-frame-type]");
        var body = $(document.body);
        if (list.length == 0) {
            var target = $("frame-lottery");
            if (!target) return;
            target.setStyle("display", "block");
            BW.Bind(target).fire();
        } else {
            list.addEvent("click", function () {
                if (current != null) current.removeClass("current");
                this.addClass("current");
                current = this;
                body.set("class", current.get("data-frame-type"));

                var target = $(this.get("data-frame-type"));
                if (target == null) return;
                if (!target.get("data-bind-action-delay")) {
                    target = target.getElement("[data-bind-action-delay]");
                }
                if (target == null) return;

                target.set({
                    "data-bind-action": target.get("data-bind-action-delay"),
                    "data-bind-action-delay": null
                });
                console.log(this);
                BW.Bind(target).fire();

            });
            list[0].fireEvent("click");
        }
    }();

    // 获取电脑客户端下载路径
    !function () {
        var appPC = function () {
            var obj = $("app-pc");
            if (obj == null) return;
            if (!Setting.Site || !Setting.Site.ID) {
                appPC.delay(500);
                return;
            }

            if (Setting.Site.Setting.APPPC) {
                obj.set("href", Setting.Site.Setting.APPPC);
            } else {
                obj.dispose();
            }

        };
        appPC.apply();
    }();

    Setting.Nofify();
});

// 绑定输入金额的控件
var bindMoneyUpper = function (obj) {
    obj.addEvents({
        "keydown": function (e) {
            var regex = /[0-9]|(backspace)/;
            var key = e.key;
            if (!regex.test(key)) {
                e.stop();
            }
        },
        "keyup": function () {
            var upper = this.get("data-upper");
            if (upper != null) upper = $(upper);
            if (upper != null) {
                upper.set("text", this.get("value").toCurrency());
            }
        }
    });
};

// 设置选中样式
var setCurrent = function (list, obj, className) {
    if (!className) className = "current";
    if (obj.hasClass(className)) return;
    list.each(function (item) {
        if (item != obj) {
            item.removeClass(className);
        } else {
            item.addClass(className);
        }
    });
};

