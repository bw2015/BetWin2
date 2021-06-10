/// <reference path="mootools.js" />
window["alert"] = function (msg) {
    new BW.Tip(msg);
};

//彩票游戏
if (!window["Lottery"]) window["Lottery"] = new Object();



var GolbalSetting = {
    // 当前登录的用户
    "User": null,
    // 当前站点
    "Site": null,
    // 站内沟通的初始化信息
    "IM": null,
    // 显示新信息
    "showNewMessage": function () {
        var t = this;
        if (!t.User) return;
        var newMsg = t.User.NewMessage.toInt();
        var msgCount = 0;
        $$("[data-usermessage]").each(function (item) {
            var value = item.get("text").toInt();
            msgCount = Math.max(msgCount, value);
            if (value == newMsg) return;

            item.set("text", newMsg);
            if (newMsg > 0) {
                item.addClass("new");
            } else {
                item.removeClass("new");
            }
        });
        if (newMsg > msgCount) t.audio.message();
    },
    // 吧用户信息存入变量
    "setUserInfo": function (info) {
        var t = this;
        t.User = info;
        t.showNewMessage();
        BW.callback["mobile-showusermoney"]();
    },
    "audio": {
        // 播放短信通知声音
        "message": function () {
            if ($("audio-message").play) {
                $("audio-message").play();
            }
        }
    },
    // 显示倒计时
    "UpdateTimer": function (obj) {
        if (obj.get("tag") == "input") return;
        var time = obj.get("data-timer").toInt();
        if (time < 0) time = 0;
        var timer = time.ToString("HH:mm:ss");
        obj.set("text", timer);
    },
    // 全局的加载效果
    "loading": function (isHide) {
        if (!isHide) {
            $(document.body).addClass("loading");
        } else {
            $(document.body).removeClass("loading");
        }
    }
};

// 原生APP的操作
var APP = null;


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

// 当前屏幕的尺寸
var DOM = {
    // 宽度
    "width": null,
    "height": null,
    // 框架显示组件
    "frames": null,
    // 标题栏
    "title": null,
    // 底部菜单栏
    "footer": null
};


var FrameData = {

};

var Frame = new Class({
    "Implements": [Events, Options],
    "options": {
        "name": null,
        "id": null,
        "href": null,
        "data": null,
        "callback": null
    },
    "dom": {

    },
    "initialize": function (options) {
        var t = this;
        t.setOptions(options);

        var item = DOM.frames.getElement(".frame-item[data-name=" + t.options.name + "]");
        if (item == null) {
            var callback = ["footer-menu"];
            if (t.options.callback) {
                t.options.callback.split(',').each(function (fn) { callback.push(fn); });
            }
            item = new Element("div", {
                "class": "frame-item",
                "data-bind-action": t.options.href,
                "data-bind-type": "control",
                "data-bind-callback": callback.join(","),
                "data-name": t.options.name,
                "data-bind-load": "scroll-top,header-title",
                "data-bind-post": t.options.data,
                "styles": {
                    "width": DOM.width
                }
            });
            item.inject(DOM.frames);
            BW.Bind(item);
            item.store("frame", t);
        } else {
            BW.Bind(item).fire();
        }
        t.show(item);
    },
    // 显示当前项目
    "show": function (item) {
        var t = this;
        var list = DOM.frames.getElements(".frame-item");
        var index = list.indexOf(item);
        if (index == -1) return;

        list.filter(function (newItem, newIndex) { return newIndex > index; }).dispose();
        DOM.frames.setStyles({
            "width": (index + 1) * DOM.width,
            "margin-left": index * DOM.width * -1
        });
        DOM.title.set("html", item.get("data-name"));
        item.setStyle("height", null);
    }
});

// 打开一个窗口的时候滚动到顶部
BW.load["scroll-top"] = function () {
    var t = this;
    t.dom.element.getAllPrevious().each(function (item) {
        item.setStyle("height", "100px");
    });
};

// 打开一个窗口
function OpenFrame(url, name, data, callback) {
    if (!callback) {
        callback = "mobile-checkapp,mobile-showusermoney";
    } else {
        callback += ",mobile-checkapp,mobile-showusermoney";
    }
    new Frame({
        "href": url,
        "name": name,
        "data": data,
        "callback": callback
    });
}

