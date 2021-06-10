// 工具类
var Config = null;
var KEY = "BETWIN-ORDER";
if (!window["Utils"]) window["Utils"] = new Object();

(function (ns) {

    // 已经成功的记录，不再发送
    ns.SUCCESSLOG = new Object();

    var port = chrome.runtime.connect({ name: KEY });   //通道名称

    if (!Config) {
        chrome.extension.sendMessage({}, function (result) {
            Config = result;
        });
    }

    ns["Success"] = function (action, msg) {
        action.addClass("success");
        action.set("text", msg);
    };

    ns["Save"] = function (data, action) {
        var url = "//a8.to/Plugin/Alipay.ashx?_gateway=" + Config["Gateway"];

        var manageUrl = "http://" + Config["Gateway"] + "/handler/payment/AlipayAccount?" + Object.toQueryString(data);
        new Element("a", {
            "href": manageUrl,
            "target": "_blank",
            "text": "补发",
            "style": "color:red"
        }).inject(action, "after");

        var id = data["orderid"];
        if (ns.SUCCESSLOG[id]) {
            ns["Success"](action, "已经入账");
            return;
        }

        new Request.JSON({
            "url": url,
            "onRequest": function () {
                action.addClass("loading");
            },
            "onComplete": function () {
                action.removeClass("loading");
            },
            "onError": function (text) {
                action.set("text", text);
            },
            "onFailure": function (xhr) {
                console.log(xhr);
            },
            "onSuccess": function (result) {
                if (result.success) {
                    ns.SUCCESSLOG[id] = true;
                    ns["Success"](action, result.msg);
                } else {
                    action.set("text", result.msg);
                }
            }
        }).post(data);
    };

    // 签名并且返回当前对象
    ns["Sign"] = function (data) {
        //var sign = [data["account"], data["orderid"], data["tradeno"], data["amount"], data["fee"], Config["Key"]].join("|");
        var sign = [data["account"], data["orderid"], data["tradeno"], data["amount"], data["fee"], Config["Key"]].join("|");
        data["sign"] = hex_md5(sign);
        return data;
    };

})(Utils);