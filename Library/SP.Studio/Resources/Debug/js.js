$import("UI.Diag.js");
var pageSize = 20;
var recordCount = 0;
var pageIndex = 1;

var Condition = {
    Level: null,
    Status: null,
    Key: null,
    PageIndex : 0
};

// 状态
var Status = {
    unfix: "未解决",
    approval: "待审核",
    fix: "已解决"
}

// 级别
var Level = {
    Low: "低",
    Middle: "中",
    High: "高",
    Urgent: "紧急",
    Serious: "严重"
}

// 当前登录的用户信息
var UserInfo = {
    ID: 0,
    Name: null,
    IsDev: true,
    IsTester: false,
    IsAdmin: false,
    load: function (user) {
        this.ID = user.ID;
        this.Name = user.Name;
        var type = user.Type.split(',');
        this.IsDev = type.contains("Dev");
        this.IsTester = type.contains("Tester");
        this.IsAdmin = type.contains("Admin");
    }
};



function loading(obj, isLoad) {
    if (isLoad === undefined) isLoad = true;
    if (isLoad) {
        switch (obj.get("tag")) {
            case "tbody":
                obj.fade("hide");
                var col = obj.getParent("table").getElements("thead > tr > th").length;
                obj.set("html", "<tr><td colspan=\"" + col + "\" class=\"loading\"><label>正在加载...</label></td></tr>");
                obj.fade("in");
                break;
        }
    } else {
        switch (obj.get("tag")) {
            case "tbody":
                obj.set("html", "");
                break;
        }
    }
}

// 系统通用函数
var Debug = {
    // 系统初始化载入
    init: function () {
        var diag = new UI.Diag({
            "type": "html",
            "showtitle": false,
            "width": 100,
            "height": 36,
            "content": "<div class=\"loading\">正在加载...</div>"
        });
        new Request({
            "url": "?ac=user",
            "onSuccess": function (response) {
                var list = JSON.decode(response);
                if (list.length === 0) {
                    diag.close();
                    User.init();
                    User.show();
                } else {
                    new Request({
                        "url": "?ac=userinfo",
                        "onSuccess": function (response) {
                            var user = JSON.decode(response);
                            if (user === null) {
                                diag.getBody().set("html", "");
                                var diagObj = diag.getBody().getParent(".UI-diag");
                                var top = diagObj.getStyle("top").toInt();
                                var left = diagObj.getStyle("left").toInt();
                                var width = 200;
                                var height = 150;
                                var fx = new Fx.Morph(diagObj, {
                                    "onComplete": function () {
                                        diag.getBody().set("html", "<div class=\"login\"><div class=\"Title\"><div class=\"Bugicon Midbox\">登录</div></div><div class=\"clear\"></div>" +
                                        "<form action=\"?ac=login\" method=\"post\" id=\"frmLogin\">" +
                                        "<ul><li>名字: <input type=\"text\" name=\"Name\" class=\"txt\" /></li>" +
                                        "<li>密码: <input type=\"password\" name=\"Password\" class=\"txt\" /></li>" +
                                        "<li><input type=\"button\" value=\" 登录 \" id=\"btnLogin\" /></li></ul>" +
                                        "</form></div>");
                                        $("frmLogin").set("send", {
                                            "onSuccess": function (response) {
                                                var user = JSON.decode(response);
                                                if (user === null) {
                                                    alert("用户名或者密码错误");
                                                } else {
                                                    UserInfo.load(user);
                                                    diag.close();
                                                    Item.init();
                                                    ItemSave.init();
                                                    User.init();
                                                    ItemInfo.init();
                                                    Item.show();
                                                }
                                            }
                                        });
                                        $("btnLogin").addEvent("click", function () {
                                            $("frmLogin").send();
                                        });
                                    }
                                });
                                fx.start({
                                    "width": width,
                                    "height": height,
                                    "left": left - (width - 100) / 2,
                                    "top": top - (height - 36) / 2
                                });
                            } else {
                                UserInfo.load(user);
                                diag.close();
                                Item.init();
                                ItemSave.init();
                                User.init();
                                ItemInfo.init();
                                Item.show();
                            }
                        }
                    }).send();
                }
            }
        }).send();
    },
    show: function (type) {
        $$(".Solution").each(function (item) {
            if (item.get("id") === type) {
                item.setStyle("display", "block");
                item.fade("hide");
                item.fade("in");
            } else {
                item.setStyle("display", "none");
            }
        });
        this.bindEvent();
    },
    bindEvent: function () {
        $$(".list > tbody > tr").each(function (item) {
            item.removeEvents();
            item.addEvents({
                "mouseover": function () { this.addClass("over"); },
                "mouseout": function () { this.removeClass("over"); }
            });
        });

        $$(".post > tbody > tr > th , .post > tbody > tr > td , .post > tfoot > tr > th , .post > tfoot > tr > td").each(function (item) { item.addClass("w"); });
    }
}