// 后退窗口
function Back() {
    var left = DOM.frames.getStyle("margin-left").toInt();
    if (left % DOM.width != 0) return;

    var index = Math.abs(left / DOM.width);
    if (index < 1) return;
    index = index - 1;

    var list = DOM.frames.getElements(".frame-item");
    var item = list[index];

    item.retrieve("frame").show(item);
    BW.callback["footer-menu"].apply(item.retrieve("bind"));
}

// 检查是否是app
BW.callback["mobile-checkapp"] = function (result) {
    var t = this;
    // 如果不是原生APP
    if (!window["APP"]) {
        t.dom.element.getElements("[data-app=true]").dispose();
    }
}

// 显示用户的可用余额
BW.callback["mobile-showusermoney"] = function () {
    if (GolbalSetting.User) {
        $$("[data-usermoney]").set("text", htmlFunction["money"](GolbalSetting.User.Money));
    }
};

// 载入框架的时候显示头部标题栏
BW.load["header-title"] = function () {
    var t = this;
};

// 添加下级用户的时候自动载入返点
BW.load["user-team-add"] = function () {
    var t = this;
    var rebate = t.dom.element.getElement("[name=Rebate]");
    rebate.set("value", Math.max(GolbalSetting.User.Rebate - 2, GolbalSetting.Site.Setting.MinRebate));
};

// 错误处理
BW.callback["mobile-error"] = function (result) {
    var t = this;

    if (!result.success) {
        new BW.Tip(result.msg, {
            "callback": function () {
                if (result.info && result.info.Type) {
                    if (window["error"][result.info.Type]) {
                        window["error"][result.info.Type].apply(t, [result]);
                    }
                } else {
                    var error = t.dom.element.get("data-bind-mobile-error");
                    if (error && BW.callback[error]) {
                        BW.callback[error](t, [result]);
                    }
                }
            }
        });
    }
};

// 载入站点信息
BW.callback["mobile-site"] = function (result) {
    var t = this;
    if (!result.success) return;
    GolbalSetting.Site = result.info;
};

// 载入用户信息(在user.html body控件上）
BW.callback["mobile-user"] = function (result) {
    var t = this;
    if (!result.success) return;
    GolbalSetting.setUserInfo(result.info);
    loadUser.delay(10 * 1000, t);
};

// 加载完成时候决定是否显示底部的菜单栏
BW.callback["footer-menu"] = function (html) {
    var t = this;
    var menu = t.dom.container.getElement("[data-menu]");
    DOM.footer.setStyle("display", menu == null ? "none" : "block");
    if (menu == null) {
        t.dom.container.setStyle("padding-bottom", "0px");
        return;
    }
    var menuName = menu.get("data-menu");
    var current = DOM.footer.getElement(".current[data-menu]");
    var afterCurrent = DOM.footer.getElement("[data-menu=" + menuName + "]");
    if (afterCurrent == null || afterCurrent == current) return;
    if (current != null) current.removeClass("current");
    afterCurrent.addClass("current");
};

// 隐藏所有标注为 data-fill-hide="true" 的元素
// 把所有表单元素锁定
BW.callback["fill-hide"] = function (result) {
    var t = this;
    t.dom.container.getElements("[data-fill-hide]").each(function (item) {
        if (item.get("data-fill-hide") == "true") return;
        var name = item.get("name");
        if (!name) return;
        if (result.info[name] && result.info[name] != "") {
            item.set("data-fill-hide", "disabled");
        }
    });
    t.dom.container.getElements("[data-fill-hide=true]").dispose();
    t.dom.container.getElements("[data-fill-hide=disabled]").set({
        "name": null,
        "disabled": true
    });
};

// 表单提交成功之后返回
BW.callback["mobile-back"] = function (result) {
    Back();
};

// 用户中心
BW.callback["user-center"] = function (result) {
    GolbalSetting.showNewMessage();
    var t = this;
    var parent = t.dom.element.getParent();
    !function () {
        var sound = parent.getElement("[data-sound]");
        if (!sound) return;
        var icon = sound.getElement("i");
        icon.set("class", UI.SoundState() ? "am-icon-volume-up blue" : "am-icon-volume-off blue");
        sound.addEvent("touchend", function (e) {
            icon.set("class", UI.SoundSwitch() ? "am-icon-volume-up blue" : "am-icon-volume-off blue");
        });
    }();
}

