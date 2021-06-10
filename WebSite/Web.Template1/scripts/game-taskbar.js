/// <reference path="game.js" />

// 状态栏
(function (ns) {

    ns.TaskBar = new Class({
        Implements: [Events, Options],
        "options": {

        },
        // 当前窗口
        "current": null,
        // 已经打开的frame
        "data": {
        },
        "dom": {
            // 状态栏
            "taskbar": null,
            // 应用程序窗口栏
            "applications": null
        },
        "initialize": function (el, apps, options) {
            var t = this;
            t.setOptions(options);
            t.dom.taskbar = el = $(el);
            t.dom.applications = apps = $(apps);

            t.setTaskbar();

            window.addEvent("resize", function () {
                t.setTaskbar.apply(t);
            });
        },
        // 设置状态栏的宽度
        "setTaskbar": function () {
            var t = this;
            var width = UI.getSize().x;
            var height = UI.getSize().y - t.dom.applications.getStyle("top").toInt();
            $$(".common-header .logo,.common-header #header-userinfo").each(function (item) {
                width -= item.getStyle("width").toInt();
                width -= item.getStyle("margin-left").toInt();
                width -= item.getStyle("margin-right").toInt();
            });

            t.dom.taskbar.setStyle("width", width);

            t.dom.applications.set({
                "data-height": height,
                "styles": {
                    "width": UI.getSize().x - t.dom.applications.getStyle("left").toInt()
                }
            });
            t.dom.applications.getElements(".application").each(function (item) {
                item.setStyle("height", height);
            });
        },
        // 打开一个窗口  
        "open": function (name) {
            var t = this;
            if (t.current == name) return;
            t.minimize(t.current);
            if (t.data[name]) {
                t.data[name].open();
                t.current = name;
            }
        },
        // 关闭一个窗口
        "close": function (name) {
            if (name == null) return;
            var t = this;
            if (t.data[name]) {
                t.data[name].close();
                t.data[name] = null;
                t.current = null;
            }
        },
        "minimize": function (name) {
            if (name == null) return;
            var t = this;
            if (t.data[name]) {
                t.data[name].minimize();
                t.current = null;
            }
        },
        // 最小化全部窗口
        "Minimize": function () {
            var t = this;
            if (t.current != null) {
                t.minimize(t.current);
            }
        }
    });

    ns.Task = null;

})(BW);

// 打开的一个窗口任务
(function (ns) {

    ns.Frame = new Class({
        Implements: [Events, Options],
        // 窗口名字
        "name": null,
        "options": {
            // 窗口名字（全局唯一）
            "name": null,
            // 窗口的中文名字
            "title": null,
            // 加载地址
            "src": null,
            // 回调函数
            "callback": null,
            // 初始加载函数
            "load": "",
            // 附带参数 QueryString 字符串
            "post": null
        },
        "dom": {
            //点击对象
            "app": null,
            // 打开的窗口对象
            "element": null,
            // 任务栏图标
            "task": null
        },
        // 注销时候的事件
        "onDispose": function () { },
        "initialize": function (app, options) {
            var t = this;
            t.setOptions(options);
            t.name = t.options.name;
            t.dom.app = app = $(app);
            if (t.options.callback == null) t.options.callback = "frame-control";
            t.dom.element = new Element("div", {
                "class": "application",
                "data-name": t.name,
                "data-bind-action": t.options.src,
                "data-bind-type": "control",
                "data-bind-post": t.options.post,
                "data-bind-callback": t.options.callback,
                "data-bind-load": t.options.load,
                "styles": {
                    "height": ns.Task.dom.applications.get("data-height").toInt()
                }
            });
            t.dom.element.inject(ns.Task.dom.applications);

            t.dom.task = new Element("div", {
                "class": "task",
                "title": t.options.title,
                "data-name": t.options.name,
                "title": t.options.title,
                "events": {
                    "click": function (e) {
                        ns.Task.open(t.options.name);
                    },
                    // 右键菜单
                    "contextmenu": function (e) {
                        e.stop();
                        new Element("a", {
                            "href": "javascript:",
                            "data-menu": "contextmenu",
                            "class": "header-taskbar-close",
                            "text": "关闭",
                            "events": {
                                "mousedown": function () {
                                    ns.Task.close(t.options.name);
                                }
                            },
                            "styles": {
                                "left": e.client.x,
                                "top": e.client.y
                            }
                        }).inject(document.body);
                    }
                }
            });
            new Element("div", {
                "class": "background"
            }).inject(t.dom.task);
            new Element("em", {
                "class": "lottery32 " + t.name
            }).inject(t.dom.task);

            t.dom.task.inject(ns.Task.dom.taskbar);

            new BW.BindEvent(t.dom.element);

        },
        // 打开窗口
        "open": function () {
            var t = this;
            t.dom.task.addClass("current");
            t.dom.element.addClass("current");
        },
        // 关闭窗口
        "close": function () {
            var t = this;
            t.dom.task.dispose();
            t.dom.element.dispose();
            t.fireEvent("dispose");
        },
        // 最小化
        "minimize": function () {
            var t = this;
            t.dom.task.removeClass("current");
            t.dom.element.removeClass("current");
        }
    });


    // 打开一个窗口
    ns.OpenFrame = function (element) {
        var isConfig = element["name"];
        var name = isConfig ? element["name"] : element.get("data-name");
        var title = isConfig ? element["title"] : element.get("data-title");
        var src = isConfig ? element["src"] : element.get("data-src");
        var callback = isConfig ? element["callback"] : element.get("data-callback");
        var post = isConfig ? element["post"] : element.get("data-post");
        if (!BW.Task.data[name]) {
            ns.Task.data[name] = new ns.Frame(element, {
                "name": name,
                "src": src,
                "title": title,
                "post": post,
                "callback": callback
            });
        }
        ns.Task.open(name);
    };

    // 关闭和最小化窗口的控制按钮
    ns.callback["frame-control"] = function (html) {
        var t = this;
        new Element("div", {
            "class": "application-control",
            "html": "<a href=\"javascript:\" class=\"minimize icon icon32 min-black\"></a><a href=\"javascript:\" class=\"close icon icon32 close-black\"></a>",
            "events": {
                "click": function (e) {
                    var application = this.getParent(".application[data-name]");
                    if (application == null) return;
                    var name = application.get("data-name");
                    var obj = $(e.target);
                    if (obj.hasClass("close")) {
                        ns.Task.close(name);
                    } else if (obj.hasClass("minimize")) {
                        ns.Task.minimize(name);
                    }
                }
            }
        }).inject(t.dom.element, "top");
    };

})(BW);


window.addEvent("domready", function () {
    var loadtaskbar = function () {
        var taskbar = $("header-taskbar");
        if (taskbar == null) {
            loadtaskbar.delay(500);
            return;
        }
        BW.Task = new BW.TaskBar(taskbar, "applications");
        taskbar.store("taskbar", BW.Task);
    };
    loadtaskbar.apply(this);
});

window.addEvent("click", function (e) {
    var list = $$(document.body.children).filter(function (item) {
        if (item.get("data-menu") == "contextmenu") {
            item.dispose();
        }
    });
});