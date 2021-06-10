/// <reference path="game.js" />
// 个人资料管理

// 账户资金
BW.callback["info-person-account-money"] = function () {
    var t = this;
    var table = t.dom.container.getElement(".info-person-account-money");
    if (table == null) return;

    table.empty();
    var thead = new Element("thead");
    var tit = ["主账户", "锁定金额"];
    tit.push("总金额");
    var head = new Element("tr");
    head.inject(thead);
    tit.each(function (name) {
        new Element("th", {
            "text": name
        }).inject(head);
    });
    thead.inject(table);

    var tbody = new Element("tbody");
    var money = [Setting.User.Money, Setting.User.LockMoney];
    money.push(Setting.User.TotalMoney);
    var body = new Element("tr");
    body.inject(tbody);
    money.each(function (item) {
        item = item.toFloat();
        new Element("td", {
            "html": "<span class=\"red\">" + (item.ToString("c")) + "元</span>"
        }).inject(body);
    });
    tbody.inject(table);

}

// 个人资料
BW.callback["info-person-account-info"] = function (result) {
    var t = this;
    t.dom.element.getElements("[data-type]").each(function (item) {
        var type = item.get("data-type");
        if (type == "Session") {
            item.set("html", "<span class=\"gray\">" + Setting.User.Session + "</span>");
            return;
        }
        if (!result.info[type]) {
            item.set("html", "<span class=\"red\">暂未绑定</span>");
            return;
        }
        var bind = result.info[type].Time + "修改";
        item.set("html", "<span class=\"gray ft12\">" + bind + "</span>");
    });
}