// 彩票模式对应的金额
var MODE = {
    "元": 1,
    "二元": 2,
    "一元": 1,
    "五角": 0.5,
    "角": 0.2,
    "二角": 0.2,
    "一角": 0.1,
    "五分": 0.05,
    "分": 0.02,
    "二分": 0.02,
    "一分": 0.01,
    "厘": 0.002,
    "二厘": 0.002,
    "一厘": 0.001,
    "毫": 0.0002
};

// 更新一个可编辑元素
(function (ns) {

    ns.Update = new Class({
        Implements: [Events, Options],
        "options": {
            "action": null,
            "name": null
        },
        "dom": {
            // 当前元素
            "element": null,
            "tag": null,
            "type": null
        },
        // 提示错误并且还原
        "restore": function (msg) {
            var t = this;
            var el = t.dom.element;
            new BW.Tip(msg, {
                "callback": function () {
                    switch (t.dom.type) {
                        case "checkbox":
                            el.set("checked", el.retrieve("value"));
                            break;
                        default:
                            el.set("value", el.retrieve("value"));
                            break;
                    }
                }
            });
        },
        "initialize": function (el, options) {
            var t = this;
            t.dom.element = el = $(el);
            t.setOptions(options);

            Object.forEach(Element.GetAttribute(el, "data-update-"), function (value, key) {
                t.options[key] = value;
            });

            var tag = el.get("tag");
            var type = null;
            switch (tag) {
                case "input":
                    switch (el.get("type")) {
                        case "checkbox":
                        case "radio":
                            type = "checkbox";
                            if (el.get("data-checked") == "true") {
                                el.set("checked", true);
                            }
                            break;
                        default:
                            type = "text";
                            break;
                    }
                    break;
                case "select":
                case "textarea":
                    type = "text";
                    break;
            }

            t.dom.tag = tag;
            t.dom.type = type;

            el.addEvents({
                "focus": function (e) {
                    switch (type) {
                        case "checkbox":
                            this.store("value", this.get("checked"));
                            break;
                        default:
                            this.store("value", this.get("value"));
                            break;
                    }
                },
                "change": function (e) {

                    var data = Object.clone(t.options);

                    switch (type) {
                        case "checkbox":
                            data["value"] = el.get("checked") ? 1 : 0;
                            break;
                        case "text":
                            data["value"] = el.get("value");
                            break;
                    }

                    new Request.JSON({
                        "url": t.options.action,
                        "onRequest": function () {
                            el.set("disabled", true);
                        },
                        "onComplete": function () {
                            el.set("disabled", false);
                        },
                        "onFailure": function (xhr) {
                            t.restore(xhr.statusText);
                        },
                        "onSuccess": function (result) {
                            if (!result.success) {
                                t.restore(result.msg);
                                return;
                            }
                            el.highlight();
                            new BW.Tip(result.msg, {
                                "delay": 3000,
                                "type": "tip",
                                "mask": false,
                                "drag": false
                            });
                        }
                    }).post(data);
                }
            });
        }
    });

})(BW);

// 二级菜单的联动
(function (ns) {
    ns.Select = new Class({
        "Implements": [Events, Options],
        "options": {
            // 
            "bind": function (result) {
                var t = this;
                t.dom.target.empty();
                result.info.bind(t.dom.target)
            },
            "url": null,
            // 自动触发
            "auto": false
        },
        "dom": {
            // 当前元素
            "element": null,
            "target": null
        },
        "initialize": function (el, target, options) {
            var t = this;
            t.setOptions(options);
            t.dom.element = el = $(el);
            t.dom.target = target = $(target);

            var data = Element.GetAttribute(el, "data-select-");

            t.dom.element.addEvent("change", function (e) {
                var obj = this;
                data["value"] = this.get("value");
                new Request.JSON({
                    "url": t.options.url,
                    "onRequest": function () {
                        target.set("disabled", true);
                        obj.set("disabled", true);
                    },
                    "onComplete": function () {
                        target.set("disabled", false);
                        obj.set("disabled", false);
                    },
                    "onSuccess": function (result) {
                        if (!result.success) {
                            new BW.Tip(result.msg);
                            return;
                        }
                        t.options.bind.apply(t, [result]);

                        t.dom.target.fireEvent("change");

                    }
                }).post(data)
            })

            if (t.options.auto) {
                t.dom.element.fireEvent("change");
            }
        }
    });
})(BW);

