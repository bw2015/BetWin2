var form = document.getElementById('form');
var https = /:443$/;

// 判断获取到的域名
!function () {
    var site = window["SITE"];
    if (!site) return;


    document.title = site.Name;
    var domainlist = site.Domain;

    var website = $("website");
    site.Domain.each(function (item) {
        new Element("a", {
            "href": (https.test(item) ? "https://" : "http://") + item,
            "target": "_blank",
            "data-domain": item,
            "html": "<label class=\"loading\"></label><span>" + item.replace(https, "") + "</span>"
        }).inject(website);
    });

    testSpeed();
}();

// 测速
function testSpeed() {
    var website = $("website");
    var logo = $("logo");
    var time = new Object();
    var obj = null;

    website.getElements("[data-domain]").each(function (item) {
        var domain = item.get("data-domain");
        var label = item.getElement("label");
        var protocol = https.test(domain) ? "https://" : "http://";
        domain = domain.replace(https, "");
        var url = protocol + domain + "/handler/system/config/ping";
        new Request({
            "method": "OPTION",
            "url": url,
            "onRequest": function () {
                item.set("class", null);
            },
            "onLoadstart": function () {
                time[domain] = new Date().getTime();
            },
            "onFailure": function (event, xhr) {
                item.set("class", "error");
            },
            "onProgress": function (event, xhr) {
                var t = new Date().getTime() - time[domain];
                label.removeClass("loading");
                var speed = Math.floor(t / 20);
                label.set("text", speed + "ms");
                if (speed < 20) {
                    item.set("class", "good");
                } else if (speed < 100) {
                    item.set("class", "normal");
                } else {
                    item.set("class", "low");
                }
                if (obj) {
                    item.inject(obj, "after");
                } else {
                    item.inject(website, "top");
                    logo.setStyle("background-image", "url('" + protocol + domain + "/images/logo.png')");
                }
                obj = item;
            }
        }).send();
    });

    !function () {
        var form = $("form");
        if (!form) return;
        form.set("send", {
            "onSuccess": function (result) {
                var result = JSON.decode(result);
                form.getParent().removeClass("loading");
                if (!result.success) {
                    alert(result.msg || "域名不存在");
                } else {
                    alert(result.msg);
                }
            }
        });
        form.addEvent("submit", function (e) {
            e.stop();
            form.getParent().addClass("loading");
            form.send();
        });
    }();

    !function () {
        $$("[data-dom]").each(function (item) {
            switch (item.get("data-dom")) {
                case "domain":
                    item.set("text", location.host);
                    break;
                case "name":
                    item.set("text", SITE.Name);
                    break;
                case "siteid":
                    item.set("value", SITE.ID);
                    break;
            }
        });
    }();
};