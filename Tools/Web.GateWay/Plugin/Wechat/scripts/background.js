var isEvent = false;
var version = "1.0";
var url_match = "/cgi-bin/mmwebwx-bin/webwxsync?sid=";
const store_order = "BETWIN-WX-ORDER";
const store_config = "BETWIN-WX-CONFIG";
var ORDER = null;

chrome.webRequest.onBeforeSendHeaders.addListener(
    function (request) {
        if (!isEvent) {
            isEvent = true;
            chrome.debugger.attach({ //debug at current tab
                tabId: request.tabId
            }, version, onAttach.bind(null, request.tabId));
        }

        request.requestHeaders.push({
            "name": "Content-type",
            "value": "charset=utf-8"
        });
        return { requestHeaders: request.requestHeaders };

    }, { "urls": ["https://wx.qq.com/*", "https://wx2.qq.com/*", "https://web.wechat.com/*"] }, ["blocking", "requestHeaders"]);

chrome.webRequest.onHeadersReceived.addListener(
    function (details) {
        if (details.url.indexOf(url_match) < 0) return;
        details.responseHeaders.push({
            "name": "Content-type",
            "value": "text/plain;charset=utf-8"
        });
        return { responseHeaders: details.responseHeaders };
    },
    { "urls": ["https://wx.qq.com/*", "https://wx2.qq.com/*", "https://web.wechat.com/*"] }, ["blocking", "responseHeaders"]);

// 与前台页面交互
chrome.runtime.onMessage.addListener(function (data, sender, callback) {
    var order = GetOrder();
    switch (data["action"]) {
        case "order":   //补单
            var item = order[data["systemId"]];
            if (item) saveOrder(item["money"], item["name"], data["systemId"]);
            break;
        default:
            callback(order);
            break;
    }
});

function onAttach(tabId) {

    chrome.debugger.sendCommand({ //first enable the Network
        tabId: tabId
    }, "Network.enable");

    chrome.debugger.onEvent.addListener(allEventHandler);
}

function allEventHandler(debuggeeId, message, params) {
    if (!params.response || params.response.url.indexOf(url_match) < 0) {
        return;
    }
    if (message == "Network.responseReceived") { //response return 
        chrome.debugger.sendCommand({
            tabId: debuggeeId.tabId
        }, "Network.getResponseBody", {
                "requestId": params.requestId
            }, function (response) {
                if (!response) return;
                var body = response.body;
                var data = JSON.decode(body);
                if (!data) {
                    console.log("无法转化成为json" + body);
                    return;
                }
                if (!data.AddMsgList) {
                    console.log("无法找到消息对象" + body);
                    return;
                }
                data.AddMsgList.each(function (item) {
                    var content = item.Content;
                    var systemId = item.MsgId;
                    var type = item.MsgType;
                    //收款金额￥1.00<br/>付款方备注is74119
                    //收款金额￥0.02<br/>付款方备注ceshi01
                    var regex1 = /收款金额：￥([0-9\.]+)<br\/>付款方备注：(.+?)<br\/>/i;
                    if (!regex1.test(content)) {
                        regex1 = /收款金额￥([0-9\.]+)<br\/>付款方备注(.+?)<br\/>/i;
                    }
                    if (!regex1.test(content) || type != 49 || !systemId) {
                        console.log(item);
                        return;
                    }
                    var exec1 = regex1.exec(content);
                    var money = exec1[1];
                    var name = exec1[2];
                    if (systemId.length > 50) systemId = systemId.substr(0, 50);
                    console.log("金额：" + money + "，备注：" + name + "，系统编号：" + systemId);
                    saveOrder(money, name, systemId);
                });
            });
    }
}

// 获取配置参数
function GetConfig() {
    var content = localStorage.getItem(store_config) || "{}";
    return JSON.decode(content);
};

// 保存配置参数
function SetConfig(data) {
    var content = JSON.encode(data);
    localStorage.setItem(store_config, content);
}

// 获取当前的订单状态
function GetOrder() {
    var content = localStorage.getItem(store_order);
    if (!content) return new Object();
    var data = JSON.decode(content) || {};
    return data;
}

// 把订单保存至localstore
function saveOrderToStore() {
    if (!ORDER) return;
    // 只保存最新的10条订单
    var keys = Object.keys(ORDER);
    for (var index = 0; index < keys.length - 5; index++) {
        delete ORDER[keys[index]];
    }
    var content = JSON.encode(ORDER);
    localStorage.setItem(store_order, content);
}

// 保存订单
function saveOrder(money, name, systemId) {
    if (!ORDER) ORDER = GetOrder();
    var info = ORDER[systemId];
    if (info && info["status"] == "success") return;
    ORDER[systemId] = info = {
        "money": money,
        "name": name,
        "systemId": systemId
    };
    var config = GetConfig();

    if (!config["Gateway"]) {
        info["msg"] = "接口未配置";
        saveOrderToStore();
    }
    var gateway = "http://" + config["Gateway"] + "/handler/payment/AlipayAccount";

    //account=qss2262@163.com&orderid=vip9988|20161024200040011100250063742198&tradeno=20161024200040011100250063742198&amount=100.00&fee=0.00&sign=3239b3ec2e40a07099c75efa28456eed
    var post = {
        "account": config["Alipay"],
        "orderid": name + "|" + systemId,
        "tradeno": systemId,
        "amount": money,
        "fee": "0.00",
        "remark": "微信扫码"
    };
    var sign = [post["account"], post["orderid"], post["tradeno"], post["amount"], post["fee"], config["Key"]].join("|");
    post["sign"] = hex_md5(sign);
    info["url"] = gateway + "?" + Object.toQueryString(post);

    new Request.JSON({
        "url": gateway,
        "onRequest": function () {
            info["msg"] = "正在提交";
            saveOrderToStore();
        },
        "onSuccess": function (result) {
            info["msg"] = result.msg;
            if (result.success) {
                info["status"] = "success";
            } else {
                info["status"] = "faild";
            }
            saveOrderToStore();
        }
    }).post(post);
}