// 点击图片上传新图片，并且更新
(function (ns) {

    ns.callback["image-upload"] = function () {
        var t = this;
        t.dom.element.getElement("img")
    };

})(BW);

htmlFunction["switch"] = function (input) {
    var html = new Array();
    html.push("<input type=\"checkbox\" data-type=\"switch\"");
    switch (input) {
        case "true":
        case "1":
            html.push(" checked=\"checked\"");
            break;
    }
    html.push("/>");
    return html.join("");
};

htmlFunction["checked"] = function (input) {
    var html = new Array();
    switch (input) {
        case "true":
        case "1":
            html.push("true");
            break;
        default:
            html.push("false");
            break;
    }
    return html.join("");
};

htmlFunction["switch-checked"] = function (value) {
    var html = null;
    switch (value) {
        case "true":
        case "1":
            html = "checked=\"checked\"";
            break;
    }
    return html;
};

// 判断开启或者关闭
htmlFunction["open"] = function (input) {
    switch (input) {
        case "true":
        case "1":
            input = "<label class=\"am-text-success\">开启</label>";
            break;
        case "false":
        case "0":
            input = "<label class=\"am-text-danger\">关闭</label>";
            break;
    }
    return input;
}

// 格式化金额
htmlFunction["money"] = function (input) {
    if (isNaN(input)) return input;
    var list = new Array();
    var str = input.toFloat().ToString("n");
    var x = str.split('.');
    var x1 = x[0];
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1,$2');
    }
    return "￥" + x1 + "." + x[1];
}

// 金额按征服数显示
htmlFunction["money-show"] = function (value) {
    var money = value.toFloat();
    if (isNaN(money)) return value;
    if (money > 0) {
        return "<span class=\"am-text-success\">+" + htmlFunction["money"](money) + "<span>";
    } else if (money < 0) {
        return "<span class=\"am-text-danger\">" + htmlFunction["money"](money) + "<span>";
    }
    return "<span class=\"am-text-default\">" + htmlFunction["money"](money) + "<span>";
};


// 彩票的金额模式
htmlFunction["mode-show"] = function (value) {
    value = value.toFloat();
    var result = null;
    Object.each(MODE, function (mode, key) {
        if (result) return;
        if (mode == value) {
            result = key;
        }
    });
    if (!result) result = value;
    return result;
};

// 日期显示
htmlFunction["datetime"] = function (input) {
    if (input.contains("1900")) return "N/A";
    return input;
}

// 获取自定义的资金类型
htmlFunction["moneytype"] = function (value) {
    return GolbalSetting.Site.MoneyType[value] || value;
};

// 标记是测试帐号
htmlFunction["istest"] = function (value) {
    switch (value) {
        case "true":
        case "1":
            value = "<span class=\"am-text-danger am-text-xs\">(测试)</span>";
            break;
        default:
            value = "";
            break;
    }
    return value;
};

// 日期
htmlFunction["date"] = function (value) {
    if (value.contains("1900")) return "N/A";
    var exec = /^(\d{4})[\-\/](\d{1,2})[\-\/](\d{1,2})\s/.exec(value);
    if (!exec) return value;
    return [exec[1], "年", exec[2], "月", exec[3], "日"].join("");
};

// 人民币转美元
htmlFunction["money-doller"] = function (value) {
    var money = value.toFloat();
    if (isNaN(money)) return value;
    return htmlFunction["money"](money / 7).replace("￥", "$");
};

// 人民币转菲律宾比索
htmlFunction["money-pesos"] = function (value) {
    var money = value.toFloat();
    if (isNaN(money)) return value;
    return htmlFunction["money"](money * 7).replace("￥", "₱");
};

// 彩票模式
htmlFunction["lottery-mode"] = function (value) {
    var obj = {
        "1": "元",
        "0.1": "角", "0.01": "分", "0.001": "厘"
    };
    if (obj[value]) return obj[value];
    return value;
}