//用户的手机号码绑定
BW.callback["mobile-info-mobile"] = function (result) {
    var t = this;
    if (!result.success) {
        return;
    }

    if (result.info.Mobile != "") {
        BW.callback["fill-hide"].apply(t);
        return;
    }

    var mobile = t.dom.element.getElement("[name=Mobile]")
    // 发送验证码
    t.dom.element.getElement("[data-type=sendcode]").addEvent("touchend", function () {
        if (!Regex.test("mobile", mobile.get("value"))) {
            new BW.Tip("手机号码格式错误");
            return;
        }
        var send = this;
        if (send.get("disabled")) return;
        new Request.JSON({
            "url": "/handler/user/info/sendcode",
            "onRequest": function () {
                send.set({
                    "disabled": true,
                    "value": "正在发送..."
                });
            },
            "onComplete": function () {
                send.set({
                    "disabled": false,
                    "value": "发送验证码"
                });
            },
            "onSuccess": function (codeResult) {
                new BW.Tip(codeResult.msg, {
                    "callback": function () {
                        if (codeResult.success) {
                            new Element("span", {
                                "class": "green padding-top-10 right",
                                "text": "验证码发送成功"
                            }).inject(send, "after");
                            send.dispose();
                        }
                    }
                });
            }
        }).post({
            "mobile": mobile.get("value")
        });
    });
};

// 银行信息
BW.callback["mobile-info-bank"] = function (result) {
    var t = this;

    var name = t.dom.element.getElement("[name=AccountName]");
    var account = t.dom.element.getElement("[name=Account]");
    var button = t.dom.element.getElement("[type=submit]");
    var typeName = t.dom.element.getElement(".typeName");
    var type = t.dom.element.getElement("[name=Type]");

    if (!result.success) return;

    if (result.info["AccountName"]) {
        name.set({
            "disabled": true,
            "name": null
        });
    }


    account.addEvents({
        "change": function (e) {
            var obj = this;
            var value = this.get("value");
            if (!/^\d{16,20}$/.test(value)) {
                this.set("value", "");
                this.select();
                return;
            }

            new Request.JSON({
                "url": "/handler/user/info/checkaccount",
                "onRequest": function () {
                    obj.set("disabled", true);
                },
                "onComplete": function () {
                    obj.set("disabled", false);
                },
                "onSuccess": function (bank) {
                    if (!bank.success) {
                        button.set("disabled", true);
                        new BW.Tip(bank.msg, {
                            "callback": function () {
                                obj.set("value", "");
                                obj.focus();
                            }
                        });
                        return;
                    } else {
                        button.set("disabled", false);
                    }
                    typeName.set("text", bank.info.Bank);
                    type.set("value", bank.info.Type);
                }
            }).post({
                "Account": value
            });
        }
    });

    button.addEvent("touchend", function (e) {
        if (!name.get("disabled") && !Regex.test("realname", name.get("value"))) {
            e.stop();
            new BW.Tip("开户名请输入中文，2~5位之间。<br /> 如有特殊字符的客户请与客服联系。");
        }
    });

};

// 安全问题
BW.callback["mobile-info-question"] = function (result) {
    var t = this;
    if (!result.success) return;

    var question = t.dom.element.getElement("[name=Question]");
    var answer = t.dom.element.getElement("[name=Answer]");
    var count = 0;
    Object.forEach(result.info, function (value, key) {
        new Element("option", {
            "text": value,
            "value": key
        }).inject(question);
        count++;
    });

    if (count == 1) {
        question.set("disabled", true);
        new Element("div", {
            "class": "txt",
            "html": "<span class=\"red\">您已设定安全问题</span>"
        }).inject(answer, "after");

        BW.callback["fill-hide"].apply(t);
    }
};

