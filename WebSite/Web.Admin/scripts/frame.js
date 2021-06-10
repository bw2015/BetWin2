/// <reference path="../../Web.Resource/scripts/mootools.js" />
/// <reference path="../../Web.Resource/scripts/mootools-more.source.js" />
/// <reference path="../../Web.Resource/scripts/betwin.source.js" />
/// <reference path="../../Web.Resource/scripts/diag.source.js" />

// 已经打开的窗口
var frames = new Object();

var Frame = new Class({
    "Implements": [Events, Options],
    "options": {
        "name": null,
        "id": null,
        "href": null
    },
    "Dom": {
        "content": null,
        "items": null,
        "taskbar": null,
        // 当前窗口
        "item": null,
        // 当前任务条
        "task": null
    },
    "initialize": function (options) {
        var t = this;
        t.setOptions(options);
        if (frames[t.options.id]) {
            frames[t.options.id].Open();
            return;
        }

        t.Dom.content = $("frame-content");
        t.Dom.items = $("frame-items");
        t.Dom.taskbar = $("frame-taskbar")

        t.Dom.item = new Element("div", {
            "class": "frame-item",
            "data-id": t.options.id,
            "data-bind-type": "control",
            "data-bind-action": t.options.href,
            "data-bind-callback": "enum,tip"
        });

        t.Dom.item.inject(t.Dom.items);

        t.Dom.task = new Element("div", {
            "class": "frame-task",
            "data-id": t.options.id,
            "events": {
                "click": function () {
                    t.Open();
                }
            }
        });

        new Element("span", {
            "text": t.options.name
        }).inject(t.Dom.task);

        new Element("a", {
            "href": "javascript:",
            "class": "am-icon-close am-icon-btn",
            "events": {
                "click": function (e) {
                    e.stopPropagation();
                    t.Close();
                }
            }
        }).inject(t.Dom.task);

        t.Dom.task.inject(t.Dom.taskbar);

        frames[t.options.id] = t;

        if (!t.Dom.taskbar.get("data-event-move")) {
            $$("#frame-taskbar-list > a").addEvent("click", function () {
                if (this.hasClass("task-left")) t.showTask("left");
                if (this.hasClass("task-right")) t.showTask("right");
            });
            t.Dom.taskbar.set("data-event-move", true);
        }
    },
    "Open": function () {
        var t = this;
        location.href = "#" + t.options.id;
        t.resize();

        t.Dom.taskbar.getElements("[data-id]").each(function (item) {
            if (item.get("data-id") == t.options.id) {
                item.addClass("current");
            } else {
                item.removeClass("current");
            }
        });
        new BW.BindEvent(t.Dom.item);
        t.taskbar();
        t.showTask();
    },
    "Close": function () {
        var t = this;
        var next = null;
        next = t.Dom.task.getNext(".frame-task");
        if (next == null) next = t.Dom.task.getPrevious(".frame-task");

        t.Dom.task.dispose();
        t.Dom.item.dispose();
        frames[t.options.id] = t = null;

        if (next != null) frames[next.get("data-id")].Open();
    },
    "resize": function () {
        var t = this;
        var size = t.Dom.content.getSize();
        var width = 0;
        var index = 0;
        var itemIndex = 0;
        Object.forEach(frames, function (value, key) {
            if (!value) return;
            width += size.x;
            value.Dom.item.setStyles({
                "width": size.x,
                "height": UI.getSize().y - 64
            });
            if (value == t) itemIndex = index;
            index++;
        });
        t.Dom.items.setStyles({
            "width": width,
            "margin-left": t.Dom.item.getAllPrevious().length * size.x * -1
        });
    },
    // 设置状态栏的位置
    "taskbar": function () {
        var t = this;
        var list = t.Dom.taskbar.getElements(".frame-task");
        t.Dom.taskbar.setStyles({
            "width": list.length * 100
        });
        var current = t.Dom.taskbar.getElement(".current");
        if (current == null) return;

    },
    // 显示状态栏任务
    "showTask": function (action) {
        var t = this;
        var taskbar = $("frame-taskbar");
        var container = taskbar.getParent();
        var containerWidth = container.getStyle("width").toInt();
        var taskbarWitdh = taskbar.getStyle("width").toInt();

        if (taskbarWitdh < containerWidth) return;
        var marginLeft = taskbar.getStyle("margin-left").toInt();
        var minLeft = containerWidth - taskbarWitdh;

        switch (action) {
            case "right":
                taskbar.setStyle("margin-left", Math.max(marginLeft - 100, minLeft));
                break;
            case "left":
                taskbar.setStyle("margin-left", Math.min(marginLeft + 100, 0));
                break;
            default:
                var current = t.Dom.taskbar.getElement(".current");
                if (current == null) return;
                var currentLeft = (current.getPosition(taskbar).x - containerWidth / 2) * -1;

                if (currentLeft > 0) currentLeft = 0;
                if (currentLeft < minLeft) currentLeft = minLeft;
                taskbar.setStyle("margin-left", currentLeft);

                break;
        }

    }
});

