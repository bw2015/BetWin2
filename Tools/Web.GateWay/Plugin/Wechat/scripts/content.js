var KEY = "BETWIN-WX-ORDER";
var port = chrome.runtime.connect({ name: KEY });   //通道名称

window.addEvent("domready", function () {
    //var bg = chrome.extension.getBackgroundPage();
    var plan;

    // 创建面板
    !function () {
        plan = new Element("div", {
            "class": "wx-plan",
            "html": "<header>微信到帐通知</header><content><table><thead><tr><th>金额</th><th>备注</th><th>状态</th><th>信息</th><th>补单</th></tr></thead><tbody></tbody></table></content>"
        });
        plan.inject(document.body);
    }();

    var loadOrder = function () {
        var body = plan.getElement("tbody");
        try {
            chrome.extension.sendMessage({}, function (result) {
                Object.forEach(result, function (value, key) {
                    value.status = value.status || "N/A";
                    var tr = body.getElement("tr[id=" + key + "]");
                    if (tr) {
                        if (tr.get("data-status") == value.status) {
                            return;
                        } else {
                            tr.dispose();
                        }
                    }
                    tr = new Element("tr", {
                        "id": key,
                        "data-status": value.status
                    });
                    var msg = "订单编号：" + key + "\n\r信息：" + value["msg"];
                    new Element("td", { "text": value.money }).inject(tr);
                    new Element("td", { "text": value.name }).inject(tr);
                    new Element("td", { "text": value.status, "class": value.status }).inject(tr);
                    new Element("td", { "html": "<a href=\"javascript:alert('" + msg + "');\">信息</a>" }).inject(tr);
                    new Element("td", { "html": "<a href=\"" + value.url + "\" target=\"_blank\">手动</a>" }).inject(tr);
                    tr.inject(body, "top");
                });
            });
        } catch (ex) {
            console.log(ex);
        }
        console.log("重复读取开始");
        loadOrder.delay(3000);
    };
    loadOrder();
});