// 移动端充值
BW.callback["mobile-finance-recharge"] = function (result) {
    var t = this;

    var money = t.dom.element.getElement("[name=Money]");
    var bank = t.dom.element.getElement("[name=BankCode]");
    var payId = t.dom.element.getElement("[name=PayID]");

    var list = t.dom.element.getElements("a");
    list.each(function (item, index) {
        item.store("payment", result.info.list[index]);
    });

    list.addEvent("touchend", function (e) {
        this.addClass("current");
        var id = this.get("data-id");
        payId.set("value", id);
        var payment = this.retrieve("payment");
        console.log(payment);
        money.set({
            "min": payment.MinMoney,
            "max": payment.MaxMoney,
            "placeholder": "充值金额（单笔" + htmlFunction["money"](payment.MinMoney) + "～" + htmlFunction["money"](payment.MaxMoney) + "元）"
        });
        var banklist = new Array();
        if (payment.Bank) {
            Object.forEach(payment.Bank, function (value, key) {
                banklist.push({ "text": value, "value": key });
            });
        } else {
            banklist.push({ "text": payment.Name, "value": "" });
        }
        banklist.bind(bank);
    });
    if (list.length == 0) {
        new BW.Tip("系统暂不支持移动端充值", {
            "callback": function () {
                BW.callback["mobile-error"].apply(t, [result]);
            }
        });
        return;
    }
    list[0].fireEvent("touchend");
};

// 移动端充值提交成功
BW.callback["mobile-finance-recharge-submit"] = function (result) {
    var t = this;
    if (result.success) {
        new BW.Tip("点击确认进入充值页面", {
            "callback": function () {
                var url = "user/finance-recharge-submit.html?id=" + result.info.OrderID + "&bank=" + result.info.Bank;
                userOpen(url);
                BW.callback["mobile-back"].apply(t, [result]);
            }
        });
    } else {
        new BW.Tip(result.msg);
    }
};

// 提现页面
BW.callback["finance-withdraw"] = function (result) {
    var t = this;
    if (!result.success) return;
    var bank = t.dom.element.getElement("[name=BankID]");
    result.info.BankAccount.bind(bank, {
        "text": "Value",
        "value": "ID",
        "disabled": "IsWithdraw"
    });

};

// 平台转账
BW.callback["finance-transfer"] = function (result) {
    var t = this;
    if (!result.success) return;

    // 转入列表
    var select1 = t.dom.element.getElement("select[name=In]");
    var select2 = t.dom.element.getElement("select[name=Out]");
    var obj = new Object();
    t.dom.element.getElements("[data-type]").each(function (item) {
        obj[item.get("data-type")] = item;
    });
    result.info.each(function (item) {
        //.filter(function (item) { return item.Type != ""; })
        if (item.Type) {
            [select1, select2].each(function (select) {
                new Element("option", {
                    "text": item.Name + (item["disabled"] ? "(未注册)" : ""),
                    "value": item.Type,
                    "disabled": item["disabled"],
                    "data-disabed": item["disabled"],
                    "data-money": item["Money"],
                    "data-withdraw": item["Withdraw"]
                }).inject(select);
            });
        } else {
            obj["UserMoney"].set("text", htmlFunction["money"](item.Money));
        }
    });

    select2.addEvent("change", function () {
        var option = $(this.options[this.selectedIndex]);
        obj["Money"].set("text", htmlFunction["money"](option.get("data-money")));
        obj["Withdraw"].set("text", htmlFunction["money"](option.get("data-withdraw")));
    });
    if (select2.selectedIndex == -1) {
        new BW.Tip("当前暂未开放第三方游戏", {
            "callback": BW.callback["mobile-back"]
        });
        return;
    }
    select2.fireEvent("change");

    tabSwitch(t.dom.element.getElements(".header-info .bar a"), t.dom.element.getElements(".display-none"));
};

// 新闻浏览 - 上一篇下一篇
BW.callback["home-news-nav"] = function (result) {
    var t = this;

    if (!result.success) {
        new BW.Tip(result, {
            "callback": function () {
                Back();
            }
        });
        return;
    }

    var previous = t.dom.element.getElement("[data-action=previous]");
    var next = t.dom.element.getElement("[data-action=next]");

    if (result.info.Previous != null) {
        previous.set({
            "text": result.info.Previous.Title,
            "data-newsid": result.info.Previous.ID
        });
    }

    if (result.info.Next != null) {
        next.set({
            "text": result.info.Next.Title,
            "data-newsid": result.info.Next.ID
        });
    }

    if (!t.dom.element.get("data-click")) {
        t.dom.element.set("data-click", true);
        t.dom.element.addEvent("touchend", function (e) {
            var obj = $(e.target);

            if (obj.get("data-action") == null) return;
            var newsId = obj.get("data-newsid");
            if (newsId == "0") return;

            t.setOptions({
                "data": "id=" + newsId
            });
            t.fire();
        });
    }
};