// 打开一个窗口
var openFrame = function (name, href, id) {
    var frame = frames[id];
    if (!frame) {
        frame = new Frame({
            "name": name,
            "href": href,
            "id": id
        });
    }
    frame.Open();
}

// 左侧的菜单栏
BW.callback["frame-aside"] = function (result) {
    var t = this;
    var obj = t.dom.container;
    document.title = result.msg;
    var menu = obj.getElement("menu");
    var list = new Array();
    result.info.Menu.each(function (menu1) {
        list.push("<h2>" + menu1.name + "</h2>");
        list.push("<dl>");
        console.log(menu1);
        menu1.menu.each(function (menu2) {
            list.push("<dt><i class=\"icon ${icon}\"></i><span>${name}</span><i class=\"am-icon-angle-right extend\"></i></dt>".toHtml(menu2));
            list.push("<dd>");
            if (!menu2.menu) {
                console.log(menu2);
            } else {
                menu2.menu.each(function (menu3) {
                    list.push("<a href=\"${href}\" data-id=\"${id}\">${name}</a>".toHtml(menu3));
                });
            }
            list.push("</dd>");
        });
        list.push("</dl>");
    });
    menu.set("html", list.join(""));



    var current = null;
    menu.getElements("a[data-id]").addEvent("click", function (e) {
        e.stop();
        if (current != null) current.removeClass("current");
        current = this;
        current.addClass("current");
        openFrame(current.get("text"), current.get("href"), current.get("data-id"));

        var menu1 = $$("menu > dl").indexOf(current.getParent("dl"));
    });

    var url = location.href;
    var currentObj = null;
    if (/#[0-9a-f]{32}$/.test(url)) {
        var id = /#([0-9a-f]{32})/.exec(url)[1];
        currentObj = menu.getElement("a[data-id=" + id + "]");
    }
    if (currentObj == null) {
        var menuList = $$("menu a");
        if (menuList.length > 0)
            currentObj = menuList[0];
    }
    if (currentObj != null)
        currentObj.click();


    // 左侧菜单的动画效果
    (function () {
        if (currentObj == null) return;
        var dd = currentObj.getParent("dd");
        var dl = dd.getParent("dl");
        var dlList = $$("menu > dl");
        new Fx.Accordion($$("menu > h2"), dlList, {
            "alwaysHide": true,
            "display": dlList.indexOf(dl),
            "duration": 100
        });
        $$("menu > dl").each(function (submenu) {
            var ddList = submenu.getElements("dd");
            new Fx.Accordion(submenu.getElements("dt"), ddList, {
                "duration": 100,
                "display": submenu == dl ? ddList.indexOf(dd) : 0,
                "onActive": function (toggler, element) {
                    toggler.addClass("active");
                },
                "onBackground": function (toggler, element) {
                    toggler.removeClass("active");
                }
            });
        });
    })();


    GolbalSetting["Site"] = result.info;

}

// 更新站点信息
BW.callback["UpdateSiteInfo"] = function (result) {
    if (result.success) {
        BW.POST("/admin/site/get", null, {
            "callback": function (getresult) {
                GolbalSetting["Site"] = Object.merge(GolbalSetting.Site, getresult.info);
            }
        });
    } else {
    }
}

// 获取系统支持的枚举，存入全局变量
BW.load["GetEnumList"] = function () {
    BW.POST("/admin/site/enumlist", null, {
        "callback": function (enumresult) {
            GolbalSetting["Enum"] = Object.merge(GolbalSetting.Enum, enumresult.info);
        }
    });
}

// 查看用户资料的回调方法
BW.callback["userinfo-callback"] = function (result) {
    var t = this;
    var obj = t.dom.container;
    var diag = obj.getParent(".diag");
    var content = diag.getElement(".userinfo-content");
    var menu = obj.getElements(".userinfo-status > a");
    var frames = obj.getElements(".userinfo-frame");
    if (menu.length != frames.length) return;

    content.setStyle("height", diag.getSize().y - 140);
    var current = null;
    menu.addEvent("click", function (e) {
        if (this == current) return;
        if (current != null) current.removeClass("current");
        current = this;
        current.addClass("current");

        var index = menu.indexOf(this);
        frames.each(function (item, itemIndex) { item.setStyle("display", index == itemIndex ? "block" : "none"); });

        var t = BW.Bind(current);
        t.fire();
    });
    menu[0].fireEvent("click");
}