// 给加载之后的元素绑定update事件
BW.callback["update"] = function (result) {
    var t = this;
    var name = "data-bind-update-action";
    t.dom.element.getElements("[data-update-name]").each(function (item) {
        var action = item.get(name) || t.dom.element.get(name);
        if (action == null) {
            var parent = item.getParent("[" + name + "]");
            if (parent != null) action = parent.get(name);
        }
        new BW.Update(item, {
            "action": action
        });
    });
}

// 自动触发加载设定项内容
BW.callback["loadsetting"] = function (result) {
    var t = this;
    t.dom.element.getElements("td select[data-loadsetting]").each(function (item) {
        if (!item.get("data-loadsetting-change")) return;
        item.fireEvent("change");
    });
}

// 加载出Setting的设置输入框
BW.callback["load-setting"] = function (result) {
    var t = this;
    if (!result.success || !result.info.Setting) return;
    var tr = t.dom.element.getElement("tr[data-loadsetting]");
    if (tr == null) return;
    tr.getAllNext("tr.setting").dispose();
    result.info.Setting.each(function (item) {
        var newItem = new Element("tr", {
            "class": "setting",
            "html": "<th>${Description}：</th><td><input type=\"text\" name=\"Setting.${Name}\" class=\"txt am-form-field p80\" value=\"${Value}\" /></td>".toHtml(item)
        });
        newItem.inject(tr, "after");
    });
}

// 全选按钮
BW.callback["selectall"] = function () {
    var t = this;

    t.dom.element.getElements("input[data-selectall]").addEvent("click", function () {
        var name = this.get("data-selectall");
        if (!name) return;
        var checked = this.get("checked");
        t.dom.element.getElements("input[name=" + name + "]").each(function (item) {
            item.set("checked", checked);
        });
    });
};

// 绑定枚举
BW.load["enum"] = function () {
    var t = this;
    BW.callback["enum"].apply(t);
}

// 加载控件上的载入设置选项的内容
BW.load["loadsetting"] = function () {
    var t = this;
    t.dom.element.getElements("td select[data-loadsetting]").each(function (item) {
        if (item.get("data-loadsetting-change")) return;
        item.set("data-loadsetting-change", 1);
        var url = item.get("data-loadsetting");
        var tr = item.getParent("tr");
        item.addEvent("change", function () {
            var data = Element.GetAttribute(item);
            data["type"] = this.get("value");

            new Request.JSON({
                "url": url,
                "onRequest": function () {
                    item.set("disabled", true);
                },
                "onComplete": function () {
                    item.set("disabled", false);
                },
                "onSuccess": function (result) {
                    tr.getAllNext("tr.setting").dispose();
                    if (!result.success) return;
                    var list = result.info;
                    list.each(function (item) {
                        var newItem = new Element("tr", {
                            "class": "setting",
                            "html": "<th>${Description}：</th><td><input type=\"text\" name=\"Setting.${Name}\" class=\"txt am-form-field p80\" value=\"${Value}\" /></td>".toHtml(item)
                        });
                        newItem.inject(tr, "after");
                    });
                }
            }).post(data);
        });
    });
}

// 绑定select条所需要的枚举
BW.callback["enum"] = function (result) {
    var t = this;
    t.dom.element.getElements("[data-enum-name]").each(function (item) {
        var name = item.get("data-enum-name");

        if (GolbalSetting.Enum[name]) {
            var list = Array.clone(GolbalSetting.Enum[name]);
            switch (item.get("tag")) {
                case "select":
                    if (item.get("data-enum-none")) {
                        list.unshift({ "value": "", "text": item.get("data-enum-none") });
                    }
                    list.bind(item)
                    item.removeProperty("data-enum-name");
                    break;
                default:
                    var enumValue = list.filter(function (em) { return em.value == item.get("text"); }).getLast();
                    if (enumValue != null) item.set("text", enumValue.text);
                    break;
            }
        } else {
            console.log("GolbalSetting中未包含设定的枚举值" + name);
        }
    });
}