// 客服接口 

// 好友列表的初始化
BW.callback["service-init"] = function () {
    var result = layui.layim.cache();
    var t = this;
    var list = t.dom.element.getElement(".layui-layim-list");
    var index = 0;
    result.friend.each(function (friend, groupIndex) {
        friend.list.each(function (item) {
            index++;
            var li = new Element("li", {
                "data-type": "friend",
                "data-index": index,
                "id": "layim-friend" + item.id,
                "html": "<img src=\"${avatar}\"><label class=\"message\"></label><span>${username}</span><p>${sign}</p>".toHtml(item),
                "events": {
                    "touchend": function () {
                        OpenFrame('user/service-log.html', item.username, 'id=' + item.id);
                    }
                }
            }).inject(list);
        });
    });
    layui.layim.tip();
};

// 聊天窗口的初始化
BW.load["layim-chat-log-init"] = function () {
    var t = this;
    // 減去頭部底部
    t.dom.element.setStyle("height", DOM.height - 100);
    var parent = t.dom.element.getParent();
    t.dom.element.getElement("ul").set("id", t.options.data.id);
    layui.layim.tip(t.options.data.id);

    var footer = parent.getElement(".layim-chat-footer");
    var form = footer.getElement("form");
    var input = form.getElement(".layim-chat-textarea input");
    input.addEvents({
        "focus": function () {
            parent.addClass("layim-chat-focus");
            t.dom.element.scrollTo.delay(100, t.dom.element, [0, t.dom.element.getElement("ul").getHeight()]);
        },
        "blur": function () {
            parent.removeClass("layim-chat-focus");
            t.dom.element.scrollTo.delay(100, t.dom.element, [0, t.dom.element.getElement("ul").getHeight()]);
        }
    });

    // 发送信息
    var sendMessage = function (content) {
        if (content == "") return;
        layui.layim.events["sendMessage"]({
            "to": {
                "id": t.options.data.id
            },
            "mine": {
                "content": content
            }
        });
        layui.layim.showMessage({
            "id": t.options.data.id,
            "content": content
        });
        form.reset();
    };

    form.getElement("input[name=file]").addEvent("change", function () {
        var action = layui.layim.cache().config.uploadImage.url;
        var oReq = new XMLHttpRequest();
        oReq.open("POST", action, true);
        oReq.onload = function (oEvent) {
            GolbalSetting.loading(true);
            if (oReq.status == 200) {
                var result = JSON.decode(oReq.response);
                console.log(result.code);
                if (result.code == 0) {
                    sendMessage("img[" + result.data.src + "]");
                } else {
                    alert(result.msg);
                }
            } else {
                console.log(oReq);
            }
        };
        oReq.send(new FormData(form));
        GolbalSetting.loading();
    });

    // 發送文本信息
    form.addEvent("submit", function (e) {
        if (e) e.stop();
        var content = input.get("value");
        sendMessage(content);
    });
};

// 歷史聊天記錄加載完成之後
BW.callback["layim-chat-log-list"] = function (result) {
    var t = this;
    var id = t.options.data.id;
    result.info.list.each(function (item) {
        item.id = id;
        layui.layim.showMessage(item);
    });

    t.dom.element.scrollTo.delay(500, t.dom.element, [0, t.dom.element.getElement("ul").getHeight()]);
};

// 删除绑定后列表的第一个元素
BW.callback["list-remove-before-1"] = function (result) {
    var t = this;
    var li = t.dom.element.getElement("[data-list-element] > li");
    if (li != null) li.dispose();
};

// 新建下级的链接
BW.callback["mobile-team-createinfo"] = function (result) {
    var t = this;
    if (!result.success) return;
    var opt = t.dom.element.getElement("optgroup");
    result.info.Rebate.reverse();
    result.info.Rebate.each(function (item) {
        new Element("option", {
            "text": item,
            "value": item
        }).inject(opt);
    });
};

// 彩票投注记录
BW.callback["report-lottery-list"] = function (result) {
    var t = this;
    if (!result.success) return;
    t.dom.element.getElements("[data-status]").each(function (item) {
        var reward = item.get("data-reward").toFloat();
        var status = item.get("data-status");
        if (reward > 0) {
            item.set("html", "<span class=\"green\">+" + htmlFunction["money"](reward) + "</span>");
        } else {
            item.set("html", status);
        }
    });
};

