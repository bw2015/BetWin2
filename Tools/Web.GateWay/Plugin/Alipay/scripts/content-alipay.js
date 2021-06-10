var bg;

var ALIPAY_GET = function () {
    var t = this;
    if (t == null) return;
    console.clear();
    t.getElements("p.memo-info").each(function (item) {
        var tr = item.getParent("tr");
        ALIPAY_GET_INFO.apply(tr);
    });
};

var ALIPAY_GET_INFO = function () {
    var tr = this;
    var status = tr.getElement(".status");
    var action = tr.getElement(".action");
    if (!status.get("text").contains("交易成功")) return;
    var info = {
        "amount": tr.getElement(".amount-pay").get("text").replace(/ /g, ""),
        "name": tr.getElement(".memo-info").get("text"),
        "href": tr.getElement("a.record-icon").get("href")
    };
    if (info.amount.indexOf('+') != 0) return;
    info.amount = info.amount.replace("+", "");
    action.empty();
    tr.addClass("betwin-order");
    info["id"] = /bizInNo=(\w+)/.exec(info.href)[1];
    delete info["href"];
    action.set("html", "<a href=\"\" target=\"_blank\">补单</a>");
    status.set("text", "服务器状态");
    info["action"] = "order";
    chrome.extension.sendMessage(info, function (result) {
        console.log(result);
    });
};

window.addEvent("domready", function () {
    ALIPAY_GET.apply($("tradeRecordsIndex"));
});