// 用户管理
var User = {
    // 初始化
    init: function () {
        $("btnUser").addEvent("click", function () {
            var form = this.getParent("form");
            form.send();
        });
        $$("a:contains('成员管理')").addEvent("click", function () {
            User.show();
        });
        $$("#User form")[0].set("send", {
            "onSuccess": function (result) {
                if (result.toInt() === 0) {
                    alert("用户名已经存在！");
                } else {
                    alert("用户添加成功！");
                    if ($$("#User .list tbody tr").length === 0) {
                        location.reload();
                    }
                    User.show();
                }
            }
        });
    },
    // 加载用户列表
    show: function () {
        if (UserInfo !== 0 && !UserInfo.IsAdmin) {
            alert("您没有权限");
            return;
        }
        Debug.show("User");
        var tbody = $$("#User .list > tbody")[0];
        loading(tbody);
        new Request({
            "url": "?ac=user",
            "method": "get",
            "onSuccess": function (response) {
                loading(tbody, false);
                var list = JSON.decode(response);
                if (list.length === 0) {
                    alert("您是第一次进入本系统，请设置一个帐号。 这个帐号将会自动成为系统管理员。");
                }
                list.each(function (item) {
                    new Element("tr", {
                        "class": "center",
                        "html": "<td>" + item.Name + "</td><td class=\"Password\">" + item.Password + "</td><td class=\"Email\">" + item.Email + "</td><td class=\"Mobile\">" + item.Mobile + "</td>" +
                        "<td>" + item.CreateAt.substring(0, item.CreateAt.indexOf(' ')) + "</td><td class=\"cbox\">" + item.TypeForm + "</td><td><a href=\"javascript:\" class=\"delete\">删除</a></td>"
                    }).inject(tbody);
                });
                var checked = tbody.getElement("input[value=4]");
                if (checked.get("checked")) {
                    checked.set("disabled", true);
                }
                tbody.getElements("a.delete").addEvent("click", function () {
                    var tr = this.getParent("tr");
                    var name = tr.getElement("td").get("text");
                    new Request({
                        "url": "?ac=deleteuser",
                        "data": { Name: name },
                        "onSuccess": function (result) {
                            if (result === 0) {
                                alert("至少要有一个用户是管理员");
                            } else {
                                tr.fade("out");
                                (function () { tr.dispose(); }).delay(300);
                            }
                        }
                    }).send()
                });
                tbody.getElements(".Email , .Mobile , .Password").addEvent("dblclick", function () {
                    var td = this;
                    var value = td.get("text");
                    td.set("text", "");
                    var input = new Element("input", {
                        "type": "text",
                        "value": value,
                        "class": "txt",
                        "styles": {
                            "width": td.getSize().x - 20
                        },
                        "events": {
                            "blur": function () {
                                var name = td.getParent("tr").getElement("td").get("text");
                                value = this.get("value");
                                input.dispose();
                                new Request({
                                    "url": "?ac=updateuser",
                                    "data": { Name: name, Property: td.get("class"), value: value },
                                    "onSuccess": function (result) {
                                        td.set("text", value);
                                    }
                                }).send();
                            }
                        }
                    });
                    input.inject(td);
                    input.select();
                });
                tbody.getElements("input[type=checkbox]").addEvent("click", function () {
                    var cbox = this;
                    var name = cbox.getParent("tr").getElement("td").get("text");
                    var value = cbox.get("value").toInt() * (cbox.get("checked") ? 1 : -1);
                    cbox.fade("hide");
                    new Request({
                        "url": "?ac=updateusertype",
                        "data": { Value: value, Name: name },
                        "onSuccess": function (response) {
                            cbox.fade("in");
                        }
                    }).send()
                });
                Debug.bindEvent();
            }
        }).send()
    },
    // 获取所有的开发者
    getDevr: function (callback) {
        new Request({
            "url": "?ac=user",
            "method": "get",
            "onSuccess": function (response) {
                var list = JSON.decode(response).filter(function (item) { return item.Type.contains("Dev"); });
                if (callback !== undefined) callback(list);
            }
        }).send();
    }
};

