var LANGUAGE = null;
var TBODY = null;

window.addEvent("domready", function () {
    new Request.JSON({
        "url": "",
        "noCache": true,
        "onSuccess": function (result) {
            LANGUAGE = result.info.data;
            TBODY = $("tbody");
            Object.forEach(LANGUAGE, function (value, key) {
                new Element("th", { "text": value }).inject($("thead"));
                new Element("td", { "text": "" }).inject($("tfoot"));
            });
            Object.forEach(result.info.list, function (value, key) {
                BuildItem(key, value);
            });
            BindEvent();
        }
    }).get({
        "ac": "list"
    });

    $("btnSave").addEvent("click", function () {
        var btn = this;
        new Request.JSON({
            "url": "?ac=file",
            "onRequest": function () {
                btn.set("disabled", true);
            },
            "onComplete": function () {
                btn.set("disabled", false);
            }
        }).post();
    });
});


// 新建一行
function BuildItem(key, value) {
    if (!value) value = {};
    console.log(key);
    var tr = TBODY.getElement("[data-key=" + key + "]");
    if (tr) {
        tr.inject(TBODY, "bottom");
        return;
    }
    tr = new Element("tr", {
        "data-key": key,
        "html": "<th></th>"
    });
    new Element("td", {
        "html": "<div class=\"edit\">" + key + "</div>"
    }).inject(tr);

    Object.forEach(LANGUAGE, function (languageValue, type) {
        new Element("td", {
            "html": "<div class=\"edit\" data-type=\"" + type + "\">" + (value[type] || "") + "</div>"
        }).inject(tr);
    });
    tr.inject(TBODY);

}

function BindEvent(tr) {
    $("language").addEvent("click", function (e) {
        var edit = $(e.target);
        if (!edit.hasClass("edit")) return;
        var action = edit.get("data-action") || "edit";

        var td = edit.getParent("td");
        var value = edit.get("text");
        var tr = td.getParent();
        var input = new Element("input", {
            "type": "text",
            "value": value,
            "name": edit.get("data-type") || "key",
            "data-value": value,
            "events": {
                "focus": function () {
                    this.select();
                    var tr = this.getParent("tr");
                    tr.addClass("selected");
                },
                "change": function () {
                    var tr = this.getParent("tr");
                    var value = this.get("value");
                    switch (action) {
                        case "new":
                            AddNewKey.apply(this, [value]);
                            break;
                        case "edit":
                            UpdateValue.apply(this, [tr.get("data-key"), value]);
                            break;
                    }
                    this.blur();
                },
                "blur": function () {
                    tr.removeClass("selected");
                    var value = this.get("value");
                    switch (action) {
                        case "new":
                            edit.set("text", "");
                            break;
                        case "edit":
                            edit.set("text", value);
                            break;
                    }
                    edit.inject(this, "after");
                    this.dispose();
                }
            }
        });
        input.inject(edit, "after");
        input.focus();
        edit.dispose();
    });
}

// 添加一个新的KEY值
function AddNewKey(key) {
    var input = this;
    BuildItem(key);
}

function UpdateValue(key, value) {
    var input = this;
    var type = input.get("name");
    if (type == "key") {
        if (!value) DeleteKey.apply(this, [input.get("data-value")]);
        return;
    }
    new Request.JSON({
        "url": "?ac=save",
        "onRquest": function () {
            loading();
        },
        "onComplete": function () {
            loading();
        },
        "onSuccess": function (result) {
            $("language").removeClass("disable");
        }
    }).post({
        "Language": type,
        "Key": key,
        "value": value.trim()
    });
}

// 删除一个Key值
function DeleteKey(key) {
    var input = this;
    var tr = this.getParent("tr");
    new Request.JSON({
        "url": "?ac=delete",
        "onRequest": function () {
            loading();
        },
        "onComplete": function () {
            loading();
        },
        "onSuccess": function (result) {
            tr.dispose();
        }
    }).post({
        "Key": key.trim()
    });
}


function loading() {
    $("language").toggleClass("disable");
}