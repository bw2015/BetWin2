// 检测订单那支付状态
!function () {
    var orderId = window["ORDERID"];
    if (!orderId) return;
    var time = new Date().getTime();
    var totalTime = 300;
    function setTime() {
        var obj = $("time");
        if (!obj) return;
        var diff = Math.max(0, 300 - (new Date().getTime() - time) / 1000);
        var minute = Math.floor(diff / 60);
        var second = Math.floor(diff) % 60;
        obj.set("text", minute + "分" + second + "秒");
    };
    var interval = setInterval(setTime, 1000);
    var success = false;
    function query() {
        if (success) {
            clearInterval(interval);
            return;
        };
        new Request.JSON({
            "url": "/handler/user/money/rechargequery",
            "onComplete": function () {
                if (new Date().getTime() - time > totalTime * 1000) {
                    alert("支付超时");
                    window.close();
                    document.body.empty();
                    return;
                }
                query.delay(3000);
            },
            "onSuccess": function (result) {
                if (result.info && result.info.IsPayment == "1") {
                    success = true;
                    alert("支付成功");
                    window.close();
                }
            }
        }).post({
            "OrderID": orderId
        })
    }
    query.delay(5 * 1000);
}();