// 绑定所有的彩种到一个select上
BW.load["lottery-game"] = function () {
    var t = this;
    var obj = t.dom.element.getElement("select[data-lottery-game]");
    if (!obj || !GolbalSetting["Lottery"]) return;
    obj.empty();
    Object.forEach(GolbalSetting["Lottery"], function (game, key) {
        var group = new Element("optgroup", {
            "label": key
        });
        Object.forEach(game, function (text, value) {
            new Element("option", {
                "value": value,
                "text": text,
                "disabled": text.contains("未开放")
            }).inject(group);
        });
        group.inject(obj);
    });

    console.log(obj);
};

!function () {
    var rehcargeId = new Array();

    // 获取菜单上的新消息提示
    var getMenuTip = function () {
        $$("#aside menu dt").each(function (dt) {
            var dd = dt.getNext("dd");
            if (dd == null) return;
            var count = 0;
            dd.getElements("a .tip").each(function (item) { count += item.get("text").toInt(); });
            var tip = dt.getElement(".tip");
            if (count == 0) {
                if (tip != null) tip.dispose();
            } else {
                if (tip == null) {
                    tip = new Element("span", {
                        "class": "am-badge am-badge-danger am-round tip"
                    });
                    tip.inject(dt);
                }
                tip.set("text", count);
            }
        });

        $$("#aside menu h2").each(function (h2) {
            var dl = h2.getNext("dl");
            if (dl == null) return;
            var count = 0;
            dl.getElements("dt .tip").each(function (item) { count += item.get("text").toInt(); });

            var tip = h2.getElement(".tip");
            if (count == 0) {
                if (tip != null) tip.dispose();
            } else {
                if (tip == null) {
                    tip = new Element("span", {
                        "class": "am-badge am-badge-danger am-round tip"
                    });
                    tip.inject(h2);
                }
                tip.set("text", count);
            }

        });
    };

    // 加载系统提示(30s一次)
    var tip = function () {
        new Request.JSON({
            "url": "/admin/site/tip",
            "onComplete": function () {
                tip.delay(30 * 1000);
            },
            "onSuccess": function (result) {
                var newTip = function (id, count, sound) {
                    var obj = $("aside").getElement("[data-id=" + id + "]");
                    if (obj == null) return;
                    var tipObj = obj.getElement(".tip");

                    if (count) {
                        if (tipObj == null) {
                            tipObj = new Element("span", {
                                "class": "am-badge am-badge-danger am-round tip"
                            });
                            tipObj.inject(obj);
                        }
                        tipObj.set("text", count);
                        UI.Sound("/studio/sound/" + sound + ".mp3");
                    } else {
                        if (tipObj) tipObj.dispose();
                    }
                };

                //系统账单提醒
                newTip("9f7dd3bdaac94615a6fadabcd70a3b99", result.info.Bill, "bill");

                //转账提醒
                newTip("061ab65e41d84a358d36e5e920fe1604", result.info.Transfer, "transfer");

                // 提现通知
                newTip("82e2f7bb54ca4b6d98404a6699d6e6ff", result.info.Withdraw, "withdraw");

                //到期账单提醒
                if (result.info.ExpireBill) {
                    new BW.Tip("系统有账单超时未支付，请尽快处理");
                }

                // 充值通知
                if (result.info.Recharge.length > 0 && !rehcargeId.contains(result.info.Recharge.getLast().ID)) {
                    var html = result.info.Recharge.map(function (item) {
                        if (rehcargeId.contains(item.ID)) return "";
                        rehcargeId.push(item.ID);
                        return "<p style=\"color:white;\">${UserName}充值${Money:money}元</p>".toHtml(item);
                    }).join("");
                    new BW.Tip(html, {
                        "type": "tip",
                        "mask": false
                    });
                    UI.Sound("/studio/sound/recharge.mp3");
                }

                // 没有接收到的客服信息
                if (result.info.Message.length > 0) {
                    result.info.Message.each(function (item) {
                        if (window["IM"] && window["IM"].message) {
                            window["IM"].message(item);
                        }
                    });
                }

                getMenuTip();
            }
        }).post();
    };

    tip.delay(1000 * 5);
}();

// 加载系统的彩种设定
!function () {

    new Request.JSON({
        "url": "/admin/lottery/gamelist",
        "onSuccess": function (result) {
            GolbalSetting["Lottery"] = result.info;
        }
    }).post();

}();