// 彩票投注详情
BW.callback["lottery-detail"] = function (result) {
    var t = this;
    if (!result.success) return;
    var action = t.dom.element.getElement("[data-type=IsAction]");
    if (result.info.IsRevoke == "0") {
        action.dispose();
    }
    if (result.info.LotteryAt.contains("1900")) {
        t.dom.element.getElement("[data-type=LotteryAt]").dispose();
    }
    if (result.info.Reward.toFloat() == 0) {
        t.dom.element.getElement("[data-type=IsReward]").dispose();
    }

    t.dom.element.getElements("[data-type]").addEvent("click", function () {
        switch (this.get("data-type")) {
            case "revoke":
                new BW.Tip("确认要撤单吗？", {
                    "type": "confirm",
                    "callback": function () {

                        new Request.JSON({
                            "url": "/handler/user/lottery/orderrevoke",
                            "onRequest": function () {
                                t.dom.container.addClass("lottery-loading");
                            },
                            "onComplete": function () {
                                t.dom.container.removeClass("lottery-loading");
                            },
                            "onSuccess": function (revoke) {
                                new BW.Tip(revoke.msg, {
                                    "callback": function () {
                                        if (revoke.success) {
                                            BW.callback["mobile-back"].apply(t);
                                        }
                                    }
                                });
                            }
                        }).post({
                            "id": result.info.ID
                        });

                    }
                })
                break;
        }
    });
};