// Bug 项目
var Item = {
    // 记录总数
    RecordCount: null,
    // 查询条件
    Condition: {
        Level: null,
        Status: null,
        Key: null,
        PageIndex: 1,
        PageSize: 20
    },
    // 初始化
    init: function () {
        Item.count();
        $$(".Count > a").addEvent("click", function () {
            var status = this.get("class").substring(this.get("class").indexOf('-') + 1);
            Item.Condition.Status = status;
            Item.show();
        });

        $("btnSearch").addEvent("click", function () {
            var frm = this.getParent();
            Item.Condition.Status = frm.getElement("select[name=Status]").get("value");
            Item.Condition.Level = frm.getElement("select[name=Level]").get("value");
            Item.Condition.Key = frm.getElement("input[name=Key]").get("value");
            Item.loadPage(1);
        });
    },
    // 获取统计信息
    count: function () {
        new Request({
            "url": "?ac=count",
            "method": "get",
            "onSuccess": function (respnse) {
                var count = JSON.decode(respnse);
                $$(".Count > a > .num").each(function (item, index) {
                    item.set("text", count["Item" + (index + 1)]);
                });
            }
        }).send();
    },
    // 显示debug项目
    show: function () {
        Debug.show("Item");
        this.count();
        $$("#Item .SearchBox .search select , #Item .SearchBox .search input").each(function (item) {
            var name = item.get("name");
            if (Item.Condition[name] !== undefined) {
                var isValue = true;
                switch (name) {
                    case "Status":
                        if (Status[Item.Condition[name]] === undefined) isValue = false;
                        break;
                    case "Level":
                        if (Level[Item.Condition[name]] === undefined) isValue = false;
                        break;
                }
                if (isValue) item.set("value", Item.Condition[name]);
            }
        });
        this.loadPage(1);
    },
    // 分页代码
    pageSplit: function () {
        var pageIndex = this.Condition.PageIndex;
        var recordCount = this.RecordCount;
        var obj = $("pageSplit");

        var maxPage = recordCount % pageSize === 0 ? recordCount / pageSize : Math.floor(recordCount / pageSize) + 1;
        var page = new Array();
        for (var i = 1; i <= maxPage; i++) {
            if (this.Condition.PageIndex === i) {
                page.push("<span class=\"current\">" + i + "</span>");
            } else {
                page.push("<a href=\"javascript:Item.loadPage(" + i + ");\">" + i + "</a>");
            }
        }
        obj.set("html", page.join(" "));
    },
    // 加载信息
    loadPage: function (index) {
        var tbody = $$("#Item .list tbody")[0];
        loading(tbody);
        this.Condition.PageIndex = index;
        new Request({
            "url": "?ac=list",
            "data": Item.Condition,
            "onSuccess": function (response) {
                Item.RecordCount = response.substring(0, response.indexOf(';')).toInt();
                Item.pageSplit();
                response = response.substring(response.indexOf(';') + 1);
                loading(tbody, false);
                var list = JSON.decode(response);
                list.each(function (item) {
                    new Element("tr", {
                        "html": "<td><a href=\"javascript:ItemInfo.show('" + item.ID + "');\">" + item.Title + "</a></td>" +
                                "<td class=\"w " + item.Level + "\">" + Level[item.Level] + "</td>" +
                                "<td class=\"w " + item.Status + "\">" + Status[item.Status] + "</td>" +
                                "<td class=\"w\">" + item.Owner + "</td>" +
                                "<td class=\"w\">" + item.CreateAt + "</td>"
                    }).inject(tbody);
                });

                Debug.bindEvent();
            }
        }).send();
    }
}