// 表单提交之后的提示信息
BW.callback["form-submit-tip"] = function (result) {
    var t = this;
    new BW.Tip(result.msg, {
        "callback": function () {
            if (result.success && t.dom.element.get("tag") == "form") {
                t.dom.element.reset();
            }
        }
    });
}

// 管理员退出登录
BW.callback["admininfo-logout"] = function (result) {
    var t = this;
    $(document.body).fade("out");
    location.href = "index.html";
}

// 绑定页面上的站点参数配置信息
BW.load["setting-fill"] = function () {
    var t = this;
    t.dom.container.set("html", t.dom.container.get("html").toHtml(GolbalSetting));
}

// 输入用户名可直接看到用户的基本信息
BW.load["event-userinfo"] = function () {
    var t = this;
    t.dom.element.getElements("[data-userinfo]").addEvent("change", function () {
        var result = this.getNext(".result-info");
        if (result == null) {
            result = new Element("span", {
                "class": "result-info am-margin-left"
            });
            result.inject(this, "after");
        }
        var username = this.get("value");
        if (username == "") {
            result.empty();
            return;
        }
        new Request.JSON({
            "url": "/admin/user/getinfo",
            "onRequest": function () {
                result.empty();
                result.addClass("loading");
            },
            "onComplete": function () {
                result.removeClass("loading");
            },
            "onSuccess": function (info) {
                if (!info.success) {
                    result.set("html", "<span class=\"am-text-danger\">" + info.msg + "</span>");
                    return;
                }
                result.set("html",
                    "<a href=\"javascript:\" data-userid=\"${ID}\" class=\"diag-user am-btn am-btn-secondary am-round\">${UserName}</a> 当前余额:<label class=\"am-text-success\">${Money:money}</label>元 状态:<label class=\"am-text-primary\">${Status}</label>".toHtml(info.info));
            }
        }).post({
            "UserName": username
        });

    });
};

window.addEvents({
    "domready": function () {
        // 头部的管理菜单
        (function () {
            var admininfo = $("admin-info");
            var hiddenAdminInfoControlShow = function () {
                admininfo.removeClass("admin-info-control-show");
                $(document.body).removeEvent("click", hiddenAdminInfoControlShow);
            };
            admininfo.addEvent("click", function (e) {
                var usercontrol = admininfo.getElement(".admin-info-control");
                var userinfo = admininfo.getElement(".name");
                var obj = $(e.target);
                if (obj == userinfo || obj.getParent() == userinfo) {
                    usercontrol.setStyle("width", userinfo.getStyle("width"));
                    if (admininfo.hasClass("admin-info-control-show")) return;
                    admininfo.addClass("admin-info-control-show");
                    $(document.body).addEvent("click", hiddenAdminInfoControlShow);
                    e.stopPropagation();
                }
            });
        })();

        // 5分钟自动调用一次批量处理第三方游戏的锁定订单
        setInterval(function () {
            new Request.JSON({
                "url": "admin/game/transferchecklist",
                "onSuccess": function (result) {
                    if (!result.success || !result.info) return;
                    if (result.info.Success + result.info.Faild == 0) return;
                    new BW.Tip("第三方游戏转账锁定处理，成功${Success}笔，失败${Faild}笔".toHtml(result.info), {
                        "type": "tip",
                        "mask": false
                    });
                }
            }).post();
        }, 1000 * 60 * 5)

        // 演示站提示
        !function () {
            if (location.host != "admin.betwin.ph") return;
            new BW.Tip("温馨提示：您现在查看的是BetWin 2.0演示后台。<br />" +
                "如需购买系统，请点击左侧的<b>系统首页</b> - <b>财务结算</b>进行账单支付<br />" +
                "此支付方式为唯一付款渠道，其他渠道付款导致被骗本司概不负责。");
        }();
    },
    "load": function () {
        (function () {
            this.fireEvent("resize");
        }).delay(1000);
    },
    "resize": function () {
        // 调整头部的宽度
        (function () {
            var width = UI.getSize().x - $("admin-info").getSize().x - $("aside").getSize().x;
            $("frame-taskbar-list").setStyle("width", width);

            $("frame-taskbar-list").getElement(".frame-taskbar-container").setStyles({
                "width": width - 64
            });
        })();
    }
});