// =========== 彩票游戏页面 =================


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
            "name": null
        },
        "dom": {
            "id": null,
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

            t.dom.id = "lottery-" + Math.random();
            t.dom.element.set("id", t.dom.id);

            t.running = true;

            t.request = new Request.JSON({
                "url": "/handler/game/lottery/index",
                "onSuccess": function (result) {
                    if (!result.success) {
                        new BW.Tip(result.msg, {
                            "callback": function () {

                            }
                        });
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
            if (!$(t.dom.id)) {
                t.dispose();
                return;
            }
            t.dom.objs.each(function (obj) {
                var name = obj.get("data-resulttime");
                var value = info[name];
                if (value == undefined) return;
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
                        GolbalSetting.UpdateTimer(obj);
                        break;
                    case "OpenNumber":
                        var openIndex = info.OpenIndex.contains("-") ? info.OpenIndex.substring(info.OpenIndex.indexOf("-") + 1) : info.OpenIndex;

                        if (value == "") {
                            if (!obj.hasClass("loading")) {
                                obj.addClass("loading");
                                t.dom.numbers.set("text", "");
                                UI.SoundText(t.options.name + "第" + openIndex + "期正在开奖");
                            }
                        } else {
                            if (obj.hasClass("loading")) {
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
            t.dom.element = t.dom.id = t.dom.objs = t.dom.numbers = null;
            t.running = false;
            t.request = null;
        }
    });

})(Lottery);


// 彩票玩法页面初始化
BW.callback["lottery-player"] = function (result) {
    var t = this;
    // 填充玩法
    !function () {
        // 保存默认玩法
        var saveDefaultPlayer = function (name, value) {
            var player = JSON.decode(localStorage.getItem("player") || "{}") || new Object();
            if (value) {
                player[name] = value;
                localStorage.setItem("player", JSON.encode(player));
            } else {
                return player[name];
            }
        };

        var pleryName = t.dom.element.getElement(".lottery-player-name h3");
        var player = t.dom.element.getElement(".lottery-player-list");
        var group = new Object();
        var defaultPlayer = null;
        result.info.Player.each(function (item) {
            if (!group[item.GroupName]) {
                group[item.GroupName] = new Array();
            }
            group[item.GroupName].push(item);
        });
        Object.forEach(group, function (value, key) {
            var item = new Element("div", {
                "class": "lottery-player-list-group"
            });
            new Element("label", {
                "text": key
            }).inject(item);
            var code = saveDefaultPlayer(result.info.Type);
            value.each(function (p) {
                var playerObj = new Element("a", {
                    "href": "javascript:",
                    "text": p.PlayName,
                    "data-code": p.Code,
                    "data-playerid": p.ID,
                    "data-name": p.LabelName + "_" + p.PlayName,
                    "data-tip": p.Tip,
                    "data-reward": p.RewardMoney,
                    "data-name": p.GroupName + " " + p.LabelName + " " + p.PlayName,
                    "events": {
                        "click": function (e) {
                            pleryName.set("text", this.get("data-name"));
                            BW.callback["lottery-player-selected"].apply(t, [this, result]);
                            saveDefaultPlayer(result.info.Type, p.Code);
                        }
                    }
                });
                playerObj.inject(item);
                if (!defaultPlayer || code == p.Code) {
                    defaultPlayer = playerObj;
                }
            });
            item.inject(player);
        });
        if (defaultPlayer) defaultPlayer.fireEvent("click");
    }();

    //选择玩法的按钮
    !function () {
        var player = t.dom.element.getElement(".lottery-player-name");

        var list = t.dom.element.getElement(".lottery-player-list");

        t.dom.element.getElements(".lottery-player-select, .lottery-player-list-group a").addEvent("click", function () {
            var height = t.dom.element.getHeight();
            list.setStyle("max-height", height);
            player.toggleClass("show");
        });
    }();

    //开奖器（倒计时）
    !function () {
        new Lottery.Time(t.dom.element, {
            "type": result.info.Type,
            "name": result.info.Game
        });
    }();

    // 号码区域的点击事件
    !function () {
        // 号码区的点击事件
        t.dom.container.getElement(".lottery-player-selector-ball").addEvent("touchend", function (e) {
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
    }();

    // 模式选择
    !function () {
        t.dom.container.getElement("[data-dom=mode]").addEvent("touchend", function (e) {
            var obj = $(e.target);
            if (obj.get("tag") != "a") return;
            setCurrent(this.getElements("a"), obj);
            var player = t.dom.element.retrieve("player");
            player.getMoney();
        });
    }();

    // 倍数选择
    !function () {
        t.dom.container.getElement("[data-dom=times]").addEvents({
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
    }();

    // 快速投注按钮
    !(function () {
        t.dom.container.getElement("[data-dom=quick]").addEvent("touchend", function () {
            var player = t.dom.element.retrieve("player");
            if (player.data.submit.money < 0.01) {
                new BW.Tip("投注金额错误");
                return;
            }

            var data = new Array();
            data.push(player.selected());

            new Request.JSON({
                "url": "/handler/user/lottery/save",
                "onRequest": function () {
                    t.dom.container.addClass("lottery-loading");
                },
                "onComplete": function () {
                    t.dom.container.removeClass("lottery-loading");
                },
                "onSuccess": function (result) {
                    new BW.Tip(result.msg, {
                        "callback": function () {
                            if (result.success) {

                            }
                        }
                    });
                }
            }).post(JSON.encode(data))
        });
    })();

    // 投注记录赋值
    !function () {
        //
        var log = t.dom.element.getElement("[data-id=betlog]");
        if (log) {
            log.set("href", "javascript:OpenFrame('user/report-lottery.html','" + result.info.Game + "投注记录','Type=" + result.info.Type + "');");
        }
    }();

    // 走势图
    !function () {
        var trend = t.dom.element.getElement("[data-trend]");
        if (!trend) return;
        trend.addEvent("click", function () {
            var url = trend.get("data-trend") + "?" + result.info.Type;
            window.open(url, "_blank");
        });
    }();
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

    var container = t.dom.element.getElement(".lottery-player-selector");

    var obj = new Lottery[category][playerId]({
        "bind": t,
        "id": player.get("data-playerid"),
        "name": player.get("data-name"),
        "code": player.get("data-code"),
        "reward": player.get("data-reward").toFloat()
    });

    t.dom.element.store("player", obj);
    if (t.dom.element.retrieve("playerCurrent") != null) {
        t.dom.element.retrieve("playerCurrent").removeClass("current");
    }
    player.addClass("current");
    t.dom.element.store("playerCurrent", player);
};


/* =============== 格式化  =================== */
htmlFunction["money"] = function (value) {
    var money = value.toFloat();
    if (isNaN(money)) return value;

    return money.ToString("n");
};

// 资金流水
htmlFunction["trunover"] = function (value) {
    var money = value.toFloat();
    if (isNaN(money)) return value;

    if (money > 0) {
        return "<label class=\"green\">+" + money.ToString("n") + "</label>";
    } else if (money < 0) {
        return "<label class=\"red\">" + money.ToString("n") + "</label>";
    }
    return money.ToString("n");
};

//站内信是否已读
htmlFunction["message-read"] = function (value) {
    return value == "true" ? "<em class=\"isread\">已读</em>" : "<em class=\"unread\">未读</em>";
};

// 布尔类型的格式化
htmlFunction["Status"] = function (value) {
    switch (value) {
        case "true":
            value = "<span class=\"green\">可用</span>";
            break;
        case "false":
            value = "<span class=\"red\">不可用</span>";
            break;
    }
    return value;
};

// 下級在線
htmlFunction["IsOnline"] = function (value) {
    switch (value) {
        case "true":
            value = "<span class=\"green\">在线</span>";
            break;
        case "false":
            value = "<span class=\"gray\">离线</span>";
            break;
    }
    return value;
};

// 小寫
htmlFunction["lower"] = function (value) {
    return value.toLowerCase();
};

// 是否是自己發出的信息
htmlFunction["layim-chat-mine"] = function (value) {
    switch (value) {
        case "1":
            value = "layim-chat-mine";
            break;
        default:
            value = "";
            break;
    }
    return value;
}

// 彩票的模式
htmlFunction["lottery-mode"] = function (value) {
    var mode = value.toFloat();
    if (isNaN(mode)) return value;
    switch (mode) {
        case 1:
            value = "元";
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

/* =============  公共方法  ================ */

// 绑定tab切换事件（自动触发第一个事件）
function tabSwitch(menu, objs, className) {
    if (!className) className = "current";

    if (menu.length != objs.length) return;

    var currentMenu = null, currentObj = null;

    menu.addEvent("touchend", function (e) {
        var index = menu.indexOf(this);
        if (currentMenu != null) currentMenu.removeClass(className);
        if (currentObj != null) currentObj.removeClass(className);

        this.addClass(className);
        objs[index].addClass(className);

        currentMenu = this;
        currentObj = objs[index];
    });

    menu[0].fireEvent("touchend");
}

// 弹出新窗口
function userOpen(url, target, noSession) {
    if (!target) target = "_system";
    if (!noSession) {
        url += ((url.contains("?") ? "&" : "?") + "session=" + GolbalSetting.User.Session);
    }
    window.open(url, target);
}

// 加载用户信息
function loadUser() {
    var t = this;
    new Request.JSON({
        "url": "/handler/user/info/get",
        "onComplete": function () {
            loadUser.delay(10 * 1000);
        },
        "onSuccess": function (result) {
            if (!result.success) {
                new BW.Tip(result.msg, {
                    "callback": function () {
                        BW.callback["mobile-error"].apply(t, [result]);
                    }
                })
                return;
            }
            GolbalSetting.setUserInfo(result.info);
        }
    }).post();
};

// 阅读站内信
function Read(obj) {
    obj = $(obj);
    var read = obj.getElement(".unread");
    if (read == null) return;
    read.set({
        "class": "isread",
        "text": "已读"
    });
}

// 错误处理回调
window["error"] = {
    "Login": function (result) {
        var t = this;
        document.body.empty();
        location.href = "login.html";
    },
    "PayPassword": function (result) {
        OpenFrame('user/info-paypassword.html', '修改资金密码');
    },
    "BankAccount": function (result) {
        Back();
    }
};


// 加载APP资源
(function () {
    if (!/x5app/i.test(window.navigator.userAgent)) return;
    document.writeln("<script type=\"text/javascript\" src=\"/cordova.js\"></script>");
    APP = new Object();
    document.addEventListener('deviceready', function () {
        // 清除APP缓存
        APP["ClearCache"] = function () {
            window.cache.clear(function (status) {
                alert('缓存清除成功');
            }, function (status) {
                alert('缓存清理失败，' + status);
            });
        };
    });
})();


// 系统加载事件
window.addEvent("domready", function () {

    DOM.width = document.body.getWidth();
    DOM.height = document.body.getHeight();
    DOM.frames = $("mobile-frames");
    DOM.title = $("mobile-title");
    DOM.footer = $$("footer").getLast();

    OpenFrame('user/home.html', '游戏广场');
});

