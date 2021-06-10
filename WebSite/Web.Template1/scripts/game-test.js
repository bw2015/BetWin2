

(function () {
    // 加载彩票
    var lottery = function () {
        var game = location.href.get("lottery");
        if (!game) return;

        var count = 0;
        var timer = setInterval(function () {
            count++;
            var item = $$("[data-name=" + game + "]").getLast();
            if (item != null || count > 200) {
                clearInterval(timer);
                if (item) item.click();
            }
        }, 50);
    };

    var frame = function () {
        var game = location.href.get("frame");
        if (!game) return;
        var count = 0;
        var timer = setInterval(function () {
            count++;
            var list = $$("#frame-lottery [data-name]");
            var item = $$("[data-frame-type=" + game + "]").getLast();
            if ((list.length > 0 && item != null) || count > 20) {
                clearInterval(timer);
                if (item) item.click();
            }
        }, 500);
    }

    lottery.apply();
    frame.apply();

})();