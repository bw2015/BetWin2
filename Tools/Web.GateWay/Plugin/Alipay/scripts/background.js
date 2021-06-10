//获取配置值

var KEY = "BETWIN 2.0";
var ORDER_KEY = "BETWIN-ALIPAY-ORDER";
var ORDER = null;

function GetConfig() {
    var config = new Object();
    var betwin = localStorage.getItem(KEY);
    if (betwin) config = JSON.decode(betwin);
    return config;
}

function SetConfig(config) {
    localStorage.setItem(KEY, JSON.encode(config));
}

function GetOrder() {
    if (!ORDER) return ORDER;
    var order;
    var betwin = localStorage.getItem(ORDER_KEY);
    if (betwin) order = JSON.decode(betwin);
    if (!order) order = new Object();
    return order;
}

function SaveOrder() {
    localStorage.setItem(ORDER_KEY, JSON.encode(ORDER));
}

// 保存订单信息
function saveOrder(data, callback) {
    ORDER = GetOrder();
    var id = data["id"];
    if (ORDER[id]) {
        callback(ORDER[id]);
        return;
    }
    var config = GetConfig();
    var url = "http://" + config["Gateway"] + "/handler/payment/AlipayAccount";
    new Request.JSON({
        "url": url,
        "onSuccess": function (result) {
            ORDER[id] = result;
            SaveOrder();
        }
    }).post(data);

}

// 获取本地存储的信息
chrome.runtime.onMessage.addListener(function (data, sender, callback) {
    if (!data) data = {};
    var config = GetConfig();

    switch (data["action"]) {
        case "order":
            saveOrder(data, callback);
            break;
        default:
            callback(config);
            break;
    }
});