// 添加或者修改Bug项目信息
var ItemSave = {
    init: function () {
        $$("a.button-create").addEvent("click", function () {
            ItemSave.show();
        });

        $("frmItemSave").set("send", {
            "onSuccess": function (response) {
                Item.show();
            }
        });

        $("btnItemSave").addEvent("click", function () {
            $("txtContent").set("value", KE.html('txtContent'));
            if ($("frmItemSave").getElement("input[name=Title]").get("value") === "") {
                alert("请输入标题！");
                return;
            }
            if ($("frmItemSave").getElement("textarea[name=Content]").get("value") === "") {
                alert("请输入内容！");
                return;
            }
            $("frmItemSave").send();
        });
    },
    show: function (itemID) {
        Debug.show("ItemSave");
        new Request({
            "url": "?ac=iteminfo",
            "data": { ID: itemID },
            "onSuccess": function (response) {
                var item = JSON.decode(response);
                KE.html("txtContent", item.Content);
                $$("#frmItemSave input , #frmItemSave select").each(function (input) {
                    var name = input.get("name");
                    if (item[name] !== undefined) {
                        input.set("value", item[name]);
                    }
                    if (name === "Name" && input.get("value") === "") {
                        input.set("value", UserInfo.Name);
                    }
                });
                var owner = item.Owner.split(",");
                User.getDevr(function (list) {
                    var dev = $$("#ItemSave .dever")[0];
                    var html = new Array();
                    list.each(function (user) {
                        html.push("<input type=\"checkbox\" name=\"Owner\" value=\"" + user.Name + "\"" + (owner.contains(user.Name) ? " checked" : "") + " />" + user.Name);
                    });
                    dev.set("html", html.join("&nbsp;"));
                });

            }
        }).send();
    }
}

// 项目信息
var ItemInfo = {
    init: function () {
        $("frmFollow").set("send", {
            "onSuccess": function (response) {
                var id = $("frmFollow").getElement("input[name=ID]").get("value");
                ItemInfo.show(id);
            }
        });
        $("btnFollow").addEvent("click", function () {
            var frm = this.getParent("form");
            var checked = false;
            $("txtDescription").set("value", KE.html("txtDescription"));
            frm.getElements("input[name=Status]").each(function (item) {
                if (item.get("checked")) checked = true;
            });
            if (!checked) { alert('请选择状态'); return; }

            frm.send();
        });
        $("lnkDelete").addEvent("click", function () {
            if (!confirm("确认删除吗？")) return;
            var id = $("frmFollow").getElement("input[name=ID]").get("value");
            new Request({
                "url": "?ac=itemdelete",
                "data": { ID: id },
                "onSuccess": function () {
                    Item.show();
                }
            }).send();
        });
        $("lnkEdit").addEvent("click", function () {
            var id = $("frmFollow").getElement("input[name=ID]").get("value");
            ItemSave.show(id);
        });
    },
    show: function (itemID) {
        Debug.show("ItemInfo");
        $("ItemInfo").getElement("input[name=ID]").set("value", itemID);
        var ftoot = $("ItemInfo").getElement(".post > tfoot");
        if (!UserInfo.IsTester && !UserInfo.IsDev) ftoot.setStyle("display", "none");
        new Request({
            "url": "?ac=iteminfo",
            "data": { ID: itemID },
            "onSuccess": function (response) {
                var item = JSON.decode(response);
                if (item.Status === "fix") ftoot.setStyle("display", "none");
                $$("#ItemInfo table tr td div").each(function (obj) {
                    var name = obj.get("title");
                    if (item[name] !== undefined) {
                        if (name === "Level") item[name] = Level[item[name]];
                        if (name === "Status") item[name] = Status[item[name]];
                        obj.set("html", item[name]);
                    }
                });
            }
        }).send();
        var tbody = $$("#ItemInfo .Follow table tbody")[0];
        loading(tbody);
        new Request({
            "url": "?ac=loglist",
            "data": { ID: itemID },
            "onSuccess": function (response) {
                loading(tbody, false);
                var list = JSON.decode(response);
                list.each(function (item, index) {
                    new Element("tr", {
                        "html": "<td style=\"width:100px;\">&nbsp;<span class=\"" + item.Status + "\">" + Status[item.Status] + "</span></td>" +
                                "<td style=\"width:150px;\">&nbsp;" + item.Name + "</td>" +
                                "<td>&nbsp;" + item.Description + "</td>" +
                                "<td style=\"width:150px;\">&nbsp;" + item.CreateAt + "</td>"
                    }).inject(tbody);
                    if (index < list.length - 1) {
                        new Element("tr", {
                            "html": "<td colspan=\"4\" class=\"next\"></td>"
                        }).inject(tbody);
                    }
                });
                Debug.bindEvent();
            }
        }).send();
    }
}

window.addEvent("domready", function () {
    Debug.init();


    KE.show({ id: 'txtContent' });
    KE.show({ id: 'txtDescription' });
});