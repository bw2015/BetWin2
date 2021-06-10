//彩票游戏
if (!window["Lottery"]) window["Lottery"] = new Object();

// 彩票应用图标
(function (ns) {
    ns.Lottery = new Class({
        Implements: [Events, Options],
        "options": {
            // 图标宽度
            "size": 160,
            "data": []
        },
        "dom": {
            "element": null,
            // 彩票元素对象
            "itemlist": new Array()
        },
        "data": {
            // 坐标矩阵
            "matrix": new Array()
        },
        "initialize": function (el, options) {
            var t = this;
            t.setOptions(options);
            t.dom.element = el = $(el);

            t.matrix();
            t.options.data.each(function (item) {
                t.dom.itemlist.push(t.createItem(item))
            });

            t.sort();

            window.addEvent("resize", function () {
                t.matrix();
                t.sort();
            });
        },
        // 设置矩阵
        "matrix": function () {
            var t = this;
            var scale = 1;
            // 低分辨率适配
            !function () {
                var matrix = t.dom.element.getStyle("transform");
                var regex = /matrix\((0\.\d+)/;
                if (matrix && regex.test(matrix)) {
                    scale = regex.exec(matrix)[1].toFloat();
                }
            }();
            var width = t.dom.element.getSize().x / scale;
            var height = t.dom.element.getSize().y / scale;
            var xLength = Math.floor(width / t.options.size);
            t.data.matrix = new Array();
            for (var index = 0; index < t.options.data.length; index++) {
                var x = (index % xLength) * t.options.size;
                var y = Math.floor(index / xLength) * t.options.size;
                t.data.matrix.push({ "x": [x, x + t.options.size], "y": [y, y + t.options.size] });
            }
        },
        // 获取中心点
        "getPoint": function (left, top) {
            var t = this;
            var width = t.dom.element.getSize().x;
        },
        // 排序
        "sort": function () {
            var t = this;

            t.dom.itemlist.each(function (item, index) {
                var point = t.data.matrix[item.Sort];
                item.x = (t.data.matrix[item.Sort].x[0] + t.data.matrix[item.Sort].x[1]) / 2;
                item.y = (t.data.matrix[item.Sort].y[0] + t.data.matrix[item.Sort].y[1]) / 2;
                if (!item.obj.get("data-drag")) {
                    item.obj.setStyles({
                        "left": point.x[0],
                        "top": point.y[0]
                    });
                }
            });
        },
        // 重新计算排序值
        "updateSort": function () {
            var t = this;
            t.dom.itemlist.sort(function (a, b) {
                var ya = Math.floor(a.y / t.options.size);
                var yb = Math.floor(b.y / t.options.size);
                if (ya != yb) return ya - yb;

                return a.x - b.x;
            });
            t.dom.itemlist.each(function (item, index) {
                item.Sort = index;
            });
            t.sort();
        },
        // 获取当前坐标点所在的网格
        "getIndex": function (item, point) {
            var t = this;
            var index = -1;
            for (var i = 0; i < t.data.matrix.length; i++) {
                if (t.data.matrix[i].x[0] < point.x && t.data.matrix[i].x[1] > point.x) {
                    if (t.data.matrix[i].y[0] < point.y && t.data.matrix[i].y[1] > point.y) {
                        index = i;
                        break;
                    }
                }
            }
            if (index != -1) item.Sort = index;
            return index;
        },
        // 创建一个彩票元素
        "createItem": function (item) {
            var t = this;
            var obj = t.dom.itemlist.filter(function (item1) { return item1.Game == item.Game; }).getLast();

            if (obj != null) return obj;
            var src = null;
            switch (item.Game) {
                case "Second":
                    src = "lottery/second.html";
                    break;
                default:
                    src = "lottery/" + item.Category.toLowerCase() + ".html";
                    break;
            }
            obj = new Element("div", {
                "class": "item",
                "data-name": item.Game,
                "data-game": item.Game,
                "data-title": item.Name,
                "data-src": src,
                "data-category": item.Category,
                "data-post": "Game=" + item.Game,
                "events": {
                    "click": function () {
                        var obj = this;
                        if (obj.get("data-drag") || t.dom.element.hasClass("shake")) return;
                        ns.OpenFrame(obj);
                    },
                    "mousedown": function () {
                        var obj = this;
                        var timer = obj.retrieve("timer");
                        obj.store("timeindex", 0);
                        if (timer == null) {
                            timer = setInterval(function () {
                                var timeindex = obj.retrieve("timeindex");
                                timeindex++;
                                obj.store("timeindex", timeindex);
                                if (timeindex == 10) {
                                    obj.getParent().addClass("shake");
                                    obj.setStyle("z-index", 1);
                                    clearInterval(timer);
                                }
                            }, 100);
                            obj.store("timer", timer);
                        }
                    },
                    "mouseup": function () {
                        var obj = this;
                        var timer = obj.retrieve("timer");
                        if (timer != null) {
                            clearInterval(timer);
                            obj.store("timer", null);
                        }
                        t.dom.element.removeClass.delay(50, t.dom.element, ["shake"]);
                        obj.setStyle("z-index", 0);
                    },
                    "mouseleave": function () {
                        var obj = this;
                        obj.fireEvent("mouseup");
                    }
                }
            });
            new Element("img", {
                "src": "/images/space.gif",
                "class": "lottery-icon lottery128 " + item.Game
            }).inject(obj);

            new Element("label", {
                "class": "lottery-name",
                "text": item.Name
            }).inject(obj);

            var move = new Drag.Move(obj, {
                "container": t.dom.element,
                "onSnap": function (el) {
                    if (!t.dom.element.hasClass("shake")) {
                        this.stop();
                        return;
                    }
                },
                "onDrag": function (el) {
                    var item = el.retrieve("item");
                    item.x = el.getStyle("left").toInt() + t.options.size / 2;
                    item.y = el.getStyle("top").toInt() + t.options.size / 2;
                    t.updateSort();
                },
                "onStart": function (el) {
                    el.set("data-drag", 1);
                },
                "onComplete": function (el) {
                    (function () {
                        this.set("data-drag", null);
                        t.sort();
                        t.save();
                    }).delay(50, el);
                }
            });
            obj.inject(t.dom.element);
            var result = {
                "Game": item.Game,
                "Sort": item.Sort,
                "x": 0,
                "y": 0,
                "obj": obj
            };
            obj.store("item", result);
            return result;
        },
        // 排序结果发送至服务器
        "save": function () {
            var t = this;
            var data = new Array();
            t.dom.itemlist.each(function (item) {
                data.push(item.Game);
            });
            new Request.JSON({
                "url": "/handler/user/lottery/sort"
            }).post({
                "sort": data.join(",")
            })
        }
    });
})(BW);

// 开奖器
(function (ns) {
    // 当前已经打开的开奖器，避免重复开启
    ns.Game = new Object();

    ns.Time = new Class({
        Implements: [Events, Options],
        "options": {
            // 彩票类型
            "type": null,
            // 彩票的名字
            "name": null,
            // 定期执行的事件
            "callback": function (action) { }
        },
        "dom": {
            // 开奖区域
            "element": null,
            // 需要绑定更新的元素区域
            "objs": [],
            // 号码显示的元素
            "numbers": []
        },
        // 当前正在运行
        "running": false,
        // 当前提交的对象
        "request": null,
        "initialize": function (el, options) {
            var t = this;
            t.setOptions(options);

            t.dom.element = el = $(el);
            t.dom.objs = t.dom.element.getElements("[data-resulttime]");
            t.dom.numbers = t.dom.element.getElements(".lottery-number li");

            var openNumber = t.dom.element.getElement("[data-resulttime=OpenNumber]");
            // 视频窗口
            !function () {
                if (!openNumber || !Lottery.Video[t.options.type]) return;
                var video = Lottery.Video[t.options.type];
                var link = new Element("a", {
                    "href": "javascript:",
                    "text": "开奖视频",
                    "class": "btn-lottery-video btn btn-green",
                    "events": {
                        "click": function (e) {
                            new BW.Diag(this, {
                                "name": "lottery-video-" + t.options.type,
                                "type": "frame",
                                "src": video.url,
                                "drag": true,
                                "resize": video.resize,
                                "title": "开奖视频",
                                "width": video.width,
                                "height": video.height,
                                "cssname": "diag-lottery-video"
                            });
                        }
                    }
                });
                link.inject(openNumber);
                if (video.autoplay) link.fireEvent("click");
            }();

            if (BW.Task.data[t.options.type]) {
                BW.Task.data[t.options.type].addEvent("dispose", function () {
                    t.dispose();
                });
            }
            t.running = true;

            t.request = new Request.JSON({
                "url": "/handler/game/lottery/index",
                "onSuccess": function (result) {
                    if (!result.success) {
                        if (result.info && result.info["BuildIndex"]) {
                            // 自主创建彩期

                        } else {
                            new BW.Tip(result.msg, {
                                "callback": function () {
                                    BW.Task.close(t.options.type);
                                }
                            });
                        }
                        return;
                    }
                    if (!t.running) return;
                    t.show(result.info, true);
                }
            });

            t.gettime();

        },
        // 获取时间的回调事件
        "gettime": function () {
            var t = this;
            if (!t.running) return;
            t.request.post({
                "Game": t.options.type
            });
        },
        // 在本地计算时间
        "localtime": function () {
            var t = this;
            if (!t.running) return;
            var betTime = t.dom.element.getElement("[data-resulttime=BetTime]").retrieve("endtime");
            var openTime = t.dom.element.getElement("[data-resulttime=OpenTime]").retrieve("endtime");

            t.show({
                "BetTime": betTime.getDateDiff(new Date()).TotalSecond,
                "OpenTime": openTime.getDateDiff(new Date()).TotalSecond
            })
        },
        // 显示信息
        "show": function (info) {
            var t = this;

            t.dom.objs.each(function (obj) {
                var name = obj.get("data-resulttime");
                var value = info[name];
                if (value == undefined) return;
                t.options.callback.apply(t);
                switch (name) {
                    case "BetIndex":
                    case "OpenIndex":
                        obj.set("text", value);
                        break;
                    case "BetTime":
                    case "OpenTime":
                        obj.set("data-timer", value);
                        if (info.ServerTime) {
                            obj.store("endtime", new Date().AddSecond(value));
                        }
                        Setting.UpdateTimer(obj);
                        break;
                    case "OpenNumber":
                        var openIndex = info.OpenIndex.contains("-") ? info.OpenIndex.substring(info.OpenIndex.indexOf("-") + 1) : info.OpenIndex;

                        if (value == "") {
                            if (!obj.hasClass("loading")) {
                                obj.addClass("loading");
                                t.dom.numbers.set("text", "");
                                UI.SoundText(t.options.name + "第" + openIndex + "期正在开奖");
                                t.options.callback.apply(t, ["loading"]);
                            }
                        } else {
                            var isupdate = obj.get("data-index") != openIndex;
                            if (obj.hasClass("loading") || isupdate) {
                                obj.removeClass("loading");
                                var openNumber = info.OpenNumber.split(',');
                                t.dom.numbers.each(function (li, index) {
                                    if (t.dom.numbers.length != openNumber.length) {
                                        li.empty();
                                    } else {
                                        li.set({
                                            "class": ["index", index, " num", openNumber[index]].join(""),
                                            "text": openNumber[index]
                                        });
                                    }
                                });
                                UI.SoundText(t.options.name + "第" + openIndex + "期已开奖，开奖号码：" + openNumber.join(","));
                                obj.set("data-index", openIndex);
                                t.options.callback.apply(t, ["open"]);
                            }
                        }
                        break;
                }
            });

            if (info.OpenNumber == "" || info.BetTime <= 0 || info.OpenTime <= 0) {
                t.gettime.delay(1000, t);
            } else {
                t.localtime.delay(1000, t);
            }
        },
        // 注销此定时器
        "dispose": function () {
            var t = this;
            t.request.cancel();
            t.running = false;
        }
    });

})(Lottery);

// 玩法页面打开的之后的加载回调
BW.callback["lottery-player"] = function (result) {
    var t = this;
    t.dom.element.store("player-info", result.info);

    // 关闭其他的定时器
    if (!BW.Task.data[t.options.type]) {
        Object.each(Lottery.Game, function (time, key) {
            console.log(Lottery.Game[key]);
            if (time) time.dispose();
            Lottery.Game[key] = null;
        });
    }

    var timerIndex = 0;
    var time = new Lottery.Time(t.dom.element, {
        "type": result.info.Type,
        "name": result.info.Game,
        "callback": function (action) {
            switch (action) {
                case "open":
                    var contentResult = t.dom.element.getElement(".lottery-info-content-result");
                    if (contentResult) contentResult.getBindEvent().fire();
                    break;
                case "loading":
                    break;
                default:
                    timerIndex++;
                    if (timerIndex % 30 == 0) {
                        var orderlist = t.dom.element.getElement(".lottery-bet-orderlist");
                        if (orderlist) orderlist.getBindEvent().fire();
                    }
                    break;
            }
        }
    });
    Lottery.Game[result.info.Type] = time;

    // 开始构建html
    t.dom.element.getElements(".lottery-game-name").set("html", result.info.Game);

    // 玩法一级分类
    var group = t.dom.element.getElement(".lottery-player-group");
    if (group == null) return;
    group.empty();
    var defaultGroup = null;

    result.info.Player.map(function (item) { return item.GroupName; }).distinct().each(function (item, index) {
        var groupObj = new Element("a", {
            "href": "javascript:",
            "text": item,
            "data-name": item,
            "events": {
                "click": function (e) {
                    BW.callback["lottery-player-sublist"].apply(t, [this, result]);
                }
            }
        });
        groupObj.inject(group);
        if (index == 0) defaultGroup = groupObj;
    });

    defaultGroup.fireEvent("click");

    // 号码区的点击事件
    t.dom.container.getElement(".lottery-player-selector-ball").addEvent("click", function (e) {
        var obj = $(e.target);
        var player = null;
        if (obj.get("data-tool")) {
            var number = obj.getParent(".item").getElements(".ball > a[data-value]");
            var max = number.map(function (item) { return item.get("data-value").toInt(); }).getLast();
            number.each(function (num, index) {
                var value = num.get("data-value").toInt();
                switch (obj.get("data-tool")) {
                    case "全":
                        num.addClass("current");
                        break;
                    case "大":
                        index >= number.length / 2 ? num.addClass("current") : num.removeClass("current");
                        break;
                    case "小":
                        index < number.length / 2 ? num.addClass("current") : num.removeClass("current");
                        break;
                    case "奇":
                        value % 2 == 1 ? num.addClass("current") : num.removeClass("current");
                        break;
                    case "偶":
                        value % 2 == 0 ? num.addClass("current") : num.removeClass("current");
                        break;
                    case "清":
                        num.removeClass("current");
                        break;
                }
            });
            player = t.dom.element.retrieve("player");
        }
        if (obj.get("data-value")) {
            obj.toggleClass("current");
            player = t.dom.element.retrieve("player");
        }

        if (player != null) {
            player.getMoney();
        }
    });

    // 模式选择
    !function () {
        var mode = t.dom.container.getElement("[data-dom=mode]");
        if (!mode) return;
        mode.empty();
        Setting.Site.Setting.LotteryMode.split(',').each(function (item, index) {
            if (!MODE[item]) return;
            new Element("a", {
                "href": "javascript:",
                "text": item,
                "class": index == 0 ? "current" : null
            }).inject(mode);
        });
        mode.addEvent("click", function (e) {
            var obj = $(e.target);
            if (obj.get("tag") != "a") return;
            setCurrent(this.getElements("a"), obj);
            var player = t.dom.element.retrieve("player");
            player.getMoney();
        });
    }();

    // 倍数选择
    t.dom.container.getElement("[data-dom=times").addEvents({
        "focus": function () {
            this.set("data-times", this.get("value"));
        },
        "change": function () {
            var value = this.get("value").toInt();
            if (isNaN(value) || value < 1) {
                this.set("value", this.get("data-times"));
                return;
            }
            var player = t.dom.element.retrieve("player");
            player.getMoney();
        }
    });

    // 选号按钮
    t.dom.container.getElement("[data-dom=select]").addEvent("click", function () {
        var player = t.dom.element.retrieve("player");
        if (player.data.submit.money < 0.01) {
            new BW.Tip("投注金额错误");
            return;
        }
        player.selected();
    });

    // 快速投注按钮
    t.dom.container.getElement("[data-dom=quick]").addEvent("click", function () {
        var player = t.dom.element.retrieve("player");
        if (player.data.submit.money < 0.01) {
            new BW.Tip("投注金额错误");
            return;
        }
        player.selected(function () {
            t.dom.container.getElement("[data-dom=submit]").fireEvent("click");
        });
    });

    // 已选择的投注项目
    t.dom.container.getElement(".lottery-player-selected").addEvent("click", function (e) {
        var obj = $(e.target);
        var player = t.dom.element.retrieve("player");
        switch (obj.get("data-action")) {
            case "clear":
                player.clear();
                break;
            case "delete":
                obj.getParent("li").dispose();
                player.getTotal();
                break;
        }
    });

    // 提交全部订单
    t.dom.container.getElement("[data-dom=submit]").addEvent("click", function () {
        var player = t.dom.element.retrieve("player");

        var data = new Array();
        player.dom.selected.getElements("li").each(function (item) {
            var itemData = item.retrieve("selected");
            if (itemData == null) return;
            data.push(itemData);
        });
        if (data == null || data.length == 0) {
            new BW.Tip("没有投注记录");
            return;
        }

        new Request.JSON({
            "url": "/handler/user/lottery/save",
            "onRequest": function () {
                t.dom.container.addClass("loading-lottery");
            },
            "onComplete": function () {
                t.dom.container.removeClass("loading-lottery");
            },
            "onSuccess": function (result) {
                var tip = new BW.Tip(result.msg, {
                    "delay": 3000,
                    "callback": function () {
                        if (result.success) {
                            player.clear();
                            var submitcallback = t.dom.element.retrieve("submit-callback");
                            if (submitcallback) {
                                submitcallback.each(function (callback) {
                                    callback.apply(t, [result]);
                                });
                            }
                        }
                    }
                });

            }
        }).post(JSON.encode(data))
    });
};

// 二级分类
BW.callback["lottery-player-sublist"] = function (group, result) {
    var t = this;
    var groupCurrent = group.getParent().getElement(".current");
    if (groupCurrent == group) return;
    if (groupCurrent != null) groupCurrent.removeClass("current");
    group.addClass("current");
    var groupName = group.get("data-name");
    // 玩法二级分类和玩法本身
    var playerlist = t.dom.element.getElement(".lottery-player-list");
    playerlist.empty();
    var labelList = result.info.Player.filter(function (item) { return item.GroupName == groupName; }).map(function (item) { return item.LabelName; }).distinct();
    var defaultPlayer = null;
    labelList.each(function (labelName) {
        var p = new Element("p");
        new Element("label", {
            "text": labelName + ":"
        }).inject(p);
        result.info.Player.filter(function (item) { return item.GroupName == groupName && item.LabelName == labelName; }).each(function (item) {
            var playerObj = new Element("a", {
                "href": "javascript:",
                "text": item.PlayName,
                "data-code": item.Code,
                "data-playerid": item.ID,
                "data-name": item.LabelName + "_" + item.PlayName,
                "data-tip": item.Tip,
                "data-reward": item.RewardMoney,
                "data-singlebet": item.SingledBet,
                "data-singlereward": item.SingledReward,
                "events": {
                    "click": function (e) {
                        BW.callback["lottery-player-selected"].apply(t, [this, result]);
                    }
                }
            });
            playerObj.inject(p);
            if (defaultPlayer == null) defaultPlayer = playerObj;
        });
        p.inject(playerlist);
    });

    defaultPlayer.fireEvent("click");
};

// 选择玩法
BW.callback["lottery-player-selected"] = function (player, result) {
    var t = this;

    var category = result.info.Category;
    var code = player.get("data-code");
    result = result.info.Player.filter(function (item) { return item.Code == code; }).getLast();
    var playerId = /\d+$/.exec(result.Code)[0];

    if (result == null || !Lottery[category][playerId]) {
        alert("未设定玩法" + player.get("data-name"));
        return;
    }

    var selected = t.dom.element.getElement(".lottery-player-selected ul");
    var container = t.dom.element.getElement(".lottery-player-selector");
    selected.setStyle("height", null);

    var obj = new Lottery[category][playerId]({
        "bind": t,
        "id": player.get("data-playerid"),
        "name": player.get("data-name"),
        "code": player.get("data-code"),
        "reward": player.get("data-reward").toFloat(),
        "tip": player.get("data-tip").trim(),
        "singlebet": player.get("data-singlebet"),
        "singlereward": player.get("data-singlereward")
    });

    t.dom.element.store("player", obj);
    if (t.dom.element.retrieve("playerCurrent") != null) {
        t.dom.element.retrieve("playerCurrent").removeClass("current");
    }
    player.addClass("current");
    t.dom.element.store("playerCurrent", player);

    selected.setStyle("height", Math.max(container.getStyle("height").toInt() - 100, 265));

};

// 彩票信息加载（controls/lottery-info.html)
BW.callback["lottery-player-info"] = function (html) {
    var t = this;
    var tab = t.dom.element.getElements(".lottery-info-tab > a");
    var content = t.dom.element.getElements(".lottery-info-content");
    if (tab.length != content.length) return;

    content.each(function (item, index) {
        var id = "lottery-info-" + Math.round(Math.random() * new Date().getTime());
        item.set("id", id);
        tab[index].set("data-event-fire", id);
        tab[index].addEvent("mouseover", function () {
            tab.each(function (t) { t.removeClass("current"); })
            this.addClass("current");

            content.each(function (t) { t.setStyle("display", "none"); })
            content[index].setStyle("display", "block");

            BW.Bind($(id)).fire();
        });
    });
    tab[0].fireEvent("mouseover");

    // 走势图按钮
    !function () {
        var chart = t.dom.element.getElement(".lottery-info-trend");
        if (chart == null) return;
        var game = t.dom.element.getParent("[data-game]");
        if (!game || !t.options.data) return;
        switch (game.get("data-game")) {
            case "X5":
                chart.set({
                    "href": "/lottery/trend.html?" + t.options.data.Game,
                    "target": "_blank"
                })
                break;
        }
    }();
};

// 玩法选择页面( lottery/bet-player.html)
BW.callback["lottery-bet-player"] = function (html) {
    var t = this;
    var rebate = t.dom.element.getElement(".lottery-player-submit-rebate-switch");
    var range = t.dom.element.getElement("[data-dom=rebate-range]");
    var parent = t.dom.element.getParent(".lottery-player");
    var dom = new Object();
    t.dom.element.getElements("[data-field]").each(function (item) {
        dom[item.get("data-field")] = item;
    });
    var site = Setting.Site;

    var player = null;
    var rebateSelected = function (isSelected) {
        player = parent.retrieve("player-info");
        if (player == null) {
            rebateSelected.delay(100, t, [isSelected]);
            return;
        }

        var maxRebate = Math.min(2000, player.Rebate);  //
        range.set({
            "min": site.Setting.MinRebate,
            "max": maxRebate,
        });
        if (isSelected) {
            maxRebate = site.Setting.MinRebate.toInt();
        }

        dom["Rebate"].set("text", maxRebate);
        dom["Return"].set("text", ((player.Rebate.toInt() - maxRebate) / 2000).ToString("p"));
        range.set("value", maxRebate);


        console.log(maxRebate);
    };
    rebate.addEvent("click", function () {
        this.removeClass("selected-half");
        this.toggleClass("selected");
        rebateSelected(this.hasClass("selected"));
    });
    rebateSelected(false);

    !function () {
        range.addEvent("mousemove", function (e) {
            if (!player) { e.stop(); return; }
            var maxRebate = Math.min(1980, player.Rebate);
            var value = this.get("value").toInt();
            dom["Rebate"].set("text", value);
            dom["Return"].set("text", ((player.Rebate.toInt() - value) / 2000).ToString("p"));
            if (value == maxRebate) {
                rebate.removeClass("selected-half");
                rebate.removeClass("selected");
                rebateSelected(false);
            } else if (value == site.Setting.MinRebate.toInt()) {
                rebate.removeClass("selected-half");
                rebate.addClass("selected");
                rebateSelected(true);
            } else {
                rebate.addClass("selected-half");
            }
        });
    }();

    t.dom.element.getElement(".lottery-player-selected").addEvent("mousewheel", function (e) {
        e.stopPropagation();
    });

    // 追号
    !function () {
        var btnChase = t.dom.element.getElement("[data-dom=chase]");
        if (!btnChase) return;
        btnChase.addEvent("click", function (e) {

            var data = new Array();
            t.dom.element.getElements("ul[data-dom=selected] li").each(function (item) {
                var itemData = item.retrieve("selected");
                if (itemData == null) return;
                data.push(itemData);
            });
            if (data.length == 0) {
                new BW.Tip("当前没有选择投注项目");
                return;
            }

            new BW.Diag(this, {
                "name": "game-lottery-chase",
                "width": 800,
                "height": 600,
                "type": "control",
                "src": "lottery/bet-chase.html",
                "title": "追号设置",
                "mask": true,
                "drag": true,
                "close": false,
                "store": data,
                "callback": "game-lottery-chase,tab",
                "data": "Type=" + player.Type
            });
        });
    }();

    // 合买
    !function () {
        var btnUnited = t.dom.element.getElement("[data-dom=united]");
        if (!btnUnited) return;
        btnUnited.addEvent("click", function (e) {
            var data = new Array();
            t.dom.element.getElements("ul[data-dom=selected] li").each(function (item) {
                var itemData = item.retrieve("selected");
                if (itemData == null) return;
                data.push(itemData);
            });
            if (data.length == 0) {
                new BW.Tip("当前没有选择投注项目");
                return;
            }

            new BW.Diag(this, {
                "name": "game-lottery-united",
                "width": 800,
                "height": 600,
                "type": "control",
                "src": "lottery/bet-united.html",
                "title": "发起合买",
                "mask": true,
                "drag": true,
                "close": false,
                "store": data,
                "callback": "game-lottery-united",
                "data": "Type=" + player.Type
            });
        });
    }();

    // 帮助信息
    t.dom.element.getElement(".lottery-player-tip").addEvent("click", function () {
        //console.log(player);
        var current = parent.retrieve("playerCurrent");
        if (!current) return;
        new BW.Tip(current.get("data-tip"), {
            "width": 300
        });
    });

};

// 投注页面的投注记录（lottery/bet-log.html)
BW.callback["lottery-bet-log"] = function (html) {
    var t = this;
    var list = t.dom.element.getElement("table.list");
    var id = list.get("id");
    if (!id) {
        id = "bet-log-list-" + Math.round(Math.random() * 10000) + "-" + new Date().getTime();
        list.set("id", id);
    }
    var btn = t.dom.element.getElement("[data-type=refser]");
    btn.set("data-event-fire", id);

    // 投注成功之后刷新投注记录
    var parent = t.dom.element.getParent(".lottery-player");
    if (!parent.get("data-bet-log-list")) {
        parent.set("data-bet-log-list", true);
        var callback = parent.retrieve("submit-callback") || new Array();
        callback.push(function () {
            t.fire();
        });
        parent.store("submit-callback", callback);
    }
};

// 六合彩的玩法回调
BW.callback["lottery-player-m6"] = function (result) {
    var t = this;

    new Lottery.Time(t.dom.element, {
        "type": result.info.Type,
        "name": result.info.Game
    });

    // 开始构建html
    t.dom.element.getElements(".lottery-game-name").set("html", result.info.Game);

};

// 自主创建彩期玩法的回调
BW.callback["lottery-buildindex"] = function () {
    var t = this;
    var callback = t.dom.element.retrieve("submit-callback") || new Array();

    callback.push(function (result) {
        if (!result.info || !result.info.BuildIndex) return;
        var openNumber = t.dom.element.getElement("[data-resulttime=OpenNumber]");
        if (!openNumber) return;
        if (openNumber.hasClass("loading") || openNumber.hasClass("waitresult")) return;
        openNumber.addClass("loading waitresult");
        var mask = new Element("div", {
            "class": "diag-mask"
        });
        mask.inject(document.body);
        var data = t.options.data;
        data["index"] = result.info.Index;
        var request = new Request.JSON({
            "url": "/handler/game/lottery/indexresult",
            "onSuccess": function (indexResult) {
                if (!indexResult.info.Number) {
                    request.post.delay(1000, request, [data]);
                } else {
                    //#1 去除等待效果
                    openNumber.removeClass("loading");
                    //#2 逐个号码赋值
                    var num = indexResult.info.Number.split(",");
                    openNumber.getElements("li").each(function (li, index) {
                        li.set("text", num[index]);
                    });
                    //#3 遮罩擦去效果
                    openNumber.addClass("openresult");
                    //#4 显示确定按钮
                    new Element("a", {
                        "href": "javascript:",
                        "class": "btn btn-green",
                        "text": "确定",
                        "events": {
                            "click": function () {
                                var obj = this;
                                console.log(mask);
                                console.log(obj);
                                mask.dispose();
                                openNumber.removeClass("openresult");
                                openNumber.removeClass("waitresult");
                                obj.dispose();
                            }
                        }
                    }).inject(openNumber);
                }
            }
        });

        request.post(data);
    });

    t.dom.element.store("submit-callback", callback);
};

// 小游戏
BW.callback["lottery-smallgame"] = function (result) {
    var t = this;
    if (!result.success) {
        new BW.Tip(result.msg, {
            "callback": function () {

            }
        });
        return;
    }
    var dom = new Object();
    t.dom.element.getElements("[data-dom]").each(function (item) {
        dom[item.get("data-dom")] = item;
    });
    dom["name"].set("text", result.info.Game);

    result.info.Player.each(function (item) {
        var li = new Element("li", {
            "events": {
                "click": function () {
                    var game = /_(\w+)$/i.exec(item.Code)[1].toLowerCase();
                    new BW.Diag(this, {
                        "type": "frame",
                        "cssname": "smallgame-diag",
                        "src": "game/" + game + "/?id=" + item.ID,
                        "name": item.Code,
                        "width": 400,
                        "height": 600,
                        "title": item.PlayName,
                        "drag": true
                    });
                }
            }
        });
        new Element("img", {
            "src": "images/space.gif",
            "class": "lottery128 " + item.Code
        }).inject(li);
        new Element("label", {
            "text": item.PlayName
        }).inject(li);
        li.inject(dom["player"]);
    });
};

// 彩票的模式
htmlFunction["lottery-mode"] = function (value) {
    var mode = value.toFloat();
    if (isNaN(mode)) return value;
    switch (mode) {
        case 1:
            value = "元";
            break;
        case 0.5:
            value = "一元";
            break;
        case 0.1:
            value = "角";
            break;
        case 0.01:
            value = "分";
            break;
        case 0.001:
            value = "厘";
            break;
    }
    return value;
};