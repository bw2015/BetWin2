/// <reference path="mootools.js" />
/// <reference path="mootools-more.js" />
/// <reference path="md5.js" />

var KEY = "BETWIN-ORDER";

// 密钥相关配置
var Config = null;

var dom = {
    "tradeno": {
        "selector": ".tradeNo",
        "regex": /:(\d{10,})/
    },
    "amount": {
        "selector": ".amount",
        "regex": /\+ (\d+\.\d{2})/
    },
    "title": {
        "selector": ".consume-title",
        "regex": /(.+)/
    },
    "status": {
        "selector": ".status",
        "regex": /(交易成功)/
    }
};

// 建立一个多次通知渠道
var port = chrome.runtime.connect({ name: KEY });   //通道名称

if (!Config) {
    chrome.extension.sendMessage({}, function (result) {
        Config = result;
        Config.Url = "https://consumeprod.alipay.com/record/advanced.htm?status=success&fundFlow=in";
        !function () {
            var url = location.href;
            if (/\/i.htm$/.test(url)) {
                main.apply(this);
            } else if (location.href == Config.Url) {
                order.apply();
            }
        }();
    });
}

// 首页的执行方法
function main() {

    // 检测当前账户
    var name = $("J-userInfo-account-userEmail");
    if (!name) {
        alert("未检测到当前登录帐号，请使用email账号登录");
        return;
    }
    name = name.get("title");
    if (name != Config["Alipay"]) {
        alert("当前登录帐号" + name + "，与系统配置不一致");
        return;
    }
    document.body.empty();
    var link = new Element("a", {
        "href": "https://consumeprod.alipay.com/record/advanced.htm?status=success&fundFlow=in",
        "text": "插件检测成功",
        "class": "plugin-check"
    });
    link.inject(document.body, "top");
    var time = Math.random() * 5000 + 3000;
    var index = 0;
    (function () {
        link.click();
    }).delay(time);
};

// 检查订单列表
function order() {

    // 进度条
    !function () {
        var thead = $("tradeRecordsIndex").getElement("thead");
        new Element("tr", {
            "html": "<td colspan=\"9\" class=\"progress-bg\"><div class=\"progress-bar\"></div></td>"
        }).inject(thead);
        Config.Progress = thead.getElement(".progress-bar");
    }();

    var list = $$("#tradeRecordsIndex tbody tr");
    Config.Total = list.length;
    Config.Count = 0;

    list.each(function (tr) {
        var data = new Object();
        Object.forEach(dom, function (value, key) {
            if (!data) return;
            var obj = tr.getElement(value.selector);
            if (!obj) {
                data = null;
                return;
            }
            var text = obj.get("text").trim();
            if (!value.regex.test(text)) {
                data = null;
                return;
            }
            data[key] = value.regex.exec(text)[1];
        });

        if (!data) {
            tr.dispose();
            Config.Count++;
            return;
        }
        var action = tr.getElement(".action");

        var order = localStorage.getItem(KEY) || new Object();

        //account=qss2262@163.com&orderid=vip9988|20161024200040011100250063742198&tradeno=20161024200040011100250063742198&amount=100.00&fee=0.00&sign=3239b3ec2e40a07099c75efa28456eed
        data["orderid"] = data["title"] + "|" + data["tradeno"];
        data["fee"] = "0.00";
        data["account"] = Config["Alipay"];

        var sign = [data["account"], data["orderid"], data["tradeno"], data["amount"], data["fee"], Config["Key"]].join("|");
        data["sign"] = hex_md5(sign);

        if (order[data.tradeno]) {
            orderShow.apply(action, [order[data.tradeno]]);
        } else {
            orderSend.apply(action, [data]);
        }
    });
};

// 显示进度条
function progress() {
    Config.Progress.setStyle("width", (Config.Count / Config.Total * 100) + "%");
    if (Config.Count == Config.Total) {
        (function () {
            var link = $$(".global-logo a").getLast();
            if (link) link.click();
        }).delay(20 * 1000 + Math.random() * 10000);
    }
}

// 显示返回结果
function orderShow(result) {
    var action = this;
    if (result.success) {
        action.set("html", "<label style=\"color:green;\">" + result.msg + "</label>");
    } else {
        action.set("html", "<label style=\"color:red;\">" + result.msg + "</label>");
    }
};

// 添加一个手动发送按钮
function addLink(link) {
    var action = this;
    new Element("a", {
        "href": link,
        "target": "_blank",
        "text": "补发"
    }).inject(action);
    Config.Count++;
    progress();
}


// 发送订单
function orderSend(data) {
    var action = this;
    action.empty();

    var url = "//a8.to/Plugin/Alipay.ashx?_gateway=" + Config["Gateway"];
    // 手动发送通知的地址
    var manage = "http://" + Config["Gateway"] + "/handler/payment/AlipayAccount?" + Object.toQueryString(data);

    try {
        new Request.JSON({
            "url": url,
            "onRequest": function () {
                action.set("text", "正在检测...");
            },
            "onComplete": function () {
                console.log("完成");
            },
            "onError": function (text) {
                action.set("text", text);
                addLink.apply(action, [manage]);
            },
            "onFailure": function (xhr) {
                action.set("text", xhr.statusText);
                addLink.apply(action, [manage]);
            },
            "onSuccess": function (result) {
                orderShow.apply(action, [result]);
                addLink.apply(action, [manage]);
            }
        }).post(data);
    } catch (ex) {
        action.set("text", ex.message);
        addLink.apply(action, [manage]);
    }
}