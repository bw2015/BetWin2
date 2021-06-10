BW.callback["loginbox"] = function (result) {
    var form = this.dom.element;
    var loginbox = form.getParent(".loginbox");
    if (result.success) {
        UI.Sound("login");
        loginbox.addClass("login-success");
        loginbox.fade("out");
        (function () {
            loginbox.dispose();
            location.href = "frame.html";
        }).delay(1000);
    } else {
        UI.Sound("no");
        loginbox.addClass("shake animated infinite login-faild");
        loginbox.removeClass.delay(500, loginbox, ["shake animated infinite login-faild"]);
    }
}

