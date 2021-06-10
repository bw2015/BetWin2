
BW.callback["form"] = function (result) {
    var element = this.dom.element;
    element.getElements("form").each(function (form) {

        form.getElements("img.vcode").each(function (img) {
            updateValidCode(img);
        });
    });
};

// 对于错误的处理
BW.callback["golbal-error"] = function (result) {
    if (!result.info) return;
    switch (result.info.Type) {
        case "Login":
            if (/game\.html/.test(location.href)) {
                $(document.body).fade("out");
                new BW.Tip(result.msg, {
                    "callback": function () {
                        location.href = "index.html";
                    }
                });
            }
            break;
        case "Stop":
            $(document.body).fade("out");
            location.href = "stop.html?" + result.msg;
            break;
    }
};

// 更新验证码
var updateValidCode = function (img) {
    if (!img) return;
    var name = img.get("data-name");
    var src = "/handler/ValidateCode.png?name=" + name + "&r=" + Math.random();
    img.set("src", src);
}

// 公共的登录事件
BW.load["common-login"] = function () {
    var t = this;
    !function () {
        var img = t.dom.element.getElement("img[data-name]");
        if (!img) return;
        updateValidCode(img);
    }();
};

// 公共的登录回调事件
BW.callback["common-login"] = function (result) {
    var t = this;
    if (!result.success) {
        new BW.Tip(result.msg, {
            "callback": function () {
                var img = t.dom.element.getElement("img[data-name]");
                if (img) updateValidCode(img);
            }
        });
        return;
    }

    document.body.fade("out");
    var action = "game.html";
    if (t.dom.element.get("data-login")) action = t.dom.element.get("data-login");
    location.href = action + "?" + new Date().getTime();
};

// 轮换效果
BW.callback["common-banner"] = function (result) {
    var t = this;
    var images = t.dom.element.getElements("img").map(function (item) { return "<a href=\"javascript:\"><img src=\"" + item.get("src") + "\" /></a>"; });
    t.dom.element.set("html", images.join(""));
    if (!UI.Image || !UI.Image.Banner) {
        new BW.Tip("没有引用banner组件");
        return;
    }
    new UI.Image.Banner(t.dom.element, {
        "width": "100%",
        "height": t.dom.element.getHeight(),
        "background": true,
        "timer": t.dom.element.get("data-banner-timer") || 8000
    });

};

window.addEvents({
    "domready": function (e) {

    },
    "click": function (e) {
        var obj = $(e.target);
        // 验证码
        if (obj.hasClass("vcode")) {
            updateValidCode(obj);
        }
    }
});